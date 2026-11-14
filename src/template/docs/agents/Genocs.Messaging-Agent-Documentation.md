# Genocs.Messaging Agent Reference

## Agent Operating Mode

- Assume `Genocs.Messaging` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented interfaces, attributes, and extension methods as the only safe API surface.
- Generate broker-agnostic messaging code only. Do not invent RabbitMQ, Azure Service Bus, Kafka, MassTransit, or other transport behavior unless a concrete provider package is explicitly confirmed.
- If package composition is unclear, ask whether a transport package such as `Genocs.Messaging.RabbitMQ` is installed, because this base package does not publish or consume anything by itself.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Messaging` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Transport-agnostic message-bus abstraction and CQRS bridge layer |
| Main value | Stable `IBusPublisher` and `IBusSubscriber` contracts, command and event subscription helpers, bus-backed dispatcher bridge registrations, message metadata contracts |
| Requires | `Genocs.Core` and a concrete transport provider for runtime behavior |

## What This Package Is For

Use `Genocs.Messaging` when you need to:

- depend on a broker-neutral publish contract through `IBusPublisher`
- depend on a broker-neutral subscribe contract through `IBusSubscriber`
- bridge Genocs command dispatching to bus publishing
- bridge Genocs event dispatching to bus publishing
- subscribe command and event handlers through a transport subscriber with scoped DI resolution
- pass transport metadata through `MessageAttribute`
- read per-message delivery metadata through `IMessageProperties`
- carry a correlation context object across publish and consume flows

## What This Package Does Not Do By Itself

Do not assume `Genocs.Messaging` can:

- connect to RabbitMQ, Azure Service Bus, Kafka, or any other broker on its own
- register `IBusPublisher` or `IBusSubscriber` implementations by itself
- declare queues, exchanges, topics, subscriptions, or dead-letter infrastructure
- host a background consumer loop by itself
- register `ICorrelationContextAccessor` or `IMessagePropertiesAccessor` by itself
- dispatch queries over the bus
- persist messages, retries, or inbox or outbox state
- interpret `MessageAttribute` unless the installed provider chooses to honor it

## Safe Default Mental Model

Treat `Genocs.Messaging` as four things:

1. A stable bus abstraction through `IBusPublisher` and `IBusSubscriber`
2. A command and event bridge from `Genocs.Core` dispatchers to whatever bus provider is installed
3. A metadata surface for message properties and correlation context
4. A package that is useful only when combined with a transport provider or a higher-level package such as outbox support

If a user asks for actual broker behavior, identify the missing provider package first.

## Fast Start Recipes

### Recipe 1: Bridge Commands And Events To A Bus Provider

Use this when the application already installs a concrete `IBusPublisher` implementation and wants Core command and event dispatchers to publish over that bus.

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.CQRS;
using Genocs.Messaging.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddCommandHandlers()
    .AddEventHandlers()
    .AddServiceBusCommandDispatcher()
    .AddServiceBusEventDispatcher();

await genocs.AddRabbitMQAsync();
genocs.Build();

var app = builder.Build();
app.UseRabbitMQ();
app.Run();
```

Effect:

- `ICommandDispatcher.SendAsync(...)` publishes commands through `IBusPublisher`
- `IEventDispatcher.PublishAsync(...)` publishes events through `IBusPublisher`
- actual delivery depends entirely on the installed provider package

Important behavior:

- despite the method names, `AddServiceBusCommandDispatcher()` and `AddServiceBusEventDispatcher()` are generic bus bridges in this package
- they do not install Azure Service Bus specifically
- they only work when `IBusPublisher` and `ICorrelationContextAccessor` are available from another package

### Recipe 2: Subscribe A Command Handler Through A Provider Subscriber

Use this when the transport provider exposes an `IBusSubscriber` and the host already registered command handlers.

```csharp
using Genocs.Common.CQRS.Commands;
using Genocs.Messaging;
using Genocs.Messaging.CQRS;

public sealed record CreateOrder(Guid CustomerId) : ICommand;

public static class SubscriptionSetup
{
    public static IBusSubscriber AddSubscriptions(this IBusSubscriber subscriber)
        => subscriber.SubscribeCommand<CreateOrder>();
}
```

Effect:

- the helper resolves `ICommandHandler<CreateOrder>` from a fresh DI scope for each delivered message
- the provider still owns the actual message pump, acknowledgment, retry, and dead-letter behavior

### Recipe 3: Subscribe An Event Handler Through A Provider Subscriber

Use this when the transport provider delivers integration or domain events and the host already registered event handlers.

```csharp
using Genocs.Common.CQRS.Events;
using Genocs.Messaging;
using Genocs.Messaging.CQRS;

public sealed record OrderCreated(Guid Id) : IEvent;

public static class SubscriptionSetup
{
    public static IBusSubscriber AddSubscriptions(this IBusSubscriber subscriber)
        => subscriber.SubscribeEvent<OrderCreated>();
}
```

Effect:

- the helper resolves `IEventHandler<OrderCreated>` from a fresh DI scope for each delivered message
- the base package does not fan out to multiple handlers by itself; it asks DI for one `IEventHandler<T>` in the current scope

### Recipe 4: Publish A Message Directly

Use this when application code wants to publish a message without going through `ICommandDispatcher` or `IEventDispatcher`.

```csharp
using Genocs.Messaging;

public sealed class OrderPublisher(IBusPublisher busPublisher)
{
    public Task PublishAsync(OrderCreated message, CancellationToken cancellationToken)
        => busPublisher.PublishAsync(
            message,
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

Use this when the caller needs explicit message identifiers, correlation identifiers, span context, or transport headers.

### Recipe 5: Describe Transport Hints On A Message Type

Use this when a provider can read transport-specific conventions from the message contract.

```csharp
using Genocs.Messaging;

[Message(exchange: "orders", routingKey: "orders.created", queue: "orders-created", external: true)]
public sealed record OrderCreated(Guid Id);
```

Important behavior:

- `MessageAttribute` is metadata only in this package
- the installed provider may honor all, some, or none of these values
- for example, a provider can ignore exchange, routing-key, or queue overrides based on its own configuration

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `IBusPublisher.PublishAsync<T>(...)` | Publish any class-based message through the installed provider | Accepts message identifiers, correlation identifiers, span context, arbitrary message context, and headers | Assuming the base package queues or delivers anything without a provider |
| `IBusSubscriber.Subscribe<T>(...)` | Register a typed subscription callback against the installed provider | The callback receives `IServiceProvider`, the typed message, and an untyped message context object | Assuming this starts a background consumer automatically |
| `busPublisher.SendAsync<TCommand>(...)` | Publish a command through the bus with a message context | Thin CQRS helper over `IBusPublisher.PublishAsync(...)` | Treating it like an in-process `ICommandDispatcher` |
| `busPublisher.PublishAsync<TEvent>(...)` in `Genocs.Messaging.CQRS` | Publish an event through the bus with a message context | Thin CQRS helper over `IBusPublisher.PublishAsync(...)` | Confusing it with the base `PublishAsync<T>(...)` overload |
| `busSubscriber.SubscribeCommand<T>()` | Resolve and execute `ICommandHandler<T>` for an incoming command message | Creates a fresh scope and resolves one handler from DI | Assuming handler registration comes from this package |
| `busSubscriber.SubscribeEvent<T>()` | Resolve and execute `IEventHandler<T>` for an incoming event message | Creates a fresh scope and resolves one handler from DI | Assuming all matching handlers are invoked automatically |
| `AddServiceBusCommandDispatcher()` | Register `ICommandDispatcher` backed by bus publishing | Uses the internal `ServiceBusMessageDispatcher` and requires `IBusPublisher` plus `ICorrelationContextAccessor` | Assuming this installs a real transport client |
| `AddServiceBusEventDispatcher()` | Register `IEventDispatcher` backed by bus publishing | Uses the same internal bridge as commands | Assuming this also registers `IQueryDispatcher` |
| `MessageAttribute` | Attach provider-readable routing metadata to a message type | Exposes `Exchange`, `RoutingKey`, `Queue`, and `External` only | Assuming every provider honors every property |
| `IMessageProperties` and `IMessagePropertiesAccessor` | Read metadata about the currently handled inbound message | The accessor type is only a contract here; providers must populate it | Assuming it is available in every host |
| `ICorrelationContextAccessor` | Read or set an ambient correlation-context object | Uses an opaque `object` contract rather than a typed model | Assuming the base package registers a default implementation |

## Messaging Semantics

### Publish Surface

- `IBusPublisher` is the central abstraction for message publication.
- Messages are constrained only to `class`.
- Publication can carry:
  - `messageId`
  - `correlationId`
  - `spanContext`
  - `messageContext`
  - arbitrary `headers`
- The base package does not define serialization, routing, retries, acknowledgments, or delivery guarantees.

### Subscribe Surface

- `IBusSubscriber` is the central abstraction for message consumption.
- The raw contract is `Subscribe<T>(Func<IServiceProvider, T, object, Task> handle)`.
- The third callback parameter is an untyped message context object supplied by the provider.
- The base package does not define when or how subscriptions are activated.

### CQRS Bridge Surface

- `SubscribeCommand<T>()` maps an incoming message to `ICommandHandler<T>.HandleAsync(...)`.
- `SubscribeEvent<T>()` maps an incoming message to `IEventHandler<T>.HandleAsync(...)`.
- `AddServiceBusCommandDispatcher()` maps `ICommandDispatcher` to bus publishing.
- `AddServiceBusEventDispatcher()` maps `IEventDispatcher` to bus publishing.
- There is no bus-backed query dispatcher in this package.

## Configuration Ownership

`Genocs.Messaging` does not define its own configuration section.

Runtime configuration belongs to companion packages instead:

| Package | Typical section |
|---|---|
| `Genocs.Messaging.RabbitMQ` | `rabbitmq` |
| `Genocs.Messaging.Outbox` | `outbox` |
| transport-specific future providers | provider-owned sections |

If a user asks to change retries, topology, serialization, dead-letter behavior, or connection settings, point them to the installed provider package rather than this base package.

## Public Capability Map

### Core Contracts

- `IBusPublisher`
- `IBusSubscriber`
- `IMessageProperties`
- `IMessagePropertiesAccessor`
- `ICorrelationContextAccessor`

### Metadata Types

- `MessageAttribute`
- `MessageProperties`
- `MessagePropertiesAccessor`
- `CorrelationContextAccessor`

### CQRS Bridge Extensions

- `SendAsync<TCommand>(this IBusPublisher, TCommand, object)`
- `PublishAsync<TEvent>(this IBusPublisher, TEvent, object)`
- `SubscribeCommand<T>(this IBusSubscriber)`
- `SubscribeEvent<T>(this IBusSubscriber)`
- `AddServiceBusCommandDispatcher(this IGenocsBuilder)`
- `AddServiceBusEventDispatcher(this IGenocsBuilder)`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `Genocs.Messaging` is a usable runtime transport on its own.
2. Do not assume `AddServiceBusCommandDispatcher()` means Azure Service Bus is installed.
3. Do not assume `IBusSubscriber` starts consuming messages automatically.
4. Do not assume query dispatch over the bus exists in this package.
5. Do not assume `MessageAttribute` is required or honored by every provider.
6. Do not assume `IMessagePropertiesAccessor` or `ICorrelationContextAccessor` are registered unless the provider documentation says so.
7. Do not assume event subscriptions fan out to multiple handlers unless the provider or higher-level runtime documents that behavior.
8. Do not assume retry, dead-letter, or ordering guarantees from the base abstraction.
9. Do not assume message serialization format from the base package.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Messaging`, answer these questions:

1. Which concrete transport provider is installed?
2. Should the code publish directly through `IBusPublisher`, or through Core dispatchers bridged to the bus?
3. Are command handlers or event handlers already registered in DI?
4. Does the provider expose a startup call that activates subscriptions?
5. Are message identifiers, correlation identifiers, or custom headers required?
6. Does the provider honor `MessageAttribute`, or should routing stay provider-configured?
7. Is durable delivery or outbox behavior required from another package?

If any answer is unknown, generate abstraction-level code only and ask for the provider package name.

## Common Tasks And Safe Responses

### Task: "Publish an integration event"

Safe response:

- inject `IBusPublisher`
- call `PublishAsync(...)`
- include correlation identifiers or headers only if the caller has them
- ask which provider package is installed if actual transport behavior matters

### Task: "Route commands over the message bus"

Safe response:

- register `AddServiceBusCommandDispatcher()`
- ensure a provider package registers `IBusPublisher`
- explain that the Core command dispatcher now publishes commands instead of handling them in process

### Task: "Consume a message with an existing handler"

Safe response:

- use `SubscribeCommand<T>()` or `SubscribeEvent<T>()`
- confirm the matching handler interface is already registered in DI
- confirm the provider has a startup activation method

### Task: "Read broker metadata inside a handler"

Safe response:

- inject `IMessagePropertiesAccessor`
- mention that availability depends on the installed provider populating it

### Task: "Use message-specific queue or routing metadata"

Safe response:

- add `MessageAttribute` to the contract type
- note that actual provider support can vary

## Failure Modes And Troubleshooting

1. Publish calls compile but no message reaches infrastructure.
Fix: Install and configure a concrete transport provider such as `Genocs.Messaging.RabbitMQ`.

2. Command or event dispatch resolves but no broker activity happens.
Fix: Ensure a provider registered `IBusPublisher`, then verify `AddServiceBusCommandDispatcher()` or `AddServiceBusEventDispatcher()` was added.

3. Subscriptions are configured but handlers never run.
Fix: Confirm the provider-specific startup activation step is called, such as `UseRabbitMQ()` for the RabbitMQ provider.

4. Subscription handling fails because a handler cannot be resolved.
Fix: Register the matching `ICommandHandler<T>` or `IEventHandler<T>` in DI before activating subscriptions.

5. Message metadata is always null.
Fix: Use a provider that populates `IMessagePropertiesAccessor`; the base package only defines the contract.

6. Correlation context is always missing.
Fix: Confirm the installed provider registers and populates `ICorrelationContextAccessor` during inbound message handling.

7. A query cannot be routed over the bus.
Fix: This package does not provide a bus query bridge. Keep queries in-process or add a separate architectural layer.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.Outbox`
- `Genocs.Messaging.Outbox.MongoDB`
- `Genocs.Saga`
- `Genocs.Logging`
- `Genocs.Telemetry`

## One-Line Recommendation For Agents

If you only know that `Genocs.Messaging` is installed, generate broker-agnostic publish or subscribe code against `IBusPublisher` and `IBusSubscriber`, treat the CQRS extensions as thin bridges, and ask which provider package supplies the actual runtime transport.

## Quality Gate

Use [validate-messaging.mk](../validate-messaging.mk) as the package-level messaging quality baseline:

- `Genocs.Messaging`, `Genocs.Messaging.Outbox`, `Genocs.Messaging.AzureServiceBus`, and `Genocs.Messaging.RabbitMQ` must build for `net10.0` with `-warnaserror`.
- Messaging regression tests run through `Genocs.Messaging.UnitTests` and `Genocs.Messaging.RabbitMQ.UnitTests`.
- If a change introduces new warnings for the `net10.0` gate, treat it as a release blocker.

