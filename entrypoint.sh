#!/bin/sh

sleep 1
cd /app/NCVC.App 

dotnet restore
PATH="$PATH:/root/.dotnet/tools" dotnet ef database update

cd /app/NCVC.App/out
dotnet NCVC.App.dll 
