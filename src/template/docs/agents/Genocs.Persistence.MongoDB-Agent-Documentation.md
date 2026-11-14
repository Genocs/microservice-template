# Genocs.Persistence.MongoDB Agent Reference

## Agent Operating Mode

- Assume `Genocs.Persistence.MongoDB` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, options, interfaces, and repository types as the only safe API surface.
- Generate MongoDB registration, repository injection, collection mapping, and seeding code only.
- Do not invent unit-of-work orchestration, automatic migrations, partial-update helpers, or production-safe encryption features that this package does not actually wire.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because all registration entry points extend `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Persistence.MongoDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | MongoDB connectivity and repository integration for Genocs services |
| Main value | `AddMongo(...)`, `AddMongoWithRegistration(...)`, `IMongoRepository<TEntity>`, `IMongoBaseRepository<TEntity, TKey>`, `IMongoSessionFactory`, and startup seeding through `IMongoInitializer` |
| Requires | `Genocs.Core` and MongoDB |

## Current Implementation Status (April 2026)

The following backlog tasks are completed and reflected by the current package behavior:

- `MONGO-001`: Removed runtime `NotImplementedException` paths from repository APIs.
- `MONGO-002`: Removed sync-over-async and fire-and-forget write behavior from critical repository paths.
- `MONGO-003`: Aligned repository contract nullability and hidden-member semantics (`override`/contract shape).
- `MONGO-004`: Unified `IMongoClient`/`IMongoDatabase` composition so provider, session factory, and repositories use one DI-managed client pipeline.
- `MONGO-005`: Seeding guard now tracks per database key (`connectionString|database`) with retry-safe behavior after failures.
- `MONGO-006`: GUID serializer strategy defaults to `Standard` with explicit compatibility mode (`CSharpLegacy`).
- `MONGO-007`: Removed unfinished built-in encryption surface from package runtime composition.
- `MONGO-008`: Added executable unit tests covering repository contracts, registration behavior, and initializer guard behavior.
- `MONGO-009`: Replaced SQL-based component test scaffold with MongoDB Testcontainers integration tests using environment-driven skip conditions.

## Agent Execution Protocol

Use this protocol when generating or modifying code for this package:

1. Detect registration path first:
- `AddMongoWithRegistration()` for `ObjectId` + `IMongoEntity` path.
- `AddMongo()` + `AddMongoRepository<TEntity, TKey>(collectionName)` for custom key path.
2. Detect collection mapping strategy:
- `TableMappingAttribute`/type-name for default `IMongoRepository<TEntity>` path.
- explicit `collectionName` for `IMongoBaseRepository<TEntity, TKey>` path.
3. Detect startup behavior requirements:
- if seeding is required, ensure both `genocs.Build()` and `app.UseGenocs()` are present.
- if not required, keep `mongoDb.seed` disabled.
4. Detect serialization compatibility requirements:
- use `mongoDb.guidRepresentationMode = Standard` unless legacy data compatibility is explicitly required.
5. Avoid unsupported assumptions:
- do not generate built-in encryption configuration for this package.
- do not generate partial-update semantics for repository updates.

## What This Package Is For

Use `Genocs.Persistence.MongoDB` when you need to:

- register `IMongoClient` and `IMongoDatabase` for a Genocs host
- inject a generic Mongo repository for `ObjectId`-backed entities
- inject a custom-key Mongo repository bound to an explicit collection name
- map collections from `TableMappingAttribute` or entity type name
- page Mongo query results through `IPagedQuery`
- start driver sessions through `IMongoSessionFactory`
- run startup-time seed logic through `IMongoSeeder`

## What This Package Does Not Do By Itself

Do not assume `Genocs.Persistence.MongoDB` can:

- create or validate schema migrations the way an EF Core package would
- provide a MongoDB-specific unit-of-work abstraction
- provide partial-update builders or patch semantics
- validate database connectivity during registration time
- guarantee transaction support on standalone MongoDB deployments
- give one uniform repository behavior across every registration path in this package
- enable client-side field-level encryption in the current implementation
- register all custom repositories automatically unless you explicitly scan or add them

## Safe Default Mental Model

Treat `Genocs.Persistence.MongoDB` as four things:

1. A Genocs builder extension that registers MongoDB driver services
2. Two different repository registration paths with different behavior
3. A startup initializer hook for optional seeding
4. A package that applies global BSON conventions once per process

If a user asks for advanced repository behavior, verify which registration path is actually in use before generating code.

## Choose The Right Registration Path

| Situation | Prefer | Why |
|---|---|---|
| You only need Mongo driver services and will register repositories yourself | `AddMongo()` | smallest registration path |
| Your entities use `ObjectId` and implement `IMongoEntity` | `AddMongoWithRegistration()` | registers `IMongoRepository<TEntity>` automatically |
| Your entities use a non-`ObjectId` key such as `Guid` or `string` | `AddMongoRepository<TEntity, TKey>(collectionName)` | binds one explicit `IMongoBaseRepository<TEntity, TKey>` |
| You have custom repository implementations that already implement `IMongoRepository<>` | `RegisterMongoRepositories(assembly)` | scans and registers those implementations |
| You need raw driver access | inject `IMongoDatabaseProvider`, `IMongoDatabase`, or `IMongoClient` | bypass repository abstractions deliberately |

## Fast Start Recipes

### Recipe 1: Register MongoDB Driver Services Only

Use this when the host needs database access but will not use the package's default `IMongoRepository<TEntity>` registration.

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddMongo();

genocs.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();
```

Configuration:

```json
{
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "orders",
        "enableTracing": true,
        "seed": false,
        "setRandomDatabaseSuffix": false
    }
}
```

Effect:

- registers `MongoOptions` as a singleton
- registers `IMongoClient` as a singleton
- registers `IMongoDatabase` as a singleton service
- registers `IMongoInitializer`, `IMongoSeeder`, `IMongoSessionFactory`, and `IMongoDatabaseProvider`
- adds the Mongo initializer into the Genocs startup initializer pipeline

Important behavior:

- this path does not register `IMongoRepository<TEntity>` automatically
- seed logic runs only if the host also calls `genocs.Build()` and `app.UseGenocs()`

### Recipe 2: Register The Default ObjectId Repository

Use this when an entity uses MongoDB `ObjectId` as its key and implements `IMongoEntity`.

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddMongoWithRegistration();

genocs.Build();

public sealed class UsersService(IMongoRepository<UserDocument> repository)
{
}
```

Use this when the default repository should infer the collection name from `TableMappingAttribute` or the entity type name.

### Recipe 3: Register A Custom-Key Repository With An Explicit Collection Name

Use this when the entity key is not `ObjectId`.

```csharp
using Genocs.Common.Domain.Entities;
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddMongo()
        .AddMongoRepository<ProductDocument, Guid>("products");

genocs.Build();

public sealed class ProductsService(IMongoBaseRepository<ProductDocument, Guid> repository)
{
}

public sealed class ProductDocument : IEntity<Guid>
{
        public Guid Id { get; set; }
}
```

Use this when the collection name must be explicit and stable.

### Recipe 4: Map The Collection With `TableMappingAttribute`

Use this when the default repository should use a collection name that differs from the CLR type name.

```csharp
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.MongoDB.Domain.Entities;
using MongoDB.Bson;

[TableMapping("Users")]
public sealed class UserDocument : IMongoEntity
{
        public ObjectId Id { get; set; }
}
```

Important behavior:

- the default `MongoRepository<TEntity>` path checks `TableMappingAttribute`
- if the attribute is absent, it falls back to `typeof(TEntity).Name`
- the explicit `AddMongoRepository<TEntity, TKey>(collectionName)` path ignores `TableMappingAttribute` because the collection name is already fixed at registration time

### Recipe 5: Run Seed Logic At Startup

Use this when the application wants initial data or collection setup during startup.

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Persistence.MongoDB.Repositories;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddMongo(seederType: typeof(AppMongoSeeder));

genocs.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();

public sealed class AppMongoSeeder : IMongoSeeder
{
        public Task SeedAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
}
```

Configuration:

```json
{
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "orders",
        "seed": true
    }
}
```

Important behavior:

- seeding is guarded per database key (`connectionString|database`)
- the same database key seeds once per process, but different database keys seed independently
- failed seeding clears the guard key so a subsequent startup attempt can retry deterministically

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddMongo(string sectionName = "mongoDb", Type? seederType = null, bool registerConventions = true)` | Register Mongo driver services from configuration | No-ops when the config section does not exist | Assuming registration fails loudly |
| `AddMongo(MongoOptions options, Type? seederType = null, bool registerConventions = true)` | Register Mongo driver services from explicit options | Refuses registration if `TryRegister("mongoDb")` fails or options are invalid | Calling it repeatedly and assuming the second call will override the first |
| `AddMongoWithRegistration(...)` | Register Mongo plus `IMongoRepository<TEntity>` | Adds `IMongoRepository<>` only when the config section exists | Assuming it supports non-`ObjectId` keys |
| `AddMongoRepository<TEntity, TKey>(collectionName)` | Register one explicit collection-bound repository | Registers `IMongoBaseRepository<TEntity, TKey>` only | Assuming it also registers `IMongoRepository<TEntity>` |
| `RegisterMongoRepositories(assembly, lifetime)` | Scan and register custom repository implementations | Scans assembly dependencies and registers implementations as their interfaces | Assuming it registers the package's built-in generic repository |
| `IMongoRepository<TEntity>` | Use the default `ObjectId` repository contract | Requires `TEntity : IMongoEntity` | Using it for `Guid` or `string` keys |
| `IMongoBaseRepository<TEntity, TKey>` | Use the Mongo-specific generic repository contract | Adds async predicate, paging, and existence methods on top of the base repository contract | Assuming behavior is identical across the package's two concrete repository implementations |
| `IMongoDatabaseProvider` | Access `MongoClient` and `Database` together | Uses DI-registered `IMongoClient` and `MongoOptions` to resolve `IMongoDatabase` | Assuming it creates its own client instance separate from DI |
| `IMongoSessionFactory.CreateAsync()` | Start a MongoDB client session | Uses the DI-registered `IMongoClient` singleton | Assuming transactions work on every MongoDB deployment |
| `IMongoSeeder` | Provide startup seed logic | Runs only through `IMongoInitializer` | Forgetting that the host must run Genocs startup initializers |

## Repository Semantics That Matter

### There Are Two Concrete Repository Paths

The package uses two different internal repository implementations:

1. `MongoRepository<TEntity>` via `AddMongoWithRegistration()`
2. `MongoBaseRepository<TEntity, TKey>` via `AddMongoRepository<TEntity, TKey>(collectionName)`

They share the same interface surface poorly, not perfectly.

### Collection Resolution

`MongoRepository<TEntity>`:

- uses `TableMappingAttribute` when present
- otherwise uses `typeof(TEntity).Name`

`AddMongoRepository<TEntity, TKey>(collectionName)`:

- always uses the explicit `collectionName`

### Async Lookup Difference

This is important for AI-generated code:

- the default `MongoRepository<TEntity>` path implements `GetAsync(predicate)` with `FirstAsync()` and will throw when there is no match
- the explicit `AddMongoRepository<TEntity, TKey>` path implements `GetAsync(predicate)` with `SingleOrDefaultAsync()` and can return `null` when there is no match

Do not describe `GetAsync(predicate)` as having one universal behavior across the package.

### Update Behavior

Both repository paths use full-document replacement through `ReplaceOne` or `ReplaceOneAsync`.

Safe assumption:

- pass the full replacement entity

Unsafe assumption:

- expecting patch or partial-update semantics

### Paging Behavior

`BrowseAsync(...)` and `Pagination.PaginateAsync(...)`:

- normalize `page <= 0` to `1`
- normalize `resultsPerPage <= 0` to `10`
- return `PagedResult<T>.Empty` when the query is empty
- sort dynamically by property name when `orderBy` is supplied
- throw if `orderBy` does not name a valid property on `T`

### BSON Conventions

When conventions are enabled, the package registers these global conventions and serializers once per process:

- decimal as `Decimal128`
- nullable decimal as `Decimal128`
- `GuidSerializer(GuidRepresentation.Standard)` by default
- camelCase element names
- ignored extra elements on deserialization
- enum values serialized as strings

These are process-wide serializer settings, not per-database settings.

GUID representation behavior:

- default mode is `GuidRepresentation.Standard`
- compatibility mode `CSharpLegacy` is available through `mongoDb.guidRepresentationMode`

## Configuration Ownership

`Genocs.Persistence.MongoDB` owns the `mongoDb` section through `MongoOptions`.

```json
{
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "orders",
        "enableTracing": true,
        "seed": false,
                "setRandomDatabaseSuffix": false,
                "guidRepresentationMode": "Standard"
    }
}
```

What the package actively uses:

- `connectionString`
- `database`
- `enableTracing`
- `seed`
- `setRandomDatabaseSuffix`

What each field does:

- `connectionString`: required
- `database`: required
- `enableTracing`: subscribes the Mongo driver diagnostics activity subscriber
- `seed`: enables startup seeding through `IMongoInitializer`
- `setRandomDatabaseSuffix`: appends a random GUID suffix to the configured database name before registration
- `guidRepresentationMode`: chooses BSON GUID strategy (`Standard` default, `CSharpLegacy` compatibility)

## Public Capability Map

### Registration

- `AddMongo(...)`
- `AddMongoWithRegistration(...)`
- `AddMongoRepository<TEntity, TKey>(...)`
- `RegisterMongoRepositories(...)`

### Options

- `MongoOptions`

### Repositories

- `IMongoRepository<TEntity>`
- `IMongoBaseRepository<TEntity, TKey>`
- `MongoRepository<TEntity>`
- `MongoBaseRepositoryOfType<TEntity, TKey>`

### Driver Access

- `IMongoDatabaseProvider`
- `IMongoSessionFactory`

### Startup And Seeding

- `IMongoInitializer`
- `IMongoSeeder`

### Utilities

- `Pagination.PaginateAsync(...)`
- `Pagination.Limit(...)`
- `IMongoEntity`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `AddMongo()` throws when configuration is missing. It can no-op.
2. Do not assume repeated `AddMongo(...)` calls override one another. Registration is guarded by `TryRegister("mongoDb")`.
3. Do not assume `IMongoRepository<TEntity>` works for `Guid`, `string`, or other non-`ObjectId` keys.
4. Do not assume every `IMongoBaseRepository<TEntity, TKey>` behaves exactly the same. The package uses two different concrete implementations.
5. Do not assume `GetAsync(predicate)` has one shared null-or-throw behavior across registration paths.
6. Do not assume updates are partial; they are full replacements.
7. Do not assume seeding runs unless the host executes the Genocs startup initializer pipeline.
8. Do not assume BSON conventions are local to one host module. They are global process-wide registrations.
9. `IMongoDatabaseProvider.MongoClient` is sourced from the DI-registered `IMongoClient` singleton; treat it as the same composition pipeline.
10. Do not assume built-in client-side field encryption support exists in this package.
11. Do not assume transaction support exists unless MongoDB is configured to support sessions and transactions.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Persistence.MongoDB`, answer these questions:

1. Is the host using `Genocs.Core` and calling `genocs.Build()`?
2. Will the app call `app.UseGenocs()` so startup initializers can run?
3. Are entity keys `ObjectId`, or does the app need a custom-key repository?
4. Should the collection name come from `TableMappingAttribute`, type name, or explicit registration?
5. Should no-match async lookups throw or return null in the chosen repository path?
6. Does the app need raw driver access or a repository abstraction?
7. Is MongoDB transaction support actually available in the deployment topology?
8. Is startup seeding required, and can it safely run once per database key (`connectionString|database`) in the current process?
9. Does the app rely on a specific GUID representation or BSON convention that might conflict with the package defaults?

If any answer is unknown, prefer the smallest registration path and be explicit about collection names and repository type choices.

## Common Tasks And Safe Responses

### Task: "Register MongoDB for a service"

Safe response:

- use `AddMongo()` when only driver services are needed
- use `AddMongoWithRegistration()` only for `ObjectId` repository scenarios
- keep the `mongoDb` section explicit

### Task: "Store documents with Guid keys"

Safe response:

- use `AddMongo()`
- register `AddMongoRepository<TEntity, Guid>("collection-name")`
- inject `IMongoBaseRepository<TEntity, Guid>`

### Task: "Use a custom collection name"

Safe response:

- use `TableMappingAttribute` with `IMongoRepository<TEntity>`
- or use `AddMongoRepository<TEntity, TKey>(collectionName)` when explicit registration is preferred

### Task: "Seed MongoDB on startup"

Safe response:

- implement `IMongoSeeder`
- pass the seeder type into `AddMongo(...)`
- set `mongoDb.seed` to `true`
- ensure the host runs `UseGenocs()`

### Task: "Use Mongo transactions"

Safe response:

- inject `IMongoSessionFactory`
- mention that transaction support depends on the MongoDB deployment topology
- avoid promising unit-of-work integration from this package alone

### Task: "Add paging to Mongo queries"

Safe response:

- use `BrowseAsync(...)` with an `IPagedQuery`
- validate `orderBy` against real property names
- expect page numbering to normalize to `1` when invalid values are supplied

## Failure Modes And Troubleshooting

1. Repository services cannot be resolved.
Fix: Confirm the chosen registration path matches the injected type. `IMongoRepository<TEntity>` requires `AddMongoWithRegistration()`, while `IMongoBaseRepository<TEntity, TKey>` with custom keys requires `AddMongoRepository<TEntity, TKey>(...)`.

2. Mongo services appear unregistered with no startup error.
Fix: Check whether the `mongoDb` section exists and whether both `connectionString` and `database` are non-empty. `AddMongo()` can return without throwing.

3. Startup seeding never runs.
Fix: Ensure `mongoDb.seed` is `true`, the host called `genocs.Build()`, and the built app calls `UseGenocs()`.

4. `GetAsync(predicate)` throws in one app but returns null in another.
Fix: Check which repository registration path is in use. The package has different implementations behind the same interface contract.

5. Documents land in an unexpected collection.
Fix: Re-check `TableMappingAttribute` usage for `IMongoRepository<TEntity>` or the explicit `collectionName` used with `AddMongoRepository<TEntity, TKey>(...)`.

6. Sorting during paging throws at runtime.
Fix: Verify that `orderBy` matches an actual property name on the document type. The pagination helper builds an expression dynamically from the raw string.

7. Mongo transactions fail even though session creation works.
Fix: Confirm the MongoDB deployment supports transactions. Session support alone does not guarantee transaction support.

8. MongoDB encryption configuration appears missing.
Fix: This package no longer exposes built-in encryption configuration surface; implement encryption externally in application composition if required.

9. Another library sees unexpected Mongo serialization behavior.
Fix: Re-check the process-wide BSON conventions and serializers registered by this package.

10. Provider/session/repository behavior appears inconsistent across execution paths.
Fix: Verify everything is resolved through DI (`IMongoClient`, `IMongoDatabase`, `IMongoDatabaseProvider`, `IMongoSessionFactory`) and avoid manual client construction in app code.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Common`
- `Genocs.Messaging.Outbox.MongoDB`
- `Genocs.Saga.Integrations.MongoDB`
- `Genocs.Messaging.Outbox`
- `Genocs.Messaging`

## One-Line Recommendation For Agents

If you only know that `Genocs.Persistence.MongoDB` is installed, register Mongo with `AddMongo()` first, choose repository registration explicitly based on the entity key type and collection-mapping needs, and do not assume identical repository semantics across the package's two registration paths.
