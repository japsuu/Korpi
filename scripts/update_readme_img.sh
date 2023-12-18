#!/bin/bash

# Capture parameters
CLIENT_ID="$1"
SECRET="$2"

# Run the .NET project
dotnet run --project "BlockEngine/BlockEngine.csproj" -- --photomode "$CLIENT_ID" "$SECRET"
