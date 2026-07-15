FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["src/ApiGateway/Gateway.Api/Gateway.Api.csproj", "src/ApiGateway/Gateway.Api/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Extensions/BuildingBlocks.Extensions.csproj", "src/BuildingBlocks/BuildingBlocks.Extensions/"]
RUN dotnet restore "src/ApiGateway/Gateway.Api/Gateway.Api.csproj"

COPY . .
WORKDIR "/src/src/ApiGateway/Gateway.Api"
RUN dotnet build "Gateway.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
