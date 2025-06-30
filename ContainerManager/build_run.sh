#!/bin/bash

set -e

echo "🧹 Cleaning up previous container..."
docker stop containermanager-api 2>/dev/null || true
docker rm containermanager-api 2>/dev/null || true

echo "🧼 Removing previous image..."
docker rmi containermanager-api 2>/dev/null || true

echo "🧽 Pruning dangling images..."
docker image prune -af

echo "📦 Restoring NuGet packages..."
docker run --rm \
  -v "$PWD":/app -w /app \
  -v $HOME/.nuget/packages:/root/.nuget/packages \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet restore

echo "🔧 Cleaning .NET project via Docker..."
docker run --rm \
  -v "$PWD":/app -w /app \
  -v $HOME/.nuget/packages:/root/.nuget/packages \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet clean

echo "⚙️  Building .NET project via Docker..."
docker run --rm \
  -v "$PWD":/app -w /app \
  -v $HOME/.nuget/packages:/root/.nuget/packages \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet build

echo "🚀 Publishing .NET project via Docker..."
docker run --rm \
  -v "$PWD":/app -w /app \
  -v $HOME/.nuget/packages:/root/.nuget/packages \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet publish ContainerManager/ContainerManager.csproj -c Release -o /app/publish

echo "🐳 Building final Docker image..."
docker build --no-cache -t containermanager-api .

echo "🚀 Running the container..."
docker run -d \
  -p 5000:80 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --name containermanager-api \
  containermanager-api




  # sudo docker run --rm -v /DATA/Documents/workspace/projects/GameServerSuite/ContainerManager:/app   -w /app/ContainerManager.Api   mcr.microsoft.com/dotnet/sdk:8.0   dotnet add reference ../ContainerManager.Application/ContainerManager.Application.csproj
