# Step 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .

RUN dotnet restore
RUN dotnet clean
RUN dotnet build
RUN dotnet publish ContainerManager.Api/ContainerManager.Api.csproj -c Release -o /app/publish
#dotnet publish ContainerManager/ContainerManager.csproj -c Release -o /app/publish

# Step 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY Resources /app/Resources
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "ContainerManager.Api.dll"]
