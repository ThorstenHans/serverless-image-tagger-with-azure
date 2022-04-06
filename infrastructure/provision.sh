#!/bin/bash
set -e
location=westeurope
rgName=rg-serverless-image-tagger

fnAppName="fn-serverless-image-tagger"
fnOs=Windows
fnVersion=3
fnRuntime=dotnet
fnRuntimeVersion="3.1"

storageAccountNameImages=saserverlessimagetaggs
storageAccountNameFunctions=saserverlessimagetaggsfn  

az group create -n $rgName -l $location -o none

# exit if one of both is not available
imageStorageAccountNameAvailable=$(az storage account check-name -n $storageAccountNameImages --query 'nameAvailable' -o tsv)
if [ "$imageStorageAccountNameAvailable" = false ] ; then
    echo 'Sorry! Storage Account Name' $storageAccountNameImages 'is already taken'
    exit 1 
fi
functionsStorageAccountNameAvailable=$(az storage account check-name -n $storageAccountNameFunctions --query 'nameAvailable' -o tsv)
if [ "$functionsStorageAccountNameAvailable" = false ] ; then
    echo 'Sorry! Storage Account Name' $storageAccountNameFunctions 'is already taken'
    exit 1 
fi

az storage account create -n $storageAccountNameImages -g $rgName \
  -l $location -o none
az storage account create -n $storageAccountNameFunctions -g $rgName \
  -l $location -o none
echo "Storage Accounts created."

az cognitiveservices account create -n acv-image-tagger \
  -g $rgName --kind ComputerVision \
  --sku F0 --assign-identity \
  -l $location \
  --yes -o none
echo "Azure Computer Visison created."

principalId=$(az cognitiveservices account show -n acv-image-tagger -g $rgName --query identity.principalId -o tsv)
scope=$(az storage account show -n $storageAccountNameImages -g $rgName --query id -o tsv)
echo "Waiting for Identity propagation"
sleep 10s
az role assignment create --scope $scope --assignee $principalId --role "Storage Blob Data Reader" -o none
echo "Azure Role Assignment created"

az monitor log-analytics workspace create -n law-image-tagger \
  -g $rgName \
  -l $location \
  -o none
echo "Log Analytics Workspace created"

lawId=$(az monitor log-analytics workspace show -n law-image-tagger -g $rgName --query id -o tsv)
az monitor app-insights component create -a ai-image-tagger \
  -g $rgName \
  --workspace $lawId \
  -l $location \
  -o none
echo "Application Insights created"

az functionapp create -n $fnAppName -g $rgName \
  -s $storageAccountNameFunctions \
  -c $location \
  --app-insights ai-image-tagger \
  --functions-version $fnVersion \
  --runtime $fnRuntime \
  --os-type $fnOs \
  -o none
echo "Azure Functions app created"

key=$(az cognitiveservices account keys list -n acv-image-tagger -g $rgName --query key1 -o tsv)
endpoint=$(az cognitiveservices account show -n acv-image-tagger -g $rgName --query properties.endpoint -o tsv)
storageAccountConnectionString=$(az storage account show-connection-string -n $storageAccountNameImages -g $rgName --query connectionString -o tsv)

az functionapp config appsettings set -n $fnAppName \
  -g $rgName \
  --settings "ImagesStorageAccount=$storageAccountConnectionString" -o none
  
az functionapp config appsettings set -n $fnAppName \
  -g $rgName \
  --settings "ComputerVision__SubscriptionKey=$key" -o none

az functionapp config appsettings set -n $fnAppName \
  -g $rgName \
  --settings "ComputerVision__Endpoint=$endpoint" -o none

echo "Azure Functions App configured"
echo "Done"
