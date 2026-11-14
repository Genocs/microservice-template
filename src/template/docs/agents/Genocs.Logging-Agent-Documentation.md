# Genocs.Logging Agent Reference

## Agent Operating Mode

- Assume `Genocs.Logging` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat the package as a Serilog-based host logging integration package, not a generic logging abstraction.
- Generate logging bootstrap, configuration, middleware registration, and CQRS decorator wiring only when those capabilities are explicitly needed.
- Do not invent custom sinks, tracing emitters, or security filtering behavior that this package does not provide.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Logging` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Serilog-based logging bootstrap and sink orchestration for Genocs hosts |
| Main value | Host logging setup, sink configuration, correlation context middleware, runtime log-level switching, CQRS command and event handler logging decorators |
| Requires | ASP.NET Core host composition and `Genocs.Core` |

## What This Package Is For

Use `Genocs.Logging` when you need to:

- configure Serilog through `builder.Host.UseLogging()`
- emit structured logs to console, file, OTLP, Elasticsearch, Seq, Loki, or Application Insights
- expose a runtime log-level change endpoint through `MapLogLevelHandler()`
- add correlation-context logging middleware to the HTTP pipeline
- add command and event handler logging decorators to a Genocs CQRS setup
- initialize an early bootstrap logger before the host is built

## What This Package Does Not Do By Itself

Do not assume `Genocs.Logging` can:

- provide a generic logging abstraction independent of Serilog
- create traces or metrics by itself
- decorate query handlers
- register CQRS handlers for you
- sanitize secrets or redact payloads automatically
- replace the need for `Genocs.Telemetry` when distributed tracing is required

## Safe Default Mental Model

Treat `Genocs.Logging` as four things:

1. A Serilog host bootstrap layer
2. A set of configuration-bound sink options under `logger`
3. Optional HTTP middleware for correlation baggage and payload capture
4. Optional CQRS logging decorators for commands and events

If a user asks for observability beyond logs, identify whether `Genocs.Telemetry` or another package is also installed.

## Fast Start Recipes

### Recipe 1: Minimal Host Logging

Use this when the application only needs structured host logging.

```csharp
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();

var app = builder.Build();
app.Run();
```

Effect:
- creates a bootstrap logger before the host is fully configured
- configures Serilog through the `logger` and `app` sections
- registers `ILoggingService` for runtime level changes

### Recipe 2: Runtime Log-Level Switching

Use this when operators need to change the global log level without redeploying.

```csharp
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseLogging();

var app = builder.Build();
app.MapLogLevelHandler("/logging/level");
app.Run();
```

Use a `POST` request such as `/logging/level?level=Debug`.

### Recipe 3: Correlation Context Logging

Use this when request correlation baggage and optional HTTP payload capture should appear in log scope.

```csharp
using Genocs.Core.Builders;
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);
IGenocsBuilder genocs = builder.AddGenocs()
    .AddCorrelationContextLogging();

builder.Host.UseLogging();
genocs.Build();

var app = builder.Build();
app.UseCorrelationContextLogging();
app.Run();
```

This requires both registration and middleware usage.

`StaticLogger.EnsureInitialized()` installs a bootstrap logger. Its level overrides last only until `builder.Host.UseLogging()` reads the `logger` section. Put `Microsoft`, `Microsoft.EntityFrameworkCore`, and `Hangfire` in `logger.minimumLevelOverrides`. `Genocs.Logging` exposes one `UseLogging` extension for the host.

### Recipe 4: CQRS Handler Logging

Use this when Genocs command and event handlers should emit structured before or after logs through decorators.

```csharp
using Genocs.Core.Builders;
using Genocs.Logging.CQRS;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs()
    .AddCommandHandlersLogging()
    .AddEventHandlersLogging();

genocs.Build();
```

If you want actual message templates, register an `IMessageToLogTemplateMapper` implementation.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `StaticLogger.EnsureInitialized()` | Capture early startup failures | Creates a bootstrap Serilog logger before host build | Skipping it and losing startup diagnostics |
| `builder.Host.UseLogging()` | Configure Serilog for the host | Reads `logger` and `app` sections and registers `ILoggingService` | Calling it after `builder.Build()` |
| `MapLogLevelHandler()` | Expose runtime level-switch endpoint | Maps a `POST` endpoint that changes the shared `LoggingLevelSwitch` | Sending `GET` requests or forgetting `UseLogging()` |
| `AddCorrelationContextLogging()` | Register correlation middleware and options | Adds `CorrelationContextLoggingMiddleware` and stores `LoggerOptions` | Forgetting to also call `UseCorrelationContextLogging()` |
| `UseCorrelationContextLogging()` | Put correlation middleware into the pipeline | Captures Activity baggage and request payload in scope; response payload is captured after pipeline completion and added as an Activity tag | Using it without registering it first |
| `AddCommandHandlersLogging()` | Decorate command handlers with logging | Decorates handlers found in the target assembly | Assuming it registers handlers themselves |
| `AddEventHandlersLogging()` | Decorate event handlers with logging | Decorates handlers found in the target assembly | Assuming query handlers are covered too |
| `IMessageToLogTemplateMapper` | Supply log templates for CQRS decorator logs | If absent, decorators silently emit no template-based logs | Assuming logs will appear without a mapper |

## Choosing The Right Surface

| Situation | Prefer |
|---|---|
| You only need host logging | `StaticLogger.EnsureInitialized()` plus `builder.Host.UseLogging()` |
| You need to change log verbosity at runtime | `MapLogLevelHandler()` |
| You need request-scope baggage or optional payload capture | `AddCorrelationContextLogging()` plus `UseCorrelationContextLogging()` |
| You need command handler logging | `AddCommandHandlersLogging()` |
| You need event handler logging | `AddEventHandlersLogging()` |
| You need query handler logging | Another solution; this package does not provide it |
| You need traces or metrics | Ask for `Genocs.Telemetry` |

## Public Capability Map

### Host Logging Bootstrap

- `StaticLogger.EnsureInitialized()`
- `UseLogging(Action<HostBuilderContext, LoggerConfiguration>? configure = null, string loggerSectionName = "logger", string appSectionName = "app")`
- `ILoggingService`
- `MapLogLevelHandler()`

### Correlation And Request Logging

- `AddCorrelationContextLogging()`
- `UseCorrelationContextLogging()`
- `CorrelationContextLoggingMiddleware`
- `HttpPayloadOptions`

### CQRS Logging

- `AddCommandHandlersLogging()`
- `AddEventHandlersLogging()`
- `IMessageToLogTemplateMapper`
- `HandlerLogTemplate`

### Configuration Types

- `LoggerOptions`
- `ConsoleOptions`
- `LocalFileOptions`
- `ElkOptions`
- `SeqOptions`
- `LokiOptions`
- `AzureOptions`
- `HttpPayloadOptions`

## Configuration Ownership

`Genocs.Logging` owns the `logger` configuration section.

```json
{
  "logger": {
    "level": "Information",
    "otlpEndpoint": "http://localhost:4317",
    "minimumLevelOverrides": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Hangfire": "Warning",
      "System": "Warning"
    },
    "excludePaths": ["/healthz", "/alive"],
    "excludeProperties": ["RequestBody"],
    "tags": {
      "service": "orders-api"
    },
    "console": {
      "enabled": true,
      "enableStructured": false,
      "enableTracing": false,
      "enableMetrics": false
    },
    "file": {
      "enabled": false,
      "path": "logs/app.log",
      "interval": "Day"
    },
    "seq": {
      "enabled": false,
      "url": "http://localhost:5341",
      "apiKey": ""
    },
    "httpPayload": {
      "enabled": false,
      "captureRequestBody": false,
      "captureResponseBody": true,
      "maxBodyLength": 4096,
      "allowedContentTypes": ["application/json"]
    }
  }
}
```

What this package actively uses:

- `level`
- `otlpEndpoint`
- `minimumLevelOverrides`
- `excludePaths`
- `excludeProperties`
- `tags`
- `console.enabled`
- `console.enableStructured`
- `file.enabled`, `file.path`, `file.interval`
- `elk.*`
- `seq.*`
- `loki.*`
- `azure.*`
- `httpPayload.*`

The package also reads the shared `app` section from `Genocs.Common.AppOptions` for enrichment fields such as application, instance, and version.

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Call `builder.Host.UseLogging()` before the host is built.
2. Do not assume `UseLogging()` is additive with another Serilog bootstrap strategy.
3. Do not enable payload capture by default.
4. Do not promise query-handler logging; only command and event decorators exist.
5. Do not assume `IMessageToLogTemplateMapper` is registered automatically.
6. Do not assume tracing or metrics are emitted just because console flags mention them.
7. Do not assume runtime level switching affects every deployed instance in a distributed environment.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Logging`, answer these questions:

1. Is the host using ASP.NET Core with `builder.Host` available?
2. Is Serilog already configured somewhere else?
3. Are runtime level changes required, or is static config enough?
4. Is correlation baggage or HTTP payload capture actually needed?
5. Are CQRS command or event handlers already registered in the host?
6. Is there a custom `IMessageToLogTemplateMapper`, or should decorator logging stay template-free?
7. Is distributed tracing required, meaning `Genocs.Telemetry` should also be considered?
8. Which sinks are allowed in the target environment?

If any of these answers are unknown, prefer the minimal host-logging setup.

## Common Tasks And Safe Responses

### Task: "Set up logging"

Safe response:
- call `StaticLogger.EnsureInitialized()`
- call `builder.Host.UseLogging()`
- configure only the required sinks under `logger`

### Task: "Capture correlation IDs and request bodies"

Safe response:
- add and use correlation middleware explicitly
- enable payload capture only for approved content types and debugging scenarios

### Task: "Log CQRS handler activity"

Safe response:
- decorate command and event handlers only
- ask whether a custom `IMessageToLogTemplateMapper` should be registered

### Task: "Support runtime log-level changes"

Safe response:
- map `MapLogLevelHandler()`
- mention that it expects `POST` and changes a shared in-process level switch

### Task: "Add tracing"

Safe response:
- note that this package enriches logs from `Activity.Current`
- ask for `Genocs.Telemetry` if actual tracing export is required

## Failure Modes And Troubleshooting

1. No structured logs appear.
Fix: Ensure `builder.Host.UseLogging()` is called before the host is built and that at least one sink is enabled.

2. Startup exceptions are not logged.
Fix: Call `StaticLogger.EnsureInitialized()` at the top of `Program.cs`.

3. Runtime log-level endpoint returns 400.
Fix: Ensure `UseLogging()` registered `ILoggingService`, send a `POST` request, and include a valid `level` query value.

4. Correlation middleware appears inactive.
Fix: Register it with `AddCorrelationContextLogging()` and add it to the pipeline with `UseCorrelationContextLogging()`.

5. CQRS decorator logs never appear.
Fix: Confirm handlers are already registered, confirm the correct assembly is decorated, and register an `IMessageToLogTemplateMapper` if template-based logs are expected.

6. Request or response bodies are missing from logs.
Fix: Check `httpPayload.enabled`, capture flags, and strict media-type allowlist matching. Request body is available in request scope, while response body is captured after pipeline completion as `http.response.body` activity tag.

7. Payload logs are too large or expose sensitive data.
Fix: Disable payload capture, reduce `maxBodyLength`, tighten `allowedContentTypes`, or exclude sensitive properties at the sink level. Values less than or equal to `0` are normalized and very large values are capped to bounded limits.

8. Correlation baggage is missing or appears truncated.
Fix: Correlation baggage enrichment is intentionally bounded. Only the first 32 entries are added to scope, keys are capped at 64 characters, values are capped at 256 characters, and duplicate normalized keys are ignored.

## Migration Notes (Runtime Behavior Changes)

These notes apply when upgrading from older `Genocs.Logging` behavior to the hardened behavior set delivered across `LOGGING-005` to `LOGGING-013`.

### Required review before rollout

1. Validate all enabled sink endpoints (`seq.url`, `loki.url`) are present and valid URIs.
2. Confirm `logger.enabled` behavior is aligned with host intent and environment overrides.
3. Verify runtime level-switch clients use `POST` and parse the documented response contract.
4. Re-check `excludePaths` and `excludeProperties` values against exact matching semantics.
5. Re-evaluate payload capture settings (`httpPayload.enabled`, capture flags, allowlist, `maxBodyLength`) for production safety.
6. Audit Activity baggage producers for bounded key/value sizes and entry counts.

### Behavior deltas and operator impact

1. Enabled but invalid sink settings are skipped with diagnostics instead of risking startup failure.
2. Top-level logging enablement semantics are now explicit and should be treated as authoritative host configuration.
3. Runtime log-level endpoint behavior is formalized, including explicit invalid-input handling.
4. Exclusion filters use one documented matching model and may no longer match previously ambiguous partial values.
5. Request and response payload capture is explicitly bounded; invalid or extreme body length settings are normalized.
6. Response payload enrichment remains post-pipeline and is represented as Activity tag data, not in-request scope state.
7. Correlation baggage enrichment is bounded (entry count, key length, value length, duplicate-key handling).

### Recommended validation after upgrade

1. Run host startup with each target environment configuration and confirm all expected sinks initialize.
2. Execute integration checks for `MapLogLevelHandler()` success and invalid-level cases.
3. Exercise representative API calls and verify request/response payload capture behavior and truncation boundaries.
4. Verify exclusion filters suppress only intended paths/properties.
5. Confirm downstream log consumers parse current payload and endpoint response shapes.

## Release-Note Checklist For Logging Behavior Changes

When releasing any `Genocs.Logging` change that can alter runtime observability behavior, include all of the following:

1. A concise Behavior Change summary in `CHANGELOG.md`.
2. A Migration Impact section listing who is affected (hosts, operators, dashboards, automation).
3. Configuration delta details (keys added/removed/changed, defaults, normalization rules).
4. API contract delta details for endpoints such as `MapLogLevelHandler()`.
5. Backward-compatibility statement and any required rollout actions.
6. Validation commands and expected results for maintainers.
7. Links to the relevant tests that cover new or changed behavior.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Common`
- `Genocs.Telemetry`
- `Genocs.WebApi`
- `Genocs.Messaging`
- `Genocs.Auth`

## One-Line Recommendation For Agents

If you only know that `Genocs.Logging` is installed, generate minimal Serilog host bootstrap first, then add optional correlation middleware and CQRS decorators only when the host already uses Genocs.Core and those behaviors are explicitly required.