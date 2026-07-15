FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["src/Presentation/EmployeeService.Api/EmployeeService.Api.csproj", "src/Presentation/EmployeeService.Api/"]
COPY ["src/Core/EmployeeService.Application/EmployeeService.Application.csproj", "src/Core/EmployeeService.Application/"]
COPY ["src/Core/EmployeeService.Domain/EmployeeService.Domain.csproj", "src/Core/EmployeeService.Domain/"]
COPY ["src/Infrastructure/EmployeeService.Infrastructure/EmployeeService.Infrastructure.csproj", "src/Infrastructure/EmployeeService.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Extensions/BuildingBlocks.Extensions.csproj", "src/BuildingBlocks/BuildingBlocks.Extensions/"]
RUN dotnet restore "src/Presentation/EmployeeService.Api/EmployeeService.Api.csproj"

COPY . .
WORKDIR "/src/src/Presentation/EmployeeService.Api"
RUN dotnet build "EmployeeService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeService.Api.dll"]
