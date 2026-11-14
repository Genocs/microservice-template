# Genocs.Messaging.Outbox.MongoDB Agent Reference

## Agent Operating Mode

- Assume `Genocs.Messaging.Outbox.MongoDB` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat the public provider extension plus the documented companion outbox contracts as the only safe API surface.
- Generate durable MongoDB-backed outbox and inbox persistence wiring only when `Genocs.Messaging.Outbox` and `Genocs.Persistence.MongoDB` are both present.
- Do not invent lease-based claiming, exactly-once guarantees, or transparent schema migration behavior for stored messages.
- If package composition is unclear, ask whether `Genocs.Core`, `Genocs.Messaging.Outbox`, `Genocs.Persistence.MongoDB`, and a concrete `IBusPublisher` provider are all installed.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Messaging.Outbox.MongoDB` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | MongoDB storage provider for the Genocs outbox runtime |
| Main value | Durable `IMessageOutbox` and `IMessageOutboxAccessor` implementations backed by MongoDB, plus TTL index initialization for processed inbox and outbox records |
| Public entry point in this package | `AddMongo(this IMessageOutboxConfigurator)` |
| Requires | `Genocs.Messaging.Outbox`, `Genocs.Persistence.MongoDB`, MongoDB, and usually a bus provider when outbox processing is enabled |

## What This Package Is For

Use `Genocs.Messaging.Outbox.MongoDB` when you need to:

- switch the base outbox runtime from in-memory storage to MongoDB-backed storage
- persist outgoing outbox records across process restarts
- persist processed inbound message IDs across process restarts
- customize inbox and outbox collection names through the shared `outbox` section
- create TTL cleanup indexes for processed records when `outbox.expiry` is positive
- keep using the base package contracts `IMessageOutbox` and `IMessageOutboxAccessor` while changing only the storage provider

## What This Package Does Not Do By Itself

Do not assume `Genocs.Messaging.Outbox.MongoDB` can:

- register MongoDB connectivity by itself
- replace `AddMessageOutbox(...)` from the base outbox package
- register `IBusPublisher` or publish messages directly to a broker on its own
- add a new configuration section beyond the existing `outbox` and `mongoDb` sections
- guarantee exactly-once delivery or exactly-once inbound processing
- claim or lock pending outbox records before publish in multi-instance deployments
- migrate or version stored payloads automatically when message types change
- automatically honor custom JSON serializer settings from the host application
- guarantee atomic MongoDB transaction behavior for the package's own inbox-processing flow

## Safe Default Mental Model

Treat `Genocs.Messaging.Outbox.MongoDB` as four things:

1. A provider extension selected through `AddMessageOutbox(outbox => outbox.AddMongo())`
2. A registration layer that binds Mongo repositories for `InboxMessage` and `OutboxMessage`
3. A serializer and rehydrator that persists JSON plus assembly-qualified type names
4. A startup initializer that creates TTL indexes on `ProcessedAt` when cleanup is enabled

If a user asks for durable outbox semantics across several instances, remember that this provider persists records durably, but it does not add lease or claim semantics around unsent-message pickup.

## Fast Start Recipes

### Recipe 1: Register The MongoDB Outbox Provider

Use this when the host wants durable MongoDB storage for the base outbox runtime.

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Persistence.MongoDB.Extensions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddMongo()
        .AddMessageOutbox(outbox => outbox.AddMongo());

genocs.Build();

var app = builder.Build();
app.UseGenocs();
app.Run();
```

Effect:

- registers MongoDB connectivity through `Genocs.Persistence.MongoDB`
- registers Mongo-backed implementations of `IMessageOutbox` and `IMessageOutboxAccessor`
- binds the inbox and outbox collections from `outbox.inboxCollection` and `outbox.outboxCollection`
- registers a startup initializer that creates TTL indexes when cleanup is enabled
- keeps the base outbox processor behavior from `Genocs.Messaging.Outbox`

Important behavior:

- this package does not call `.AddMongo()` for you
- this package does not replace `.AddMessageOutbox(...)`; it plugs into it
- if `outbox.enabled` is `true`, the base package also registers the polling `OutboxProcessor`, which requires a concrete `IBusPublisher`

### Recipe 2: Buffer An Outgoing Message Durably

Use this when application code should persist the message first and let the base outbox processor publish later.

```csharp
using Genocs.Messaging.Outbox;

public sealed class OrderOutboxWriter(IMessageOutbox messageOutbox)
{
        public Task EnqueueAsync(OrderCreated message, CancellationToken cancellationToken)
                => messageOutbox.SendAsync(
                        message,
                        originatedMessageId: null,
                        messageId: Guid.NewGuid().ToString("N"),
                        correlationId: Activity.Current?.TraceId.ToString(),
                        spanContext: Activity.Current?.Id,
                        headers: new Dictionary<string, object>
                        {
                                ["tenant"] = "default"
                        },
                        cancellationToken: cancellationToken);
}

public sealed record OrderCreated(Guid Id);
```

Important behavior:

- pass a concrete `Dictionary<string, object>` for headers
- the provider stores serialized JSON and type names, not the live object graph itself
- publication still happens later through the base outbox processor and the configured `IBusPublisher`

### Recipe 3: Make Inbound Message Handling Durable And Idempotent

Use this when a subscriber or handler must skip messages that were already processed in a previous run.

```csharp
using Genocs.Messaging.Outbox;

public sealed class OrderCreatedConsumer(IMessageOutbox messageOutbox)
{
        public Task HandleAsync(string messageId, CancellationToken cancellationToken)
                => messageOutbox.HandleAsync(
                        messageId,
                        async () =>
                        {
                                await Task.CompletedTask;
                        },
                        cancellationToken);
}
```

Effect:

- checks the inbox collection for the `messageId`
- skips the handler when the ID already exists
- writes the processed message ID to MongoDB after the handler completes successfully

Important behavior:

- deduplication is durable across restarts because processed IDs are stored in MongoDB
- the provider records the inbox entry after handler completion, so do not assume concurrent duplicate deliveries are fully serialized or exactly-once

### Recipe 4: Enable Cleanup Of Processed Records

Use this when processed inbox and outbox records should expire automatically in MongoDB.

```json
{
    "outbox": {
        "enabled": true,
        "expiry": 3600,
        "inboxCollection": "inbox",
        "outboxCollection": "outbox"
    },
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "orders"
    }
}
```

Important behavior:

- TTL indexes are created only when `outbox.enabled` is `true`
- TTL indexes are created only when `outbox.expiry > 0`
- the TTL initializer runs through the Genocs startup initializer pipeline, so the host must call `genocs.Build()` and `app.UseGenocs()`
- MongoDB TTL deletion is asynchronous; do not expect immediate cleanup exactly at the expiry boundary

## Core Entry Points

| API | Package | Use it for | Important behavior | Common mistake |
|---|---|---|---|---|
| `AddMongo(this IMessageOutboxConfigurator)` | `Genocs.Messaging.Outbox.MongoDB` | Select the MongoDB storage provider for the base outbox runtime | Registers Mongo repositories for `InboxMessage` and `OutboxMessage`, wires Mongo-backed `IMessageOutbox` and `IMessageOutboxAccessor`, registers TTL initializer, and configures BSON mapping for `OutboxMessage` | Assuming it also registers MongoDB connectivity or the base outbox runtime |
| `AddMessageOutbox(...)` | `Genocs.Messaging.Outbox` | Register the shared outbox runtime | Still owns the base outbox options and polling processor | Treating the MongoDB package as a standalone registration path |
| `IMessageOutbox` | `Genocs.Messaging.Outbox` | Buffer outgoing messages and deduplicate inbound handling | The Mongo provider backs this contract with persisted inbox and outbox records | Assuming this interface is declared by the MongoDB package |
| `IMessageOutboxAccessor` | `Genocs.Messaging.Outbox` | Load and mark unsent outbox records | Used by the base `OutboxProcessor` to publish and mark messages | Depending on it from normal application code when `IMessageOutbox` is enough |
| `AddMongo()` | `Genocs.Persistence.MongoDB` | Register `IMongoDatabase`, repositories, and session factory support | Must be called separately before the outbox provider can work | Assuming the outbox provider brings Mongo infrastructure with it |

## Storage And Serialization Semantics

### Stored Collections

The provider registers two Mongo repositories with `string` keys:

- `InboxMessage` in `outbox.inboxCollection` or `inbox`
- `OutboxMessage` in `outbox.outboxCollection` or `outbox`

### What The Provider Persists

For outgoing messages, the provider stores:

- `Id`
- `OriginatedMessageId`
- `CorrelationId`
- `SpanContext`
- `Headers`
- `MessageType`
- `MessageContextType`
- `SerializedMessage`
- `SerializedMessageContext`
- `SentAt`
- `ProcessedAt`

The live `Message` and `MessageContext` object properties are explicitly excluded from BSON mapping and are rehydrated later from the serialized string fields.

### Rehydration Rules

When the base outbox processor asks for unsent messages, the provider:

- loads unprocessed `OutboxMessage` documents from MongoDB
- resolves types through `Type.GetType(assemblyQualifiedName)`
- deserializes with a package-local `System.Text.Json` configuration

Serializer behavior in this package:

- camelCase naming
- case-insensitive property matching
- string enum conversion using camelCase
- numeric strings can be read into number properties

Safe guidance:

- keep message types loadable by their assembly-qualified names
- avoid renaming, moving, or deleting message types while unsent records still exist
- do not assume application-wide custom serializer settings apply here

### Header Constraint

`SendAsync(...)` casts `headers` directly to `Dictionary<string, object>`.

Safe guidance:

- pass a concrete `Dictionary<string, object>`

Unsafe assumption:

- assuming any `IDictionary<string, object>` implementation works

## Processing And Concurrency Semantics

### Outgoing Message Processing

The base `OutboxProcessor` from `Genocs.Messaging.Outbox`:

- polls unsent records by `ProcessedAt == null`
- orders them by `SentAt`
- publishes them through `IBusPublisher`
- marks them processed afterward

This provider does not add a claim, lock, or lease step before publish.

Safe guidance:

- treat this as durable storage, not as a single-consumer claim-based queue

Unsafe assumption:

- assuming several app instances cannot publish the same unsent record

### Inbound Deduplication

`HandleAsync(messageId, handler)`:

- checks whether the inbox already contains the ID
- executes the handler when it does not
- inserts the inbox record after the handler succeeds

Safe guidance:

- treat this as durable best-effort deduplication

Unsafe assumption:

- assuming concurrent duplicate deliveries cannot both execute the handler body before one inbox insert wins

### Transactions Flag

When `outbox.disableTransactions` is `false`, the provider creates a Mongo session and starts a transaction in `HandleAsync(...)`.

Important limitation for source-blind consumers:

- the provider's own repository calls do not pass the created session into Mongo operations

Safe guidance:

- do not rely on this package alone for strict atomic MongoDB transaction coupling between handler side effects and inbox persistence

## Configuration Ownership

`Genocs.Messaging.Outbox.MongoDB` does not introduce a new configuration section.

It composes two existing sections:

- `outbox` from `Genocs.Messaging.Outbox`
- `mongoDb` from `Genocs.Persistence.MongoDB`

```json
{
    "outbox": {
        "enabled": true,
        "intervalMilliseconds": 5000,
        "expiry": 3600,
        "inboxCollection": "inbox",
        "outboxCollection": "outbox",
        "type": "sequential",
        "disableTransactions": false
    },
    "mongoDb": {
        "connectionString": "mongodb://localhost:27017",
        "database": "orders",
        "enableTracing": true,
        "seed": false,
        "setRandomDatabaseSuffix": false
    }
}
```

What this provider actively uses from `outbox`:

- `enabled`
- `expiry`
- `inboxCollection`
- `outboxCollection`
- `disableTransactions`

What the base outbox runtime still owns:

- `intervalMilliseconds`
- `type`
- registration of the polling `OutboxProcessor`

What Mongo connectivity still comes from:

- `mongoDb.connectionString`
- `mongoDb.database`
- the rest of `Genocs.Persistence.MongoDB`

## Public Capability Map

### Provider Registration

- `AddMongo(this IMessageOutboxConfigurator)`

### Companion Contracts Used By Consumers

- `IMessageOutbox`
- `IMessageOutboxAccessor`
- `OutboxOptions`
- `InboxMessage`
- `OutboxMessage`

### Provider Effects

- MongoDB-backed inbox persistence
- MongoDB-backed outbox persistence
- JSON payload persistence and later rehydration
- TTL index initialization for processed records

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume this package works without `AddMessageOutbox(...)` from the base package.
2. Do not assume this package registers MongoDB connectivity. Call `AddMongo()` from `Genocs.Persistence.MongoDB` explicitly.
3. Do not assume this package owns a separate configuration section. It reuses `outbox` and `mongoDb`.
4. Do not assume unsent messages are claimed or locked before publish. Several processors can observe the same pending records.
5. Do not assume `HandleAsync(...)` provides strict exactly-once processing under concurrent duplicate delivery.
6. Do not assume `disableTransactions = false` guarantees true atomic MongoDB transaction behavior for the provider itself.
7. Do not assume stored payloads survive type renames, namespace moves, or assembly changes. Rehydration depends on assembly-qualified type names.
8. Do not assume host-level custom JSON settings apply to persisted outbox payloads.
9. Do not assume any `IDictionary<string, object>` implementation is safe for `headers`. Pass `Dictionary<string, object>`.
10. Do not assume TTL cleanup runs unless the host executes the Genocs startup initializer pipeline with `UseGenocs()`.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Messaging.Outbox.MongoDB`, answer these questions:

1. Has the host already called `AddMongo()` from `Genocs.Persistence.MongoDB`?
2. Has the host already called `AddMessageOutbox(...)` from the base outbox package?
3. Is `outbox.enabled` meant to start the polling processor right now, and if so, is a concrete `IBusPublisher` registered?
4. Do inbox deduplication and outbox persistence need to survive restarts and several app instances?
5. Are message types stable enough to be rehydrated later by assembly-qualified type name?
6. Are headers being passed as a concrete `Dictionary<string, object>`?
7. Should processed records expire automatically, and will the host run `UseGenocs()` so TTL indexes can be created?
8. Does the design require true claim-based multi-instance outbox pickup or strict transactional semantics that this package does not provide by itself?

If any answer is unknown, prefer minimal durable registration and avoid promising stronger delivery or concurrency guarantees than the implementation actually provides.

## Common Tasks And Safe Responses

### Task: "Enable a durable MongoDB outbox"

Safe response:

- call `AddMongo()`
- call `AddMessageOutbox(outbox => outbox.AddMongo())`
- keep configuration in `outbox` and `mongoDb`

### Task: "Persist an integration event before publish"

Safe response:

- inject `IMessageOutbox`
- call `SendAsync(...)`
- pass `headers` as `Dictionary<string, object>`
- mention that actual publish still happens later through the base outbox processor

### Task: "Make subscriber processing survive restarts"

Safe response:

- wrap the handler body in `HandleAsync(messageId, ...)`
- require a stable incoming message ID
- mention that concurrent duplicates can still need careful upstream handling

### Task: "Expire processed inbox and outbox records"

Safe response:

- set `outbox.expiry` to a positive value
- keep `outbox.enabled` true
- ensure the host runs `UseGenocs()` so the TTL initializer executes

### Task: "Tune outbox polling"

Safe response:

- point to the base outbox settings `intervalMilliseconds` and `type`
- clarify that this MongoDB package is only the storage provider, not the polling runtime owner

## Failure Modes And Troubleshooting

1. DI fails for Mongo-backed outbox services.
Fix: Ensure `AddMongo()` from `Genocs.Persistence.MongoDB` runs before `AddMessageOutbox(outbox => outbox.AddMongo())`.

2. Pending outbox records fail to publish after a deployment.
Fix: Re-check whether the stored `MessageType` or `MessageContextType` can still be resolved by `Type.GetType(...)` and deserialized successfully.

3. Processed records never expire.
Fix: Ensure `outbox.enabled` is `true`, `outbox.expiry > 0`, the app runs `UseGenocs()`, and MongoDB has permission to create indexes.

4. `SendAsync(...)` throws when headers are supplied.
Fix: Pass a concrete `Dictionary<string, object>` rather than another `IDictionary<string, object>` implementation.

5. Durable records exist but nothing is ever published.
Fix: Ensure the base outbox runtime is enabled, `intervalMilliseconds` is positive, and a concrete `IBusPublisher` provider is registered.

6. The same pending message appears to publish more than once across instances.
Fix: This provider does not add claim or lease semantics around `ProcessedAt == null` polling. Use consumer idempotency and architecture-level deduplication accordingly.

7. The same inbound message still produces duplicate side effects under concurrency.
Fix: `HandleAsync(...)` records the inbox entry after handler success, so it is durable but not a strict concurrency lock for duplicate deliveries.

8. Transaction-related expectations do not match observed behavior.
Fix: Treat `disableTransactions` as a session toggle, not proof of strict atomic persistence semantics in this provider.

## Related Packages To Ask About

- `Genocs.Messaging.Outbox`
- `Genocs.Persistence.MongoDB`
- `Genocs.Messaging`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Core`

## One-Line Recommendation For Agents

If you only know that `Genocs.Messaging.Outbox.MongoDB` is installed, generate `AddMongo()` plus `AddMessageOutbox(outbox => outbox.AddMongo())`, write through `IMessageOutbox`, keep message contracts stable for later rehydration, and do not promise lease-based multi-instance delivery or strict transactional guarantees that this provider does not implement.
