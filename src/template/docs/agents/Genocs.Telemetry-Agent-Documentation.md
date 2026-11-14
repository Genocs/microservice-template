# Genocs.Telemetry Agent Reference

## Agent Operating Mode

- Assume `Genocs.Telemetry` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods and configuration types as the only safe API surface.
- Generate OpenTelemetry registration, configuration, and observability guidance only.
- Do not invent Jaeger-specific registration methods or custom instrumentation hooks that this package does not expose.
- The package does expose a Prometheus plug-in (exporter, auth-gate middleware, scraping-endpoint mapping); use only the documented `UsePrometheus()` / `MapPrometheus()` extensions.
- There is no `UseTelemetry()` middleware. Aside from the optional Prometheus pipeline calls, `AddTelemetry()` is registration-only.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because `AddTelemetry()` extends `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Telemetry` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | OpenTelemetry integration for traces and metrics |
| Main value | One builder extension that wires OpenTelemetry resource metadata, ASP.NET Core and HttpClient instrumentation, runtime metrics, SQL client tracing, optional MongoDB tracing, and OTLP, Azure, console, or Prometheus exporters for traces and metrics |
| Requires | `Genocs.Core` and a configured `app.service` value |
| Optional pipeline calls | `UsePrometheus()` / `MapPrometheus()` only when `telemetry.prometheus.enabled = true` |

## What This Package Is For

Use `Genocs.Telemetry` when you need to:

- register OpenTelemetry from a Genocs host with `AddTelemetry()`
- export traces and metrics to OTLP collectors
- export traces and metrics to Azure Monitor
- expose metrics on a Prometheus scraping endpoint via the built-in plug-in
- emit telemetry to the console during development
- instrument incoming ASP.NET Core requests and outgoing `HttpClient` calls
- instrument runtime metrics such as GC and thread pool data
- instrument SQL client spans
- enrich spans with correlation IDs, route information, user IDs, and exception metadata

## What This Package Does Not Do By Itself

Do not assume `Genocs.Telemetry` can:

- expose a `UseTelemetry()` middleware or any other required app-pipeline call (the only optional pipeline calls are `UsePrometheus()` and `MapPrometheus()`, and only when the Prometheus plug-in is enabled)
- expose a Prometheus exporter or scraping endpoint when `telemetry.prometheus.enabled` is `false` (the plug-in is fully opt-in via configuration)
- configure a Jaeger exporter directly; Jaeger flows are supported through OTLP collector endpoints
- register custom `ActivitySource` names beyond the built-in sources and wildcard listener it already configures
- replace Genocs.Logging log exporter responsibilities
- replace `Genocs.Logging` for bootstrap logging, Seq, file sinks, or Serilog-based host logging
- configure MongoDB metrics or MongoDB logs through `telemetry.mongoDB`; this section is tracing-only

## Safe Default Mental Model

Treat `Genocs.Telemetry` as six things:

1. A single builder extension: `AddTelemetry()`
2. An OpenTelemetry resource and exporter setup layer (OTLP, Console, Azure Monitor, Prometheus)
3. A package that instruments web requests, outbound HTTP, runtime metrics, and SQL automatically
4. A span-enrichment layer that adds correlation, route, user, and exception tags
5. A Prometheus plug-in that surfaces OTel metrics on a scraping endpoint, opt-in via configuration and exposed via two pipeline extensions (`UsePrometheus()` and `MapPrometheus()`)
6. A package that leaves log export ownership to `Genocs.Logging`

If a user asks for local file logging, Seq logging, or Serilog middleware behavior, identify `Genocs.Logging` as the companion package rather than forcing those concerns into Telemetry.

## Fast Start Recipes

### Recipe 1: Minimal OTLP Setup

Use this when the service should export traces and metrics to an OTLP collector.

```csharp
using Genocs.Core.Builders;
using Genocs.Telemetry;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddTelemetry();

genocs.Build();

var app = builder.Build();
app.Run();
```

Configuration:

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "exporter": {
      "enabled": true,
      "otlpEndpoint": "http://localhost:4317",
      "protocol": "Grpc",
      "processorType": "Batch",
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

Effect:

- configures the OpenTelemetry resource with the service name from `app.service`
- enables ASP.NET Core request traces
- enables outgoing `HttpClient` traces and metrics
- enables runtime metrics
- enables SQL client traces
- exports each enabled signal to OTLP

### Recipe 2: Console Telemetry For Development

Use this when developers need immediate local trace or metric output.

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "console": {
      "enabled": true,
      "enableStructured": true,
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

Use this in development and diagnostics. Do not treat console export as the primary production observability path.

### Recipe 3: Azure Monitor Export

Use this when traces or metrics should go to Azure Monitor.

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "azure": {
      "enabled": true,
      "connectionString": "InstrumentationKey=...;IngestionEndpoint=https://...;LiveEndpoint=https://...;ApplicationId=...",
      "enableTracing": true,
      "enableMetrics": true
    }
  }
}
```

Important behavior:

- Azure export is added only when `azure.enabled` is `true`
- each signal is controlled independently through `enableTracing` and `enableMetrics`
- an empty Azure connection string disables Azure export even if the section is enabled

### Recipe 4: MongoDB Trace Instrumentation

Use this when MongoDB driver operations should appear as traces.

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "mongoDB": {
      "enabled": true,
      "enableTracing": true
    }
  }
}
```

Important behavior:

- MongoDB support in this package is tracing-only
- `telemetry.mongoDB` supports `enabled` and `enableTracing` only
- the repository sample appsettings files mostly use a separate root `mongodb` section for persistence packages, which does not configure `Genocs.Telemetry` by itself

### Recipe 5: Expose Metrics on a Prometheus Scraping Endpoint

Use this when an in-cluster Prometheus instance must scrape OpenTelemetry metrics from the service.

```csharp
using Genocs.Core.Builders;
using Genocs.Telemetry;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddTelemetry();

var app = builder.Build();
genocs.Build(app.Services);

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Auth-gate middleware (no-op when telemetry.prometheus.enabled is false).
app.UsePrometheus();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    // Scraping endpoint (no-op when telemetry.prometheus.enabled is false).
    endpoints.MapPrometheus();
});

app.Run();
```

Configuration:

```json
{
  "app": {
    "service": "orders-api"
  },
  "telemetry": {
    "enabled": true,
    "prometheus": {
      "enabled": true,
      "endpoint": "/metrics",
      "apiKey": null,
      "allowedHosts": []
    }
  }
}
```

Important behavior:

- `AddTelemetry()` adds the Prometheus exporter to the OpenTelemetry MeterProvider when `telemetry.prometheus.enabled = true`.
- The Prometheus plug-in shares the same metric pipeline with the other configured metric exporters (OTLP, Azure, Console). Multiple metric exporters per signal are allowed.
- `endpoint` is normalized to start with `/`. The default is `/metrics`.
- When `apiKey` is set, callers must include `?apiKey=<value>` on the scraping request; otherwise the response is `404`.
- When `allowedHosts` is non-empty, only callers whose `Host` header (or `x-forwarded-for`) matches a listed value are allowed; mismatches return `404`.
- When both `apiKey` and `allowedHosts` are empty, the auth-gate middleware is a transparent no-op and the endpoint is open.
- `UsePrometheus()` and `MapPrometheus()` are safe to call unconditionally; both are no-ops when the plug-in is disabled.

### Recipe 6: Keep SQL Text Scrubbed

Use this when SQL spans are useful but raw SQL text should not be exported.

```json
{
  "telemetry": {
    "enabled": true,
    "sqlClient": {
      "enableStatementText": false
    }
  }
}
```

Important behavior:

- SQL client instrumentation is added when `telemetry.sqlClient.enabled` is either omitted or set to `true`
- set `telemetry.sqlClient.enabled` to `false` to disable SQL client instrumentation registration
- when `enableStatementText` is `false`, a custom processor removes `db.query.text` and `db.statement` tags before export
- this reduces the risk of leaking sensitive SQL or parameterized business data into observability backends

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddTelemetry()` | Register OpenTelemetry traces and metrics from a Genocs host | Returns immediately if `app.service` is empty or `telemetry.enabled` is `false`; also wires the Prometheus exporter and middleware when `telemetry.prometheus.enabled` is `true` | Assuming registration always happens |
| `UsePrometheus()` | Add the Prometheus auth-gate middleware to the HTTP pipeline | Extension on `IApplicationBuilder`. No-op when `telemetry.prometheus.enabled` is `false` | Assuming a separate `AddPrometheus()` builder call is required (no longer exists) |
| `MapPrometheus()` | Map the Prometheus scraping endpoint | Extension on `IEndpointRouteBuilder`. No-op when `telemetry.prometheus.enabled` is `false`. Endpoint defaults to `/metrics` | Hard-coding a different scraping path or skipping the call when other endpoint mappings exist |
| `TelemetryOptions` | Configure the `telemetry` section | Owns exporter, console, Azure, Prometheus, MongoDB, and SQL client sub-options | Assuming every option property is actively used |
| `OtlpExportOptions` | Configure OTLP export | Controls endpoint, protocol, processor type, and per-signal enablement | Assuming OTLP export works without `otlpEndpoint` |
| `ConsoleOptions` | Configure console export | Enables console export per signal | Treating it as a production sink by default |
| `AzureOptions` | Configure Azure Monitor export | Enables Azure export per signal when the connection string exists | Assuming `enabled = true` is enough without a connection string |
| `PrometheusOptions` | Configure the Prometheus plug-in | Controls `enabled`, `endpoint`, `apiKey`, and `allowedHosts` for the scraping endpoint | Assuming a top-level `prometheus` section is read (it must live under `telemetry.prometheus`) |
| `MongoDbOptions` | Configure MongoDB telemetry behavior | Only `enabled` and `enableTracing` affect runtime behavior today | Assuming metrics or logging are implemented for MongoDB |
| `SqlClientOptions` | Configure SQL telemetry behavior | `enabled` controls SQL tracing registration; `enableStatementText` controls SQL text scrubbing | Assuming logging package settings control SQL tracing |

## Instrumentation Semantics

### Metrics

`AddTelemetry()` configures metrics for:

- ASP.NET Core
- .NET runtime
- `HttpClient`

The package does not add custom application meters on its own.

Prometheus export is provided as a built-in plug-in and is opt-in via `telemetry.prometheus.enabled`. When enabled, `AddTelemetry()` adds an `OpenTelemetry.Exporter.Prometheus.AspNetCore` exporter to the meter provider and registers the auth-gate middleware. Exposing the scraping endpoint still requires the host to call `app.UsePrometheus()` (auth gate) and `endpoints.MapPrometheus()` (endpoint mapping). Both calls are safe no-ops when the plug-in is disabled.

### Tracing

`AddTelemetry()` configures tracing for:

- ASP.NET Core
- `HttpClient`
- SQL client
- MongoDB when `telemetry.mongoDB.enabled` and `telemetry.mongoDB.enableTracing` are both `true`

It also listens to:

- `Genocs.Saga`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.AzureServiceBus`

And can add optional sources from configuration:

- `telemetry.activitySources` for explicit source names
- `telemetry.enableWildcardActivitySources` for `*` (opt-in only)

Default behavior is bounded to the explicit Genocs sources above to reduce unintended trace and cardinality growth.
Do not enable wildcard source capture unless the deployment requires broad source discovery.

### Logging

`Genocs.Telemetry` does not configure log exporters.

Practical consequence:

- use `Genocs.Logging` as the single owner for log export wiring
- keep `Genocs.Telemetry` focused on traces, metrics, instrumentation, and enrichment

## Span Enrichment Semantics

### Incoming HTTP Requests

The package enriches request spans with:

- `http.request_id` from `HttpContext.TraceIdentifier`
- `correlation.id` from one of these headers, in order:
  - `x-correlation-id`
  - `x-request-id`
  - `correlation-id`
- `enduser.id` from either:
  - `ClaimTypes.NameIdentifier`
  - `sub`
- `http.route` from route metadata when available
- if route metadata is missing, default fallback is `/_unmatched` to keep route cardinality bounded
- optional request-path fallback can be enabled with `telemetry.enableRoutePathFallback`
- when request-path fallback is enabled, `telemetry.normalizeRoutePathFallback` (default `true`) replaces identifier-like path segments (for example numeric IDs and GUIDs)

### Incoming HTTP Responses

The package also sets `http.route` again during response enrichment because route metadata may be unavailable at request-start time.

### Exceptions

The package enriches exception activity data with:

- `error.type`
- `error.message`
- `exception.source`
- `exception.hresult`
- `exception.target_site`
- inner-exception type and message when an inner exception exists

Guardrails for exception message payloads:

- control characters are sanitized before tag/status assignment
- `error.message`, `exception.inner.message`, and span status description are capped at 1024 characters
- oversized values are truncated with `...(truncated)`

## Exporter Semantics

### OTLP

OTLP export is added only when all of these are true:

- `telemetry.exporter.enabled = true`
- `telemetry.exporter.otlpEndpoint` is not empty
- the specific signal flag is enabled for that exporter

Supported configuration values in the package:

- `protocol`: `Grpc` or `HttpProtobuf`
- `processorType`: `Simple` or `Batch`

Batch settings are applied from:

- `maxQueueSize`
- `scheduledDelayMilliseconds`
- `exporterTimeoutMilliseconds`
- `maxExportBatchSize`

### Console

Console exporters are added independently per signal when:

- `telemetry.console.enabled = true`
- the specific signal flag is enabled

### Azure Monitor

Azure exporters are added independently per signal when:

- `telemetry.azure.enabled = true`
- the specific signal flag is enabled
- `telemetry.azure.connectionString` is not empty

### Prometheus

The Prometheus exporter is added to the OpenTelemetry MeterProvider when:

- `telemetry.prometheus.enabled = true`

Prometheus is metrics-only. When the plug-in is enabled, `AddTelemetry()`:

- adds `AddPrometheusExporter()` to the meter provider
- registers `PrometheusOptions` and the `PrometheusMiddleware` auth gate as DI services

Exposing the scraping endpoint requires both pipeline calls in the host:

- `app.UsePrometheus()` adds the auth-gate middleware
- `endpoints.MapPrometheus()` maps the scraping endpoint at `telemetry.prometheus.endpoint` (default `/metrics`)

Both calls are no-ops when `telemetry.prometheus.enabled` is `false`, so they are safe to leave in place across environments.

Auth-gate behavior:

- when neither `apiKey` nor `allowedHosts` is configured, the middleware is a transparent no-op
- when `apiKey` is set, callers must pass `?apiKey=<value>` on the scraping endpoint; otherwise the response is `404`
- when `allowedHosts` is non-empty, only callers whose `Host` (or `x-forwarded-for`) matches a listed value are allowed; mismatches return `404`

Prometheus does not affect tracing.

### Multiple Exporters

This package can add more than one exporter for the same signal at the same time.

Examples:

- OTLP traces plus console traces
- Azure traces plus console traces
- OTLP metrics plus Azure metrics
- OTLP metrics plus Prometheus scraping

That can be useful intentionally. Do not assume only one exporter may be active.

The real duplication risk is eliminated by keeping log export ownership only in `Genocs.Logging`.

Exporter decision policy:

- Prefer one exporter per signal in steady-state production.
- Allow OTLP + Azure dual export only for time-bound migration, validation, or controlled fallback.
- Record the expected extra cost and retention impact before enabling dual export.

Conflict guidance with `Genocs.Logging`:

- If `Genocs.Logging` exports logs to OTLP or Azure, treat `telemetry.exporter` and `telemetry.azure` as trace/metric pipelines only.
- Do not generate templates that imply telemetry package log ownership.
- Keep overlap notes explicit in runbooks so operators do not assume duplicate log paths are intended.

Profile examples for agent responses:

- OTLP-only traces/metrics: `telemetry.exporter.enabled=true`, `telemetry.azure.enabled=false`
- Azure-only traces/metrics: `telemetry.exporter.enabled=false`, `telemetry.azure.enabled=true`
- Temporary OTLP+Azure traces/metrics: both enabled with a time-boxed migration note
- Minimal-overhead profile: disable trace exporters (`telemetry.exporter.enableTracing=false`, `telemetry.console.enableTracing=false`, `telemetry.azure.enableTracing=false`) to keep registration deterministic while skipping custom tracing enrichment and exception recording

## Configuration Ownership

`Genocs.Telemetry` owns the `telemetry` section.

```json
{
  "telemetry": {
    "enabled": true,
    "sqlClient": {
      "enabled": true,
      "enableStatementText": false
    },
    "mongoDB": {
      "enabled": true,
      "enableTracing": true
    },
    "exporter": {
      "enabled": true,
      "otlpEndpoint": "http://localhost:4317",
      "protocol": "Grpc",
      "processorType": "Batch",
      "enableTracing": true,
      "enableMetrics": true,
      "maxQueueSize": 2048,
      "scheduledDelayMilliseconds": 5000,
      "exporterTimeoutMilliseconds": 30000,
      "maxExportBatchSize": 512
    },
    "console": {
      "enabled": true,
      "enableStructured": true,
      "enableTracing": true,
      "enableMetrics": true
    },
    "azure": {
      "enabled": false,
      "connectionString": "InstrumentationKey=...;IngestionEndpoint=https://...",
      "enableTracing": false,
      "enableMetrics": false
    },
    "prometheus": {
      "enabled": false,
      "endpoint": "/metrics",
      "apiKey": null,
      "allowedHosts": []
    }
  }
}
```

What this package actively uses:

- `telemetry.enabled`
- `telemetry.exporter.*`
- `telemetry.console.*`
- `telemetry.azure.*`
- `telemetry.prometheus.enabled`
- `telemetry.prometheus.endpoint`
- `telemetry.prometheus.apiKey`
- `telemetry.prometheus.allowedHosts`
- `telemetry.mongoDB.enabled`
- `telemetry.mongoDB.enableTracing`
- `telemetry.sqlClient.enabled`
- `telemetry.sqlClient.enableStatementText`
- `telemetry.enableWildcardActivitySources`
- `telemetry.activitySources`
- `telemetry.enableRoutePathFallback`
- `telemetry.normalizeRoutePathFallback`

Note: a top-level `prometheus` section is no longer read. The Prometheus plug-in lives strictly under `telemetry.prometheus`. The previously separate `Genocs.Metrics` package and its `AddPrometheus()` builder method have been removed.

## Relationship With Genocs.Logging

`Genocs.Telemetry` and `Genocs.Logging` are complementary, not interchangeable.

### What Genocs.Logging Owns

`Genocs.Logging` owns:

- Serilog host bootstrap through `builder.Host.UseLogging()`
- bootstrap logging before the host is built
- file, Seq, ELK, Loki, console, and Serilog-based OTLP or Azure sinks
- correlation logging middleware and CQRS logging decorators

### What Genocs.Telemetry Owns

`Genocs.Telemetry` owns:

- OpenTelemetry traces
- OpenTelemetry metrics
- instrumentation and span enrichment

### Safe Integration Patterns

#### Pattern 1: Logging For Logs, Telemetry For Traces And Metrics

Use this when you want:

- Serilog console, file, or Seq output from `Genocs.Logging`
- traces and metrics from `Genocs.Telemetry`

Safe guidance:

- keep `builder.Host.UseLogging()`
- keep `AddTelemetry()`
- configure OTLP/Azure/Seq/Loki/file/console log outputs only through `Genocs.Logging`

### Correlation Relationship

The packages complement each other here:

- `Genocs.Logging` can enrich logs with correlation and activity context
- `Genocs.Telemetry` extracts incoming correlation headers and writes them as span tags

This means logs and traces can still align well when both packages are configured correctly.

### What Not To Assume

Do not assume:

- `Genocs.Telemetry` replaces bootstrap logging from `StaticLogger.EnsureInitialized()`
- `Genocs.Logging` emits traces or metrics by itself
- both packages should export the same logs to the same OTLP or Azure backend by default

## Public Capability Map

### Registration

- `AddTelemetry()`

### Pipeline Extensions (Prometheus plug-in only)

- `UsePrometheus()` (extension on `IApplicationBuilder`)
- `MapPrometheus()` (extension on `IEndpointRouteBuilder`)

### Configuration Types

- `TelemetryOptions`
- `OtlpExportOptions`
- `ConsoleOptions`
- `AzureOptions`
- `PrometheusOptions`
- `MongoDbOptions`
- `SqlClientOptions`

### Built-In Instrumentation And Enrichment

- ASP.NET Core tracing and metrics
- `HttpClient` tracing and metrics
- runtime metrics
- SQL client tracing
- optional MongoDB tracing
- optional Prometheus metric scraping endpoint
- request, response, exception, route, correlation, and user-id span enrichment

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `AddTelemetry()` always registers anything. It no-ops when `app.service` is empty or `telemetry.enabled` is `false`.
2. Do not assume a `UseTelemetry()` call exists. The only optional pipeline calls are `UsePrometheus()` and `MapPrometheus()`, and only when the Prometheus plug-in is enabled.
3. Do not assume Jaeger export has a dedicated exporter toggle in this package; use OTLP collector endpoints for Jaeger ingestion.
4. Do not assume SQL tracing is always on. `telemetry.sqlClient.enabled` can disable SQL client instrumentation.
5. Do not assume `telemetry.mongoDB` supports metrics or logging flags; MongoDB configuration is tracing-only.
6. Do not assume `Genocs.Telemetry` exports logs. Log export belongs to `Genocs.Logging`.
7. Do not assume the root `mongodb` or `mongoDb` section configures Telemetry. MongoDB tracing lives under `telemetry.mongoDB`.
8. Do not assume enabling `enableStatementText` is harmless. It can expose SQL text and sensitive data.
9. Do not assume only one telemetry exporter can be enabled. The package allows multiple exporters per signal, including Prometheus alongside OTLP/Azure/Console for metrics.
10. Do not split log export ownership between packages; keep it in `Genocs.Logging`.
11. Do not assume a top-level `prometheus` section is read; the plug-in configuration must be nested under `telemetry.prometheus`.
12. Do not invent an `AddPrometheus()` builder call. The previous `Genocs.Metrics` package has been removed; Prometheus is now configuration-driven inside `AddTelemetry()`.
13. Do not assume `MapPrometheus()` is automatic. The host must call `endpoints.MapPrometheus()` (and `app.UsePrometheus()` for auth gating) to expose the scraping endpoint.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Telemetry`, answer these questions:

1. Is `app.service` configured with a real service name?
2. Is `telemetry.enabled` actually intended to turn the feature on in this environment?
3. Is log export ownership explicitly assigned to `Genocs.Logging` for this deployment?
4. Is SQL statement text safe to emit in this environment?
5. Is MongoDB tracing actually needed, and is it configured under `telemetry.mongoDB` rather than only under persistence settings?
6. Should exporters be OTLP, Azure, console, Prometheus, or a deliberate combination?
7. If Prometheus is enabled, are `app.UsePrometheus()` and `endpoints.MapPrometheus()` both wired in the HTTP pipeline, and is the scraping endpoint protected by `apiKey` or `allowedHosts` when reachable from outside the cluster?

If any answer is unknown, prefer trace and metric export first, keep SQL text scrubbing enabled, and keep log export in `Genocs.Logging`.

## Common Tasks And Safe Responses

### Task: "Enable distributed tracing"

Safe response:

- call `AddTelemetry()` during host registration
- set `app.service`
- enable either OTLP or Azure tracing export under `telemetry`

### Task: "See telemetry locally while developing"

Safe response:

- enable `telemetry.console`
- turn on only the signal types needed for the debugging session

### Task: "Export telemetry to Azure Monitor"

Safe response:

- enable `telemetry.azure`
- provide a valid connection string
- enable only the needed signals

### Task: "Expose metrics to Prometheus"

Safe response:

- set `telemetry.prometheus.enabled = true` and choose an endpoint (default `/metrics`)
- in the HTTP pipeline, call `app.UsePrometheus()` (auth gate) and `endpoints.MapPrometheus()` (endpoint mapping)
- if the endpoint is reachable from outside a private network, set `apiKey` or `allowedHosts` (or both)
- expect Prometheus to coexist with other metric exporters (OTLP, Azure, Console) without conflict

### Task: "Instrument MongoDB"

Safe response:

- configure `telemetry.mongoDB.enabled = true`
- configure `telemetry.mongoDB.enableTracing = true`
- do not rely on the separate persistence `mongodb` section for Telemetry behavior

### Task: "Keep SQL safe"

Safe response:

- leave `telemetry.sqlClient.enableStatementText = false`
- mention explicitly that SQL client tracing still runs without raw SQL text

### Task: "Use Logging and Telemetry together"

Safe response:

- keep `Genocs.Logging` for bootstrap and local sinks
- keep `Genocs.Telemetry` for traces and metrics
- disable overlapping OTLP or Azure log export if duplicate ingestion is not wanted

## Failure Modes And Troubleshooting

1. No telemetry appears anywhere.
Fix: Confirm `app.service` is set and `telemetry.enabled` is `true`. `AddTelemetry()` silently returns otherwise.

2. Traces and metrics work, but OpenTelemetry logs never appear.
Fix: This is expected. `Genocs.Telemetry` no longer exports logs. Configure log exporters in `Genocs.Logging`.

3. MongoDB operations are not traced even though Mongo persistence is enabled.
Fix: Add `telemetry.mongoDB.enabled = true` and `telemetry.mongoDB.enableTracing = true`. The root persistence `mongodb` section is separate.

4. SQL spans appear without raw SQL text.
Fix: This is expected when `telemetry.sqlClient.enableStatementText = false`. Enable it only after a data-exposure review.

5. `telemetry.sqlClient.enabled = false` stops SQL tracing as expected.
Fix: Ensure this flag is set under `telemetry.sqlClient` and that the service was restarted with updated configuration.

6. Azure export is configured but nothing is sent.
Fix: Confirm `telemetry.azure.enabled = true`, the per-signal flags are enabled, and the Azure connection string is non-empty.

7. OTLP export is configured but no data reaches the collector.
Fix: Confirm `telemetry.exporter.enabled = true`, `otlpEndpoint` is correct, and the collector accepts the configured protocol.

8. Log volume looks doubled in OTLP or Azure.
Fix: Ensure only `Genocs.Logging` is exporting logs and remove duplicate sink targets in logger configuration.

9. The Prometheus scraping endpoint returns `404`.
Fix: Confirm `telemetry.prometheus.enabled = true`, the host calls both `app.UsePrometheus()` and `endpoints.MapPrometheus()`, the request path matches `telemetry.prometheus.endpoint`, and that any configured `apiKey` or `allowedHosts` constraints are satisfied by the caller.

10. Metrics appear in OTLP but not on Prometheus (or vice versa).
Fix: Each metric exporter is registered independently. Confirm both `telemetry.exporter` (or `telemetry.azure`) and `telemetry.prometheus` are enabled when dual export is intended; they share the same MeterProvider but each has its own enable flag.

11. Code references `using Genocs.Metrics.Prometheus;` or `.AddPrometheus()` and no longer compiles.
Fix: The standalone `Genocs.Metrics` package has been removed. Replace the using directive with `using Genocs.Telemetry;`, remove the `.AddPrometheus()` call from the builder chain, and move the JSON `prometheus` section under `telemetry.prometheus`.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Logging`
- `Genocs.WebApi`
- `Genocs.Messaging`
- `Genocs.Messaging.RabbitMQ`
- `Genocs.Messaging.AzureServiceBus`
- `Genocs.Saga`

## One-Line Recommendation For Agents

If you only know that `Genocs.Telemetry` is installed, generate `AddTelemetry()` with a valid `app.service`, treat the package as registration-only OpenTelemetry infrastructure for traces and metrics (plus an opt-in Prometheus plug-in exposed through `UsePrometheus()` and `MapPrometheus()`), keep SQL text scrubbing enabled by default, and keep log export ownership in `Genocs.Logging`.