#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["eShop-services.sln","eShop-services.sln"]
COPY ["src/Services/Catalog/Catalog.API/Catalog.API.csproj", "src/Services/Catalog/Catalog.API/"]
COPY ["BuildingBlocks/Logging/Common.Logging/Common.Logging.csproj", "BuildingBlocks/Logging/Common.Logging/"]
#RUN dotnet restore "src/Services/Catalog/Catalog.API/Catalog.API.csproj" 
COPY "docker-compose.dcproj" "docker-compose.dcproj"
COPY "NuGet.config" "NuGet.config"
RUN dotnet restore "eShop-services.sln"
COPY . .
#WORKDIR "/src/src/Services/Catalog/Catalog.API"
#RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

#FROM build AS publish
#RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish

WORKDIR /src/Services/Catalog/Catalog.API
RUN dotnet publish --no-restore -c Release -o /app
FROM build as publish


FROM base AS final
WORKDIR /app
#COPY --from=publish /app/publish .
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]