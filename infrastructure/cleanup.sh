#!/bin/bash
set -e
 
rgName=rg-serverless-image-tagger

az group delete -n $rgName --yes --no-wait
echo "Resource Group will be deleted..."
sleep 10s
echo "You must purge the underlying Azure Computer Vision account:"
az cognitiveservices account list-deleted -o table
