# Genocs.Saga.Integrations.Redis Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request package version and configuration details.

## Purpose

`Genocs.Saga.Integrations.Redis` replaces default in-memory saga persistence with Redis-backed saga state and saga log storage for distributed deployments. The current implementation uses compare-and-set semantics for saga state writes and a serialized log payload stored through Redis-backed distributed cache services.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Saga.Integrations.Redis` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Redis persistence provider for saga state and logs |
| Main APIs | `UseRedisPersistence(ISagaBuilder, SagaRedisOptions)`, `UseRedisPersistence(ISagaBuilder, string, IConfiguration)` |

## Install

```bash
dotnet add package Genocs.Saga.Integrations.Redis
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Saga;
using Genocs.Saga.Integrations.Redis;
using Genocs.Saga.Integrations.Redis.Configurations;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var redisSettings = builder.Configuration
	.GetSection(SagaRedisOptions.Position)
	.Get<SagaRedisOptions>()
	?? throw new InvalidOperationException("Missing sagaRedis configuration.");

builder.Services.AddSaga(saga =>
{
    saga.UseRedisPersistence(redisSettings);
});

var app = builder.Build();
app.Run();
```

## Configuration

Use `SagaRedisOptions` directly for normal hosts. The string-plus-configuration overload is available, but it expects the section value itself to be serialized JSON rather than a normal nested configuration object.

```json
{
    "sagaRedis": {
        "enabled": true,
        "configuration": "localhost:6379",
        "instanceName": "genocs-saga"
    }
}
```

| Setting | Type | Description |
|---|---|---|
| `enabled` | `bool` | Option flag available in the model. Registration still depends on calling `UseRedisPersistence(...)`. |
| `configuration` | `string` | Redis connection string passed to `AddStackExchangeRedisCache`. |
| `instanceName` | `string` | Cache key prefix used for saga state and log entries. |

The current options type is `SagaRedisOptions` and its default section name is `sagaRedis`.

## Decision Matrix For Agents

| Goal | Preferred API |
|---|---|
| Configure Redis persistence with explicit values | `UseRedisPersistence(ISagaBuilder, SagaRedisOptions)` |
| Configure Redis persistence from a normal `appsettings.json` object | Bind `SagaRedisOptions` in the host, then call `UseRedisPersistence(ISagaBuilder, SagaRedisOptions)` |
| Configure Redis persistence from a JSON string section value | `UseRedisPersistence(ISagaBuilder, string, IConfiguration)` |
| Replace in-memory saga persistence | Call `UseRedisPersistence(...)` inside `AddSaga(...)` |
| Keep saga action contracts unchanged | Use Redis integration only at registration time |

## Behavior Notes / Constraints

- Must be configured inside `AddSaga(...)` registration.
- The `UseRedisPersistence(ISagaBuilder, string, IConfiguration)` overload reads `configuration.GetSection(sectionName).Value`, so it expects that section value to already contain serialized JSON. It does not bind nested child properties.
- For standard nested `appsettings.json` sections, bind `SagaRedisOptions` in the host and pass the options overload instead.
- Saga state writes use optimistic concurrency through Redis compare-and-set semantics in `RedisSagaStateStore`.
- Saga log updates rewrite the serialized log payload for the saga key; they do not use per-entry compare-and-set semantics.
- Persisted payload compatibility affects rehydration after contract changes.

## Public Capability Map

| Capability | Surface |
|---|---|
| Register Redis persistence from explicit options | `UseRedisPersistence(ISagaBuilder, SagaRedisOptions)` |
| Register Redis persistence from a JSON string configuration value | `UseRedisPersistence(ISagaBuilder, string, IConfiguration)` |
| Redis-backed versioned saga state storage | `ISagaStateRepository` backed by `RedisSagaStateRepository` |
| Redis-backed outcome-aware saga log storage | `ISagaLog` backed by `RedisSagaLog` |

## Dependencies

- `Genocs.Saga`
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `Microsoft.Extensions.Configuration`
- `Newtonsoft.Json`

## Troubleshooting

1. Saga data is not shared across service instances.
Fix: Verify Redis connectivity and ensure `UseRedisPersistence(...)` is called in startup.
2. Startup throws during Redis settings binding.
Fix: If you use `UseRedisPersistence(ISagaBuilder, string, IConfiguration)`, validate that the selected section value is serialized JSON, not a nested object. For normal `appsettings.json` sections, bind `SagaRedisOptions` in the host and call the options overload instead.
3. Saga state or log payloads fail to rehydrate after deployment.
Fix: Keep serialized saga contracts backward-compatible when evolving message and state types.
