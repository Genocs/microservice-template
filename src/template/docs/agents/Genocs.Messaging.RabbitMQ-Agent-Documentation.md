# Genocs.Messaging.RabbitMQ Agent Reference

## Consumer Mode for Agents

- Assume package is installed from NuGet.
- Do not rely on repository source code access.
- Prefer stable public APIs and extension methods documented here.
- If behavior is uncertain, fail safely and request config/package version details.

## Purpose

Genocs.Messaging.RabbitMQ implements the Genocs messaging abstractions over RabbitMQ. It wires `IBusPublisher` and `IBusSubscriber` to a real AMQP broker, providing exchange/queue convention management, configurable retry policies, dead-letter routing, TLS/SSL support, plugin hooks, and a background consumer hosted service — all driven by a single `rabbitmq` configuration section.

## Quick Facts

| Key | Value |
|---|---|
| Package | `Genocs.Messaging.RabbitMQ` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | RabbitMQ transport provider for Genocs messaging |
| Typical startup APIs | `AddRabbitMQAsync`, `UseRabbitMQ` |

## Install

```bash
dotnet add package Genocs.Messaging.RabbitMQ
```

## Minimal Integration Recipe (Program.cs)

```csharp
using Genocs.Core.Builders;
using Genocs.Messaging.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
IGenocsBuilder gnxBuilder = builder.AddGenocs();

await gnxBuilder.AddRabbitMQAsync();
gnxBuilder.Build();

var app = builder.Build();
app.UseRabbitMQ();
app.Run();
```

## Configuration

Use the `rabbitmq` section in `appsettings.json`.

```json
{
	"rabbitmq": {
		"connectionName": "orders-api",
		"hostNames": ["localhost"],
		"port": 5672,
		"virtualHost": "/",
		"username": "guest",
		"password": "guest",
		"requestedHeartbeat": "00:01:00",
		"requestedConnectionTimeout": "00:00:30",
		"socketReadTimeout": "00:00:30",
		"socketWriteTimeout": "00:00:30",
		"continuationTimeout": "00:00:20",
		"handshakeContinuationTimeout": "00:00:10",
		"networkRecoveryInterval": "00:00:05",
		"messageProcessingTimeout": "00:00:30",
		"requestedChannelMax": 0,
		"requestedFrameMax": 0,
		"conventionsCasing": "snake_case",
		"retries": 3,
		"retryInterval": 5,
		"messagesPersisted": true,
		"spanContextHeader": "span_context",
		"maxProducerChannels": 16,
		"requeueFailedMessages": false,
		"context": {
			"enabled": true,
			"header": "context"
		},
		"exchange": {
			"name": "genocs",
			"type": "topic",
			"declare": true,
			"durable": true,
			"autoDelete": false
		},
		"queue": {
			"template": "{service}/{message}",
			"declare": true,
			"durable": true,
			"exclusive": false,
			"autoDelete": false
		},
		"deadLetter": {
			"enabled": true,
			"prefix": "dlx.",
			"suffix": ".dead",
			"declare": true,
			"durable": true,
			"exclusive": false,
			"autoDelete": false,
			"ttl": 60000
		},
		"ssl": {
			"enabled": false,
			"serverName": null,
			"certificatePath": null,
			"caCertificatePath": null,
			"x509IgnoredStatuses": []
		},
		"qos": {
			"prefetchSize": 0,
			"prefetchCount": 10,
			"global": false
		},
		"conventions": {
			"messageAttribute": {
				"ignoreExchange": false,
				"ignoreRoutingKey": false,
				"ignoreQueue": false
			}
		},
		"logger": {
			"enabled": true,
			"logConnectionStatus": true,
			"logMessagePayload": false
		}
	}
}
```

| Setting | Type | Description |
|---|---|---|
| `connectionName` | `string` | Friendly connection name visible on the RabbitMQ server. |
| `hostNames` | `string[]` | One or more RabbitMQ broker hostnames. |
| `port` | `int` | AMQP port; `0` leaves the driver default. |
| `virtualHost` | `string` | Virtual host. Defaults to `/`. |
| `username` | `string` | Broker username. |
| `password` | `string` | Broker password. |
| `requestedHeartbeat` | `TimeSpan` | Requested heartbeat interval. |
| `requestedConnectionTimeout` | `TimeSpan` | Connection timeout. |
| `socketReadTimeout` | `TimeSpan` | Socket read timeout. |
| `socketWriteTimeout` | `TimeSpan` | Socket write timeout. |
| `continuationTimeout` | `TimeSpan` | Timeout for AMQP continuation operations. |
| `handshakeContinuationTimeout` | `TimeSpan` | Timeout for handshake continuation operations. |
| `networkRecoveryInterval` | `TimeSpan` | Delay between automatic recovery attempts. |
| `messageProcessingTimeout` | `TimeSpan?` | Optional max processing time per consumed message. |
| `requestedChannelMax` | `ushort` | Requested max channel count for the connection. |
| `requestedFrameMax` | `uint` | Requested max frame size. |
| `conventionsCasing` | `string` | Naming convention for generated exchange, queue, and routing names. |
| `retries` | `int` | Maximum message processing retry attempts. |
| `retryInterval` | `int` | Seconds between retries. |
| `messagesPersisted` | `bool` | Publishes messages using persistent delivery. |
| `spanContextHeader` | `string` | Header name used to propagate tracing span context. |
| `maxProducerChannels` | `int` | Max producer channels cached for concurrent publishing. |
| `requeueFailedMessages` | `bool` | Requeues failed messages instead of routing directly to dead-letter flow. |
| `context.enabled` | `bool` | Enables message-context propagation. |
| `context.header` | `string` | Header name used for serialized message context. |
| `exchange.name` | `string` | Default exchange name. |
| `exchange.type` | `string` | Exchange type such as `topic` or `direct`. |
| `exchange.declare` | `bool` | Declares the exchange automatically. |
| `exchange.durable` | `bool` | Makes the exchange durable. |
| `exchange.autoDelete` | `bool` | Auto-deletes the exchange when unused. |
| `queue.template` | `string` | Queue naming template. |
| `queue.declare` | `bool` | Declares queues automatically. |
| `queue.durable` | `bool` | Makes queues durable. |
| `queue.exclusive` | `bool` | Makes queues exclusive to the declaring connection. |
| `queue.autoDelete` | `bool` | Auto-deletes queues when unused. |
| `deadLetter.enabled` | `bool` | Enables dead-letter routing. |
| `deadLetter.prefix` | `string` | Prefix applied to dead-letter exchange names. |
| `deadLetter.suffix` | `string` | Suffix applied to dead-letter queue names. |
| `deadLetter.declare` | `bool` | Declares dead-letter infrastructure automatically. |
| `deadLetter.durable` | `bool` | Makes dead-letter entities durable. |
| `deadLetter.exclusive` | `bool` | Makes dead-letter queues exclusive. |
| `deadLetter.autoDelete` | `bool` | Auto-deletes dead-letter queues when unused. |
| `deadLetter.ttl` | `int?` | Optional TTL for dead-lettered messages. |
| `ssl.enabled` | `bool` | Enables TLS for broker connections. |
| `ssl.serverName` | `string` | TLS server name override. |
| `ssl.certificatePath` | `string` | Client certificate path. |
| `ssl.caCertificatePath` | `string` | CA certificate path. |
| `ssl.x509IgnoredStatuses` | `string[]` | Certificate validation statuses to ignore. |
| `qos.prefetchSize` | `uint` | QoS prefetch size. |
| `qos.prefetchCount` | `ushort` | QoS prefetch count. |
| `qos.global` | `bool` | Applies QoS settings globally on the channel. |
| `conventions.messageAttribute.ignoreExchange` | `bool` | Ignores message attribute exchange overrides. |
| `conventions.messageAttribute.ignoreRoutingKey` | `bool` | Ignores message attribute routing-key overrides. |
| `conventions.messageAttribute.ignoreQueue` | `bool` | Ignores message attribute queue overrides. |
| `logger.enabled` | `bool` | Enables transport-level RabbitMQ logging. |
| `logger.logConnectionStatus` | `bool` | Logs connection state changes. |
| `logger.logMessagePayload` | `bool` | Logs raw message payloads. |

## Decision Matrix For Agents

| Goal | Preferred API | Why |
|---|---|---|
| Register RabbitMQ transport | `await gnxBuilder.AddRabbitMQAsync()` | Wires `IBusPublisher`, `IBusSubscriber`, and all broker services |
| Start the background consumer | `app.UseRabbitMQ()` | Activates the hosted subscriber processing pipeline |
| Inject a custom serializer | `AddRabbitMQAsync(..., serializer: mySerializer)` | Replaces the default Newtonsoft.Json payload serializer |
| Register processing plugins | `AddRabbitMQAsync(..., plugins: r => r.Add<MyPlugin>())` | Adds pre/post-processing hooks via `IRabbitMqPlugin` |
| Tune retries and dead-letter | `rabbitmq` config options | Uses the supported configuration path without custom runtime code |

## Behavior Notes / Constraints

- `AddRabbitMQAsync` is asynchronous and must be awaited; startup throws `ArgumentException` if `hostNames` is null or empty.
- `UseRabbitMQ` must be called after `builder.Build()` to activate the subscriber background service.
- Conventions are derived from exchange and queue options in the `rabbitmq` section; names follow `conventionsCasing` if set.
- Retry behaviour and dead-letter routing are controlled by `retries`, `retryInterval`, and `deadLetter` options — not by handler code.
- Message context propagation (correlation IDs, span context) is controlled by the `context` and `spanContextHeader` options.

## Public Capability Map

| Capability | Surface |
|---|---|
| Register RabbitMQ transport and services | `AddRabbitMQAsync` on `IGenocsBuilder` |
| Activate subscriber background service | `UseRabbitMQ` on `IApplicationBuilder` |
| Publish messages to the broker | `IBusPublisher` (resolved from DI) |
| Subscribe to broker messages | `IBusSubscriber` (resolved from DI) |
| Access message delivery properties | `IMessagePropertiesAccessor` (resolved from DI) |
| Access correlation context | `ICorrelationContextAccessor` (resolved from DI) |
| Add a custom processing plugin | `IRabbitMqPlugin` implementation |
| Replace the payload serializer | `IRabbitMQSerializer` implementation |

## Dependencies

- `Genocs.Messaging`
- `RabbitMQ.Client`
- `Polly`
- `Newtonsoft.Json`

## Troubleshooting

1. Service fails to start with a broker connection error.
Fix: Verify `rabbitmq.hostNames`, `username`, `password`, and that the broker is reachable on the configured port.
2. Messages are published successfully but no handlers execute.
Fix: Confirm `UseRabbitMQ()` is called after `builder.Build()` and that subscriber registrations are in place before the app starts.
3. Messages are retried too many times or move to dead-letter unexpectedly.
Fix: Review `retries`, `retryInterval`, `requeueFailedMessages`, and `deadLetter.enabled` in the `rabbitmq` configuration section.

## Quality Gate

RabbitMQ package quality checks are part of [validate-messaging.mk](../validate-messaging.mk):

- `Genocs.Messaging.RabbitMQ` must build for `net10.0` with `-warnaserror`.
- Reliability regression coverage is enforced via `Genocs.Messaging.RabbitMQ.UnitTests`, including ack/nack settlement, retry, dead-letter, and scoped-handler execution paths.
