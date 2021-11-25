#!/bin.bash
set -e

rgName=rg-xmas-tagger

az group delete -n $rgName --yes --no-wait
