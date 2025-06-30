#!/bin/bash

echo "🧹 Stopping and removing old container..."
docker stop containermanager-api 2>/dev/null || true
docker rm containermanager-api 2>/dev/null || true

echo "🧼 Removing previous image..."
docker rmi containermanager-api 2>/dev/null || true

echo "♻️  Building image without cache..."
docker build --no-cache -t containermanager-api .

echo "🧽 Cleaning up dangling images..."
docker image prune -f

echo "🚀 Launching new container..."
docker run -d \
  -p 5000:80 \
  -v /var/run/docker.sock:/var/run/docker.sock \
  --name containermanager-api \
  containermanager-api
