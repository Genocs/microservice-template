# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the official .NET runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Copy global build configuration files first for better layer caching
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["stylecop.json", "."]
COPY ["dotnet.ruleset", "."]
COPY ["global.json", "."]

# COPY .csproj and restore as distinct layers
# This is CRITICAL: If code changes but .csproj doesn't, Docker reuses this cached layer
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Contracts/Contracts.csproj", "src/Contracts/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Migrators.MSSQL/Migrators.MSSQL.csproj", "src/Migrators.MSSQL/"]
COPY ["src/Migrators.MySQL/Migrators.MySQL.csproj", "src/Migrators.MySQL/"]
COPY ["src/Migrators.Oracle/Migrators.Oracle.csproj", "src/Migrators.Oracle/"]
COPY ["src/Migrators.PostgreSQL/Migrators.PostgreSQL.csproj", "src/Migrators.PostgreSQL/"]
COPY ["src/Migrators.SqLite/Migrators.SqLite.csproj", "src/Migrators.SqLite/"]
COPY ["src/WebApi/Host.csproj", "src/WebApi/"]

RUN dotnet restore "./src/WebApi/Host.csproj"

# Copy the rest of the source code
COPY ["src/Application", "src/Application/"]
COPY ["src/Domain", "src/Domain/"]
COPY ["src/Contracts", "src/Contracts/"]
COPY ["src/Infrastructure", "src/Infrastructure/"]
COPY ["src/Migrators.MSSQL", "src/Migrators.MSSQL/"]
COPY ["src/Migrators.MySQL", "src/Migrators.MySQL/"]
COPY ["src/Migrators.Oracle", "src/Migrators.Oracle/"]
COPY ["src/Migrators.PostgreSQL", "src/Migrators.PostgreSQL/"]
COPY ["src/Migrators.SqLite", "src/Migrators.SqLite/"]
COPY ["src/WebApi", "src/WebApi/"]

WORKDIR "/src/src/WebApi"
RUN dotnet build "./Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Build and Publish
# -c Release: Optimizes code
# /p:UseAppHost=false: Disables creating a native exe (we just need the dll)
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create final runtime image
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Microservice.Template.WebApi.dll"]

