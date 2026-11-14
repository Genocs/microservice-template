# Genocs.Saga.Integrations.MongoDB Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

Genocs.Saga.Integrations.MongoDB replaces the default in-memory saga persistence in `Genocs.Saga` with durable MongoDB-backed storage for both saga state and saga logs. The package exposes `UseMongoPersistence` extension methods on `ISagaBuilder` and registers `MongoSagaStateRepository` plus `MongoSagaLog` for the saga runtime. MongoDB client management (`IMongoClient` and `IMongoDatabase`) is delegated to `Genocs.Persistence.MongoDB`, so the saga integration shares a single MongoDB client pipeline with the rest of the host instead of constructing its own.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Saga.Integrations.MongoDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | MongoDB-backed saga state and log persistence |
| Typical startup APIs | `AddSaga` + `UseMongoPersistence` |
| Default configuration section | `mongoDb` (shared with `Genocs.Persistence.MongoDB`) |
| Collection names | `SagaData`, `SagaLog` |
| MongoDB client management | Delegated to `Genocs.Persistence.MongoDB` |

## Install

```bash
dotnet add package Genocs.Saga.Integrations.MongoDB
```

## Minimal Integration Recipe (Program.cs)

The recommended pattern registers MongoDB once via `Genocs.Persistence.MongoDB` and reuses the same `IMongoDatabase` from the saga integration.

```csharp
using Genocs.Core.Builders;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Saga;
using Genocs.Saga.Integrations.MongoDB;

var builder = WebApplication.CreateBuilder(args);

var genocs = GenocsBuilder.Create(builder);
genocs.AddMongo();

builder.Services.AddSaga(saga =>
{
    saga.UseMongoPersistence();
});

var app = builder.Build();
app.Run();
```

The configuration and explicit-options overloads also delegate MongoDB client setup to `Genocs.Persistence.MongoDB`:

```csharp
saga.UseMongoPersistence(builder.Configuration);              // reads "mongoDb" section
saga.UseMongoPersistence(builder.Configuration, "myMongo");   // custom section name
saga.UseMongoPersistence(new MongoOptions { ... });            // explicit options
```

## Configuration

Use the same `mongoDb` section consumed by `Genocs.Persistence.MongoDB`.

```json
{
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "genocs_saga"
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `connectionString` | `string` | MongoDB connection string used to create `MongoClient`. |
| `database` | `string` | Database name used for the `SagaData` and `SagaLog` collections. |
| `enableTracing` | `bool` | Enables MongoDB diagnostic activity tracing through `Genocs.Persistence.MongoDB`. |
| `setRandomDatabaseSuffix` | `bool` | Appends a random suffix to the database name. Useful for integration tests. |
| `guidRepresentationMode` | `enum` | GUID serialization mode (`Standard` by default). Inherited from `Genocs.Persistence.MongoDB`. |

`MongoOptions` is the same class exposed by `Genocs.Persistence.MongoDB.Configurations.MongoOptions` to keep configuration aligned across packages.

## Decision Matrix For Agents

| Goal | Preferred API | Why |
|---|---|---|
| Reuse the host MongoDB client (recommended) | `saga.UseMongoPersistence()` | Assumes `IMongoDatabase` was already registered (e.g., via `AddMongo`) |
| Configure persistence from application config | `saga.UseMongoPersistence(configuration)` | Reads `mongoDb` section and registers the shared MongoDB client through `Genocs.Persistence.MongoDB` |
| Configure persistence from explicit options | `saga.UseMongoPersistence(new MongoOptions { ... })` | Useful in tests and scenarios with programmatic configuration |
| Replace default in-memory persistence | `AddSaga(s => s.UseMongoPersistence(…))` | Keeps saga action contracts unchanged; only the persistence layer is replaced |
| Ensure state and logs survive restarts | Mongo-backed `ISagaStateRepository` and `ISagaLog` | Documents survive process restarts and multi-instance deployment |

## Behavior Notes / Constraints

- `UseMongoPersistence` must be called inside the `AddSaga` builder callback; calling it after `AddSaga` returns has no effect.
- MongoDB client/database registration is performed by `Genocs.Persistence.MongoDB.Extensions.ServiceCollectionExtensions.AddMongoClient`, which uses `TryAddSingleton`. If `AddMongo` already registered the client, `UseMongoPersistence` will not overwrite it.
- The configuration overload binds `MongoOptions` from the `mongoDb` section (or the supplied section name) and wraps invalid bindings as `InvalidConfigurationException` with the message `Could not deserialize given appsettings.`.
- Saga state writes use optimistic concurrency through the persisted `Version` field, so stale writes fail instead of silently overwriting newer state.
- Saga log entries persist `EntryId`, `MessageId`, and `Outcome` so the core runtime can support duplicate-delivery checks and compensation lifecycle tracking.
- Payload and state objects are serialized to BSON; the integration registers the `SagaAllowedTypes` convention pack (in addition to the `genocs` pack from `Genocs.Persistence.MongoDB`) to allow saga state to carry application-defined types via `ObjectSerializer`.
- Compensation and recovery depend on the host being able to deserialize persisted state and message types during rehydration.
- MongoDB permissions must include read and write access on the target database and the saga collections.

## Public Capability Map

| Capability | Surface |
|---|---|
| Reuse the host MongoDB client | `UseMongoPersistence(ISagaBuilder)` |
| Configure Mongo persistence from host configuration | `UseMongoPersistence(ISagaBuilder, IConfiguration, string)` |
| Configure Mongo persistence from explicit settings | `UseMongoPersistence(ISagaBuilder, MongoOptions)` |
| Durable versioned saga state storage | `ISagaStateRepository` backed by `MongoSagaStateRepository` |
| Durable outcome-aware saga log storage | `ISagaLog` backed by `MongoSagaLog` |

## Dependencies

- `Genocs.Core`
- `Genocs.Saga`
- `Genocs.Persistence.MongoDB`
- `Microsoft.Extensions.Configuration`

## Migration Notes

- The previous `MongoOptions` class in `Genocs.Saga.Integrations.MongoDB.Configurations` has been removed. Use `Genocs.Persistence.MongoDB.Configurations.MongoOptions` instead.
- The default configuration section changed from `sagaMongoDb` to `mongoDb` to match `Genocs.Persistence.MongoDB`. Hosts that need a separate MongoDB instance for sagas can pass a custom section name to `UseMongoPersistence(IConfiguration, sectionName)`.
- The saga integration no longer constructs its own `MongoClient`. The MongoDB client pipeline is now provided by `Genocs.Persistence.MongoDB` so the same client is reused when `AddMongo` is called by the host.

## Troubleshooting

1. Saga still behaves as if using in-memory persistence after adding this package.
Fix: Ensure `UseMongoPersistence` is called inside the `AddSaga(saga => { ... })` builder callback during startup; calling it after `AddSaga` returns has no effect.
2. `UseMongoPersistence()` (parameterless) fails to resolve `IMongoDatabase`.
Fix: Register MongoDB before `AddSaga` by calling `AddMongo()` from `Genocs.Persistence.MongoDB`, or use one of the `UseMongoPersistence(IConfiguration, ...)` / `UseMongoPersistence(MongoOptions)` overloads which register the client themselves.
3. Startup or the first saga operation throws `InvalidConfigurationException` while loading Mongo settings.
Fix: Validate that the configured section binds correctly, and confirm `connectionString`, `database`, network connectivity, and MongoDB credentials. The package wraps binding failures with a generic deserialization error message rather than detailed option validation output.
4. Compensation history is missing or unreadable after a deployment update.
Fix: Maintain backward compatibility in serialized saga message and state contracts across versions; avoid renaming properties or changing types stored in existing saga log documents.
