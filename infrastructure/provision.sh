#!/bin/bash
set -e
location=westeurope
rgName=rg-xmas-tagger

fnAppName="fn-app-xmas-tagger"
fnOs=Windows
fnVersion=3
fnRuntime=dotnet
fnRuntimeVersion="3.1"

storageAccountNameImages=saxmastaggerimages
storageAccountNameFunctions=saxmastaggerfn  

az group create -n $rgName -l $location -o table

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
  -l $location -o table
az storage account create -n $storageAccountNameFunctions -g $rgName \
  -l $location -o table

az cognitiveservices account create -n acv-xmas-tagger \
  -g $rgName --kind ComputerVision \
  --sku F0 --assign-identity \
  -l $location \
  --yes -o table

principalId=$(az cognitiveservices account show -n acv-xmas-tagger -g $rgName --query identity.principalId -o tsv)
appId=$(az ad sp show --id $principalId --query appId -o tsv)
scope=$(az storage account show -n $storageAccountNameImages -g $rgName --query id -o tsv)

az role assignment create --scope $scope --assignee $appId --role "Storage Blob Data Reader"

az monitor log-analytics workspace create -n law-xmas-tagger \
  -g $rgName \
  -l $location \
  -o table

lawId=$(az monitor log-analytics workspace show -n law-xmas-tagger -g rg-xmas-tagger --query id -o tsv)

az monitor app-insights component create -a ai-xmas-tagger \
  -g $rgName \
  --workspace $lawId \
  -l $location \
  -o table

az functionapp create -n $fnAppName -g $rgName \
  -s saxmastaggerfn \
  -c $location \
  --app-insights ai-xmas-tagger \
  --functions-version $fnVersion \
  --runtime $fnRuntime \
  --runtime-version $fnRuntimeVersion \
  --os-type $fnOs \
  -o table

key=$(az cognitiveservices account keys list -n acv-xmas-tagger -g $rgName --query key1 -o tsv)
endpoint=$(az cognitiveservices account show -n acv-xmas-tagger -g $rgName --query properties.endpoint -o tsv)
storageAccountConnectionString=$(az storage account show-connection-string -n $storageAccountNameImages -g $rgName --query connectionString -o tsv)

az functionapp config appsettings set -n $fnAppName \
  -g $rgName \
  --settings "ImagesStorageAccount=$storageAccountConnectionString"
  
az functionapp config appsettings set -n $fnAppName \
  -g $rgName \
  --settings "ComputerVision__SubscriptionKey=$key"

az functionapp config appsettings set -n $fnAppName \
  -g $rgName \
  --settings "ComputerVision__Endpoint=$endpoint"
  