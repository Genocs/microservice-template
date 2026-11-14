# Genocs.Persistence.EFCore Agent Reference

## Agent Operating Mode

- Assume the `Genocs.Persistence.EFCore` package family is consumed from NuGet only.
- Always pair the core package with exactly one `Genocs.Persistence.EFCore.*` provider package and register it; never generate code that relies on the core package alone connecting to a database.
- Treat documented extension methods, options, interfaces, and repository types as the only safe API surface.
- Do not invent unit-of-work orchestration, automatic migration generation, or tenant-store backends other than SQL Server.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because registration entry points extend `IGenocsBuilder`.

## Purpose

This document is optimized for AI-assisted development sessions.
It prioritizes fast retrieval of:

- What the `Genocs.Persistence.EFCore` package family is responsible for
- Which APIs to call for specific goals
- Where source of truth lives
- What constraints and runtime behaviors matter

This reference covers the whole package family introduced by the July 2026 provider split: the provider-agnostic core, the six database provider packages, and the SQL Server-backed multitenancy package.

## Quick Facts

| Key | Value |
|---|---|
| Package | Genocs.Persistence.EFCore (core) |
| Project file | [src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj](../src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj) |
| Target frameworks | net10.0, net9.0, net8.0 |
| Primary role | EF Core repositories, DbContext wiring, initialization/seeding pipeline, provider abstraction |
| Core themes | Repository pattern, Ardalis.Specification, Dapper queries, config-driven provider selection, migration gating |

### Package family

| Package | Provider key | Registration extension | Notes |
|---|---|---|---|
| `Genocs.Persistence.EFCore` | — | `AddEFCorePersistence` | Core; references no database driver |
| `Genocs.Persistence.EFCore.SqlServer` | `mssql` | `AddSqlServerDbProvider` | |
| `Genocs.Persistence.EFCore.PostgreSQL` | `postgresql` | `AddPostgreSqlDbProvider` | |
| `Genocs.Persistence.EFCore.MySql` | `mysql` | `AddMySqlDbProvider` | `Configure` throws `NotSupportedException` on net10.0 (Pomelo gap) |
| `Genocs.Persistence.EFCore.Sqlite` | `sqlite` | `AddSqliteDbProvider` | |
| `Genocs.Persistence.EFCore.Oracle` | `oracle` | `AddOracleDbProvider` | |
| `Genocs.Persistence.EFCore.MongoDB` | `mongodb` | `AddMongoDbProvider` | `SupportsMigrations = false`; database name from `DatabaseOptions` |
| `Genocs.Persistence.EFCore.MultiTenancy.SqlServer` | — | `AddFinbuckleMultiTenancyWithEfCoreStore` | Finbuckle multitenancy; tenant store pinned to SQL Server |

## Use This Package When

- You need EF Core-backed repositories (`IRepository<T>`, `IReadRepository<T>`, `IRepositoryWithEvents<T>`) for aggregate roots in a Genocs application.
- You want config-driven database provider selection via `DatabaseOptions:DBProvider` in `appsettings.json`.
- You need startup database initialization with a safety gate on applying migrations (`AutoApplyMigrations`).
- You need custom data seeding through `ICustomSeeder` implementations discovered from loaded assemblies.
- You need Finbuckle-based multitenancy with a SQL Server tenant store (install `Genocs.Persistence.EFCore.MultiTenancy.SqlServer`).

## Do Not Assume

- Do not assume the core package can connect to any database on its own. It references no database driver; a matching `Genocs.Persistence.EFCore.*` provider package must be installed and registered, otherwise `DbProviderResolver.Resolve` throws `InvalidOperationException` at first DbContext use.
- Do not assume MySQL works on net10.0. `MySqlDbProvider.Configure` throws `NotSupportedException` there until Pomelo.EntityFrameworkCore.MySql supports EF Core 10.
- Do not assume MongoDB supports migrations. `MongoDbProvider.SupportsMigrations` is `false`; `ApplicationDbInitializer` skips migration logic and goes straight to seeding.
- Do not assume the multitenancy tenant store follows `DBProvider`. `TenantDbContext` is configured with `SqlServerDbProvider` unconditionally — `Genocs.Persistence.EFCore.MultiTenancy.SqlServer` is SQL Server-only by design.
- Do not assume migrations live in the consuming application assembly. Each provider hardcodes a migrations assembly convention (`Migrators.MSSQL`, `Migrators.PostgreSQL`, `Migrators.MySQL`, `Migrators.SqLite`, `Migrators.Oracle`).
- Do not assume pending migrations are applied by default. `AutoApplyMigrations` defaults to `false`; with pending migrations present, the initializer logs an error and stops the host.

## High-Value Entry Points

### Startup and DI registration (core)

- `AddEFCorePersistence(this IGenocsBuilder, params Assembly[])` in [src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs](../src/Genocs.Persistence.EFCore/Extensions/EFCoreExtensions.cs)
- `AddGenocsRepositories<TContext>(this IServiceCollection, Assembly)` in the same file, for a host `DbContext` that is not `ApplicationDbContext`
- `DatabaseOptions` (binds section `DatabaseOptions`) in [src/Genocs.Persistence.EFCore/Configurations/DatabaseOptions.cs](../src/Genocs.Persistence.EFCore/Configurations/DatabaseOptions.cs)
- `DbProviderKeys` in [src/Genocs.Persistence.EFCore/Common/DbProviderKeys.cs](../src/Genocs.Persistence.EFCore/Common/DbProviderKeys.cs)

### Provider abstraction

- `IEFCoreDbProvider` in [src/Genocs.Persistence.EFCore/Providers/IEFCoreDbProvider.cs](../src/Genocs.Persistence.EFCore/Providers/IEFCoreDbProvider.cs)
- `DbProviderResolver` (internal dispatch by provider key) in [src/Genocs.Persistence.EFCore/Providers/DbProviderResolver.cs](../src/Genocs.Persistence.EFCore/Providers/DbProviderResolver.cs)
- Provider registrations, e.g. `AddSqlServerDbProvider` in [src/Genocs.Persistence.EFCore.SqlServer/Extensions.cs](../src/Genocs.Persistence.EFCore.SqlServer/Extensions.cs) (same shape in each provider package)

### Repositories and data access

- `ApplicationDbRepository<T>` (Ardalis.Specification-based `IRepository<T>`) in [src/Genocs.Persistence.EFCore/Repositories/ApplicationDbRepository.cs](../src/Genocs.Persistence.EFCore/Repositories/ApplicationDbRepository.cs)
- `EventAddingRepositoryDecorator<T>` (`IRepositoryWithEvents<T>`) in [src/Genocs.Persistence.EFCore/Repositories/EventAddingRepositoryDecorator.cs](../src/Genocs.Persistence.EFCore/Repositories/EventAddingRepositoryDecorator.cs)
- `DapperRepository` (`IDapperRepository`) in [src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs](../src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs)
- `ApplicationDbContext` in [src/Genocs.Persistence.EFCore/Context/ApplicationDbContext.cs](../src/Genocs.Persistence.EFCore/Context/ApplicationDbContext.cs)

### Initialization and seeding

- `IDatabaseInitializer` in [src/Genocs.Persistence.EFCore/Persistence/Initialization/IDatabaseInitializer.cs](../src/Genocs.Persistence.EFCore/Persistence/Initialization/IDatabaseInitializer.cs)
- `ApplicationDbInitializer` (public; migration gating + seeding) in [src/Genocs.Persistence.EFCore/Initialization/ApplicationDbInitializer.cs](../src/Genocs.Persistence.EFCore/Initialization/ApplicationDbInitializer.cs)
- `ApplicationDbSeeder` / `CustomSeederRunner` in [src/Genocs.Persistence.EFCore/Initialization/ApplicationDbSeeder.cs](../src/Genocs.Persistence.EFCore/Initialization/ApplicationDbSeeder.cs) and [src/Genocs.Persistence.EFCore/Initialization/CustomSeederRunner.cs](../src/Genocs.Persistence.EFCore/Initialization/CustomSeederRunner.cs)
- `ICustomSeeder` contract in [src/Genocs.Common/Persistence/Initialization/ICustomSeeder.cs](../src/Genocs.Common/Persistence/Initialization/ICustomSeeder.cs)

### Connection string handling

- `IConnectionStringSecurer` dispatcher in [src/Genocs.Persistence.EFCore/Configurations/ConnectionStringSecurer.cs](../src/Genocs.Persistence.EFCore/Configurations/ConnectionStringSecurer.cs)
- `IConnectionStringValidator` dispatcher in [src/Genocs.Persistence.EFCore/Configurations/ConnectionStringValidator.cs](../src/Genocs.Persistence.EFCore/Configurations/ConnectionStringValidator.cs)

### Multitenancy (Genocs.Persistence.EFCore.MultiTenancy.SqlServer)

- `AddFinbuckleMultiTenancy<TTenantInfo>` / `AddFinbuckleMultiTenancyWithEfCoreStore` / `UseMultiTenancy` in [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/Extensions.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/Extensions.cs)
- `GNXTenantInfo` in [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/GNXTenantInfo.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/GNXTenantInfo.cs)
- `ITenantService` (tenant CRUD, activation, subscription) in [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/ITenantService.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/ITenantService.cs)
- `ITenantDatabaseInitializer` in [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/ITenantDatabaseInitializer.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/ITenantDatabaseInitializer.cs)
- `TenantDbContext` (Finbuckle EF Core store, SQL Server) in [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDbContext.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDbContext.cs)

## Decision Matrix For Agents

| Goal | Preferred API | Notes |
|---|---|---|
| Wire EF Core persistence | `builder.AddEFCorePersistence()` | Requires a provider package registration too |
| Select database engine | `services.Add[Provider]DbProvider()` + `DatabaseOptions:DBProvider` | Provider key must match a registered `IEFCoreDbProvider` |
| Run startup initialization | resolve `IDatabaseInitializer`, call `InitializeDatabasesAsync` | Runs migrations per `AutoApplyMigrations`, then seeders |
| Seed initial data | implement `ICustomSeeder` | Discovered from loaded assemblies, run by `CustomSeederRunner` |
| Query with specifications | `IRepository<T>` / `IReadRepository<T>` | Open generics bound to `ApplicationDbContext` by `AddEFCorePersistence`. Call `AddGenocsRepositories<TContext>(domainAssembly)` when the host context is a different `DbContext`, including a multitenant context |
| Repositories that append entity events | `IRepositoryWithEvents<T>` | Registered only for concrete `IAggregateRoot` types in the scanned assembly that also implement `IGeneratesDomainEvents` |
| Raw SQL query | `IDapperRepository` | Flows `CancellationToken` via `CommandDefinition` |
| Add multitenancy with EF Core tenant store | `builder.AddFinbuckleMultiTenancyWithEfCoreStore()` | SQL Server-only tenant store; registers `ITenantService` and `ITenantDatabaseInitializer` |
| Add multitenancy from configuration | `AddFinbuckleMultiTenancy<TTenantInfo>(configuration)` | Host strategy + configuration store; does not register `ITenantService` |
| Create a tenant + its database | `ITenantService.CreateAsync` | Internally calls `ITenantDatabaseInitializer.InitializeApplicationDbForTenantAsync` |
| Mask credentials in a connection string | `IConnectionStringSecurer.MakeSecure` | Delegates to the registered provider; unknown providers return the string unchanged |

## Minimal Integration Recipe

### Install

```bash
dotnet add package Genocs.Persistence.EFCore
dotnet add package Genocs.Persistence.EFCore.SqlServer
# optional, for Finbuckle multitenancy with SQL Server tenant store:
dotnet add package Genocs.Persistence.EFCore.MultiTenancy.SqlServer
```

### Setup in Program.cs

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.EFCore.Extensions;
using Genocs.Persistence.EFCore.MultiTenancy;
using Genocs.Persistence.EFCore.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlServerDbProvider();

var genocsBuilder = builder.AddGenocs()
    .AddEFCorePersistence();

// Optional: Finbuckle multitenancy with EF Core (SQL Server) tenant store
genocsBuilder.AddFinbuckleMultiTenancyWithEfCoreStore();

var app = builder.Build();

app.UseMultiTenancy(); // only when multitenancy is registered

app.Run();
```

### Configuration (appsettings.json)

```json
{
  "DatabaseOptions": {
    "DBProvider": "mssql",
    "ConnectionString": "Server=localhost;Database=app;Integrated Security=True;TrustServerCertificate=True;",
    "AutoApplyMigrations": false
  }
}
```

## Behavior Notes That Affect Agent Decisions

- `AddEFCorePersistence` binds and validates `DatabaseOptions` with `ValidateOnStart`; missing `DBProvider` or `ConnectionString` fails at host start.
- `ApplicationDbContext` configuration is deferred: the provider is resolved from `IEnumerable<IEFCoreDbProvider>` when the DbContext is first built, so a missing provider package surfaces at first use with the message "Install the matching Genocs.Persistence.EFCore.* provider package and register it".
- `ApplicationDbInitializer.InitializeAsync` consults `IEFCoreDbProvider.SupportsMigrations`: MongoDB skips migration handling and only seeds. For relational providers, pending migrations with `AutoApplyMigrations = false` log an error and stop the host via `IHostApplicationLifetime.StopApplication()`.
- MongoDB database name resolution: explicit `DatabaseOptions.DatabaseName` wins, otherwise it is parsed from the connection string path (`mongodb://host/dbname`); validation fails when neither resolves.
- `AddEFCorePersistence` registers open `IRepository<>` and `IReadRepository<>` against `ApplicationDbContext`. It scans `Assembly.GetEntryAssembly()` plus any assemblies passed to it, and registers `IRepositoryWithEvents<T>` only for concrete aggregate roots that implement `IGeneratesDomainEvents`.
- `AddGenocsRepositories<TContext>(domainAssembly)` is the registration to use when the host `DbContext` is not `ApplicationDbContext`. `TContext : DbContext`, so a multitenant context is valid. `TContext` must be public. It scans only `domainAssembly` (not `Genocs.Common`, and not the entry assembly). Open repository registrations are kept from the first caller, so call it instead of expecting `AddEFCorePersistence` to rebind the context.
- Prefer `AggregateRoot` or `AggregateRoot<TPrimaryKey>` from `Genocs.Core` for aggregates that use `IRepositoryWithEvents<T>`. Those types implement `IGeneratesDomainEvents` and store the event list. `IAggregateRoot` extends `Genocs.Common.Domain.Entities.IEntity`, which `EntityCreatedEvent<T>`, `EntityUpdatedEvent<T>`, and `EntityDeletedEvent<T>` require. An aggregate that only implements the non-generic `IAggregateRoot` marker gets `IRepository<T>` and `IReadRepository<T>`, and does not get `IRepositoryWithEvents<T>`.
- Tenant creation (`TenantService.CreateAsync`) initializes the tenant database in an isolated DI scope; a tenant-dedicated connection string redirects the scoped `ApplicationDbContext` before initialization. On failure the tenant is removed from the store.
- The multitenancy EF Core store path registers claim, header (`tenant`), and query-string strategies for `GNXTenantInfo`; only one Finbuckle store can be active at a time.
- `TenantDbContext` still sets the process-wide `Npgsql.EnableLegacyTimestampBehavior` switch in its constructor (vestigial, tracked as EFCORE-013).

## Source-Accurate Capability Map

### Provider abstraction and selection

- Provider contract with `ProviderKey`, `Configure`, `MakeSecureConnectionString`, `TryValidateConnectionString`, `SupportsMigrations`
- Case-insensitive resolution by provider key with actionable error on missing provider
- One `TryAddEnumerable` singleton registration per provider package

Files:

- [src/Genocs.Persistence.EFCore/Providers/IEFCoreDbProvider.cs](../src/Genocs.Persistence.EFCore/Providers/IEFCoreDbProvider.cs)
- [src/Genocs.Persistence.EFCore/Providers/DbProviderResolver.cs](../src/Genocs.Persistence.EFCore/Providers/DbProviderResolver.cs)
- [src/Genocs.Persistence.EFCore.SqlServer/SqlServerDbProvider.cs](../src/Genocs.Persistence.EFCore.SqlServer/SqlServerDbProvider.cs)
- [src/Genocs.Persistence.EFCore.MongoDB/MongoDbProvider.cs](../src/Genocs.Persistence.EFCore.MongoDB/MongoDbProvider.cs)

### Repositories

- Specification-based repository with Mapster projection support
- `AddGenocsRepositories<TContext>` binds that repository to a caller `DbContext`
- Domain-event-adding decorator for aggregate roots that implement `IAggregateRoot` and `IGeneratesDomainEvents`
- Dapper-based raw SQL repository

Files:

- [src/Genocs.Persistence.EFCore/Repositories/ApplicationDbRepository.cs](../src/Genocs.Persistence.EFCore/Repositories/ApplicationDbRepository.cs)
- [src/Genocs.Persistence.EFCore/Repositories/EventAddingRepositoryDecorator.cs](../src/Genocs.Persistence.EFCore/Repositories/EventAddingRepositoryDecorator.cs)
- [src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs](../src/Genocs.Persistence.EFCore/Repositories/DapperRepository.cs)

### Initialization and seeding

- `IDatabaseInitializer` orchestrates application DB initialization
- Migration gating on `AutoApplyMigrations` with host stop on unexpected pending migrations
- `ICustomSeeder` discovery and execution

Files:

- [src/Genocs.Persistence.EFCore/Initialization/DatabaseInitializer.cs](../src/Genocs.Persistence.EFCore/Initialization/DatabaseInitializer.cs)
- [src/Genocs.Persistence.EFCore/Initialization/ApplicationDbInitializer.cs](../src/Genocs.Persistence.EFCore/Initialization/ApplicationDbInitializer.cs)
- [src/Genocs.Persistence.EFCore/Initialization/CustomSeederRunner.cs](../src/Genocs.Persistence.EFCore/Initialization/CustomSeederRunner.cs)

### Connection string handling

- Provider-dispatched masking of credentials (`IConnectionStringSecurer`)
- Provider-dispatched validation with exception logging (`IConnectionStringValidator`)

Files:

- [src/Genocs.Persistence.EFCore/Configurations/ConnectionStringSecurer.cs](../src/Genocs.Persistence.EFCore/Configurations/ConnectionStringSecurer.cs)
- [src/Genocs.Persistence.EFCore/Configurations/ConnectionStringValidator.cs](../src/Genocs.Persistence.EFCore/Configurations/ConnectionStringValidator.cs)

### Multitenancy (SQL Server)

- Finbuckle integration: configuration store or EF Core store paths
- Tenant CRUD, activation/deactivation, subscription validity via `ITenantService`
- Per-tenant database initialization in isolated scopes via `ITenantDatabaseInitializer`
- MediatR request/handler pairs for tenant operations

Files:

- [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/Extensions.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/Extensions.cs)
- [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantService.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantService.cs)
- [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDatabaseInitializer.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDatabaseInitializer.cs)
- [src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDbContext.cs](../src/Genocs.Persistence.EFCore.MultiTenancy.SqlServer/TenantDbContext.cs)

## Dependencies

From [src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj](../src/Genocs.Persistence.EFCore/Genocs.Persistence.EFCore.csproj) (core):

- `Genocs.Core` (project reference)
- `Microsoft.EntityFrameworkCore.Relational` (version per target framework)
- `Ardalis.Specification.EntityFrameworkCore`
- `Dapper`
- `Mapster`
- `MediatR`
- `Serilog.Extensions.Hosting`

Provider packages add only their EF Core driver (e.g. `Microsoft.EntityFrameworkCore.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Pomelo.EntityFrameworkCore.MySql`, `Oracle.EntityFrameworkCore`, `MongoDB.EntityFrameworkCore`). The multitenancy package adds `Finbuckle.MultiTenant.AspNetCore`, `Finbuckle.MultiTenant.EntityFrameworkCore`, `Mapster`, and `MediatR`, and references `Genocs.Persistence.EFCore.SqlServer`.

## Related Docs

- NuGet package readme: [src/Genocs.Persistence.EFCore/README.md](../src/Genocs.Persistence.EFCore/README.md)
- Repository guide: [README.md](../README.md)
- Assessment: [docs/Genocs.Persistence.EFCore-Assessment.md](Genocs.Persistence.EFCore-Assessment.md)
- Implementation backlog: [docs/Genocs.Persistence.EFCore-Implementation-Backlog.md](Genocs.Persistence.EFCore-Implementation-Backlog.md)
