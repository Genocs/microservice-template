# Genocs.Messaging.Outbox Agent Reference

## Agent Operating Mode

- Assume `Genocs.Messaging.Outbox` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, interfaces, options, and message models as the only safe API surface.
- Generate outbox-aware publish and idempotent-consume code only. Do not invent provider-specific storage behavior unless a companion package such as `Genocs.Messaging.Outbox.MongoDB` is explicitly confirmed.
- If package composition is unclear, ask whether a concrete outbox storage provider and a concrete bus provider are both installed, because this package only defines the base runtime and one limited in-memory registration path.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Messaging.Outbox` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Transactional-outbox abstraction and polling publisher runtime |
| Main value | `IMessageOutbox` for buffered outgoing messages, `HandleAsync(...)` for inbox-style deduplication, `OutboxProcessor` for timer-based publish attempts, `OutboxOptions` for shared runtime configuration |
| Requires | A Genocs builder host, `IBusPublisher` when processing is enabled, and usually a durable provider package for production |

## What This Package Is For

Use `Genocs.Messaging.Outbox` when you need to:

- buffer outgoing bus messages before a background processor publishes them
- add inbox-style idempotency around inbound message handling through stable message IDs
- configure a polling outbox processor under the shared `outbox` section
- choose an outbox storage implementation through an `IMessageOutboxConfigurator`
- build on the shared outbox contracts before selecting MongoDB or another durable store

## What This Package Does Not Do By Itself

Do not assume `Genocs.Messaging.Outbox` can:

- persist messages durably by itself in production-safe storage
- publish directly to RabbitMQ, Azure Service Bus, or another broker without a separate `IBusPublisher` implementation
- participate automatically in database transactions by itself
- guarantee exactly-once delivery
- guarantee cross-instance idempotency with the built-in in-memory registration path
- serialize and rehydrate payloads for every provider in the base package
- expose its own HTTP endpoints, middleware, or management UI

## Safe Default Mental Model

Treat `Genocs.Messaging.Outbox` as four things:

1. A host registration extension through `AddMessageOutbox(...)`
2. An abstraction for writing outgoing messages and deduplicating incoming ones through `IMessageOutbox`
3. A polling background publisher that loads unsent messages from `IMessageOutboxAccessor`
4. A base package that expects a storage implementation and a bus provider to make the full pattern useful

If a user asks for durable storage, transactions, or message rehydration guarantees, identify the companion provider package first.

## Fast Start Recipes

### Recipe 1: Register The Base Outbox Runtime

Use this when the host wants the outbox option model and provider-selection hook.

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.Outbox;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
		.AddGenocs()
		.AddMessageOutbox();

genocs.Build();

var app = builder.Build();
app.Run();
```

Important behavior:

- `AddMessageOutbox()` binds `OutboxOptions` from the `outbox` section
- provider registration is explicit; if no configure callback is supplied, startup throws with guidance to configure a provider
- if `outbox.enabled` is `true`, the package also registers the hosted `OutboxProcessor`
- the processor requires `IBusPublisher` at runtime

Safe guidance:

- use this as the base registration shape
- for production or durable storage, add a concrete provider such as `Genocs.Messaging.Outbox.MongoDB`

### Recipe 2: Register A Durable Provider

Use this when the host needs actual persisted outbox and inbox records.

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
```

Use this when durable storage matters more than a minimal in-memory setup.

### Recipe 3: Buffer An Outgoing Message

Use this when application code should write to the outbox instead of publishing to the bus directly.

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

Effect:

- the message is stored by the selected outbox implementation
- later, `OutboxProcessor` publishes the stored message through `IBusPublisher`
- the base package itself does not promise transactional coupling with your database write

### Recipe 4: Deduplicate Inbound Message Handling

Use this when a subscriber or handler must ignore already-processed messages.

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

- if the same `messageId` has already been recorded as processed, the handler body is skipped
- if the handler succeeds, the implementation records the processed message ID
- durability and cross-instance behavior depend entirely on the selected provider

### Recipe 5: Configure Polling Behavior

Use this when the host must control processing cadence and outbox strategy.

```json
{
	"outbox": {
		"enabled": true,
		"expiry": 3600,
		"intervalMilliseconds": 5000,
		"inboxCollection": "inbox",
		"outboxCollection": "outbox",
		"type": "sequential",
		"disableTransactions": false
	}
}
```

Use `type: "parallel"` only when the selected provider and publish flow can tolerate batch marking semantics.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddMessageOutbox(...)` | Register outbox options, provider configuration, and optional background processing | Requires an explicit configure callback; throws when provider configuration is omitted; adds `OutboxProcessor` only when `outbox.enabled` is `true` | Assuming it installs durable storage or a broker client by itself |
| `AddInMemory()` | Select the built-in in-memory outbox registration path | Registers `IMessageOutbox` with the internal in-memory implementation | Assuming it provides durable multi-instance semantics |
| `IMessageOutbox.HandleAsync(...)` | Deduplicate inbound message processing by message ID | Skips handler execution when the message ID was already recorded | Assuming this guarantees exactly-once delivery across app restarts with in-memory storage |
| `IMessageOutbox.SendAsync<T>(...)` | Store an outgoing message for later publication | Stores message metadata such as IDs, headers, and correlation values for the selected provider | Assuming it publishes immediately to the bus |
| `IMessageOutboxAccessor.GetUnsentAsync()` | Load pending outbox entries for background processing | Internal runtime surface used by `OutboxProcessor` | Depending on it from application code when `IMessageOutbox` is enough |
| `IMessageOutboxAccessor.ProcessAsync(...)` | Mark one or more outbox entries as processed | Sequential mode marks each message after publish; parallel mode marks a batch after the publish loop | Assuming it re-publishes failed messages or manages retries itself |
| `OutboxOptions` | Configure polling and provider-related outbox settings | Shared option model for base and provider packages | Assuming every option has meaning in every provider |

## Processing Semantics

### Background Processor Lifecycle

- `OutboxProcessor` is registered only when `outbox.enabled` is `true`.
- On startup it validates that `intervalMilliseconds` is greater than zero.
- It runs on a `Timer`, not on a queue-triggered or broker-triggered model.
- Each tick creates a new DI scope, resolves `IMessageOutboxAccessor`, and loads unsent messages.
- Messages are ordered by `SentAt` before publication.

### Publish Flow

- For each unsent message, the processor calls `IBusPublisher.PublishAsync(...)`.
- The published payload is the stored `OutboxMessage.Message` object.
- Correlation ID, span context, message context, and headers are forwarded to `IBusPublisher`.
- The base package does not define retries, poison-message handling, or dead-letter behavior.

### Sequential vs Parallel Outbox Types

`outbox.type` supports:

- `Sequential`
- `Parallel`

Actual behavior in this package:

- both modes publish messages in a loop
- `Sequential` marks each message processed immediately after publish
- `Parallel` delays marking until the full loaded batch has been published

Do not interpret `Parallel` as true concurrent publishing in the base processor. It changes the marking strategy, not the publish loop into parallel tasks.

## Configuration Ownership

`Genocs.Messaging.Outbox` owns the `outbox` section through `OutboxOptions`.

```json
{
	"outbox": {
		"enabled": true,
		"expiry": 3600,
		"intervalMilliseconds": 5000,
		"inboxCollection": "inbox",
		"outboxCollection": "outbox",
		"type": "sequential",
		"disableTransactions": false
	}
}
```

What the base package actively uses:

- `enabled`
- `expiry`
- `intervalMilliseconds`
- `type`

What the base package carries mainly for provider packages:

- `inboxCollection`
- `outboxCollection`
- `disableTransactions`

Provider examples:

- `Genocs.Messaging.Outbox.MongoDB` uses collection names and transaction settings

## Message Models

### `OutboxMessage`

The outbox storage model carries:

- `Id`
- `OriginatedMessageId`
- `CorrelationId`
- `SpanContext`
- `Headers`
- `MessageType`
- `MessageContextType`
- `Message`
- `MessageContext`
- `SerializedMessage`
- `SerializedMessageContext`
- `SentAt`
- `ProcessedAt`

### `InboxMessage`

The inbox deduplication model carries:

- `Id`
- `ProcessedAt`

These types matter mostly to provider implementations and storage concerns, not to typical application code.

## Public Capability Map

### Registration And Hosting

- `AddMessageOutbox(...)`
- `AddInMemory()`
- `IMessageOutboxConfigurator`
- `OutboxOptions`

### Application-Facing Contracts

- `IMessageOutbox`
- `HandleAsync(...)`
- `SendAsync<T>(...)`

### Runtime Processing Contracts

- `IMessageOutboxAccessor`
- `GetUnsentAsync()`
- `ProcessAsync(OutboxMessage)`
- `ProcessAsync(IEnumerable<OutboxMessage>)`

### Storage Models

- `OutboxMessage`
- `InboxMessage`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume the base package is a full durable outbox solution by itself.
2. Do not assume `AddMessageOutbox()` is enough for production-safe persistence.
3. Do not assume `SendAsync(...)` publishes immediately.
4. Do not assume `HandleAsync(...)` gives global exactly-once guarantees.
5. Do not assume `Parallel` means true concurrent publishing.
6. Do not assume `disableTransactions` does anything unless the selected provider implements transactions.
7. Do not assume collection-name settings matter unless a storage provider uses them.
8. Do not assume the base package owns broker configuration. That belongs to the installed `IBusPublisher` provider.
9. Do not assume payload serialization format from the base package. That depends on the provider implementation.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Messaging.Outbox`, answer these questions:

1. Which outbox storage provider is installed?
2. Which bus provider registers `IBusPublisher`?
3. Does the app need durable storage across restarts and instances?
4. Should idempotency survive process restarts?
5. Is the outbox meant to be transactionally coupled to a database write?
6. Does the host actually want the polling processor enabled right now?
7. Are message type names and payload contracts stable enough for provider-side rehydration?

If any answer is unknown, prefer abstraction-level guidance and ask which durable provider package is installed.

## Common Tasks And Safe Responses

### Task: "Buffer an integration event"

Safe response:

- inject `IMessageOutbox`
- call `SendAsync(...)`
- mention that actual publish happens later through the processor

### Task: "Make subscriber handling idempotent"

Safe response:

- wrap the handler body in `HandleAsync(messageId, ...)`
- require a stable incoming message ID
- mention that provider choice determines durability of deduplication

### Task: "Enable a durable transactional outbox"

Safe response:

- keep `AddMessageOutbox(...)`
- add a provider such as `Genocs.Messaging.Outbox.MongoDB`
- configure both the base `outbox` section and the provider-specific infrastructure section

### Task: "Tune polling behavior"

Safe response:

- adjust `intervalMilliseconds`
- choose `Sequential` unless batch marking is explicitly desired
- keep `enabled` false when the processor should not run

## Failure Modes And Troubleshooting

1. The host fails when outbox processing starts.
Fix: Ensure `outbox.enabled` is `true` only when a valid `IBusPublisher` and a storage implementation for pending messages are registered.

2. No buffered messages are ever published.
Fix: Verify `outbox.enabled` is `true`, `intervalMilliseconds` is positive, and the selected provider persists messages that `IMessageOutboxAccessor` can load.

3. Duplicate inbound messages still run more than once.
Fix: Pass a stable `messageId` into `HandleAsync(...)` and use a durable provider if idempotency must survive restarts or multiple instances.

4. Processed records never disappear.
Fix: Set a positive `expiry` value and confirm the selected provider actually applies cleanup behavior.

5. Messages are buffered but lost after restart.
Fix: Replace the in-memory path with a durable provider such as `Genocs.Messaging.Outbox.MongoDB`.

6. `Parallel` mode does not improve throughput as expected.
Fix: In this package, `Parallel` changes when records are marked processed. It does not turn the publish loop into concurrent publishing.

## Related Packages To Ask About

- `Genocs.Messaging`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.Outbox.MongoDB`
- `Genocs.Persistence.MongoDB`
- `Genocs.Core`
- `Genocs.Saga`

## One-Line Recommendation For Agents

If you only know that `Genocs.Messaging.Outbox` is installed, generate code that writes through `IMessageOutbox` and treats the package as a base outbox runtime. Ask which durable storage provider and bus provider complete the actual delivery path.

## Quality Gate

Outbox changes are validated through [validate-messaging.mk](../validate-messaging.mk):

- `Genocs.Messaging.Outbox` must build for `net10.0` with `-warnaserror`.
- Messaging unit tests must pass before merge to guard outbox contract and registration behavior.
