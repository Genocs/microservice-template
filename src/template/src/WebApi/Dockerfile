#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["src/Application", "Application/"]
COPY ["src/Domain", "Domain/"]
COPY ["src/Shared", "Shared/"]
COPY ["src/Infrastructure", "Infrastructure/"]
COPY ["src/Migrators.MSSQL", "Migrators.MSSQL/"]
COPY ["src/Migrators.MySQL", "Migrators.MySQL/"]
COPY ["src/Migrators.Oracle", "Migrators.Oracle/"]
COPY ["src/Migrators.PostgreSQL", "Migrators.PostgreSQL/"]
COPY ["src/Migrators.SqLite", "Migrators.SqLite/"]
COPY ["src/WebApi", "WebApi/"]

COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["global.json", "global.json"]
COPY ["stylecop.json", "stylecop.json"]

WORKDIR "/src/WebApi"

RUN dotnet restore "Host.csproj"

RUN dotnet build "Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build-env AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Microservice.Template.WebApi.dll"]
