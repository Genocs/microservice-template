# Genocs.Http Agent Reference

## Agent Operating Mode

- Assume `Genocs.Http` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, interfaces, and option types as the only safe API surface.
- Generate outbound HTTP client registration and consumption code only. Do not invent service discovery, RestEase, or resilience features that are not implemented in this package.
- If package composition is unclear, ask whether `Genocs.Core` is already installed because `AddHttpClient(...)` extends `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Http` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Outbound HTTP client abstraction and registration layer for Genocs applications |
| Main value | Typed `IHttpClient` registration, retry-enabled request execution, pluggable serialization, optional request-scoped correlation header propagation, optional request-URL masking in logs |
| Requires | `Genocs.Core` for `IGenocsBuilder` integration |

## What This Package Is For

Use `Genocs.Http` when you need to:

- register a typed outbound HTTP client through `IGenocsBuilder`
- send GET, POST, PUT, PATCH, and DELETE requests through a single abstraction
- choose between exception-oriented and result-oriented response handling
- customize JSON serialization and deserialization behavior
- propagate request-scoped correlation headers on outbound calls
- mask sensitive URL fragments in HTTP client logs
- combine **relative** request paths with a configured **`HttpClient.BaseAddress`**, or pass **absolute** URIs, using normal `System.Uri` rules (no automatic `http://` insertion)

## What This Package Does Not Do By Itself

Do not assume `Genocs.Http` can:

- provide service discovery or load balancing behavior from `httpClient.type` or `httpClient.services`
- register named downstream clients per service automatically
- interpret a `restEase` section
- apply circuit breakers, timeouts, or fallback policies beyond retry logic
- preserve non-success responses in the plain `GetAsync<T>` or `PostAsync<T>` methods (use `*ResultAsync<T>` with a string URI for that behavior)
- add correlation values unless factories and header names are available
- log or mask request bodies

## Safe Default Mental Model

Treat `Genocs.Http` as four things:

1. A builder extension that registers one typed `IHttpClient` backed by `HttpClientFactory`
2. A retrying wrapper around `HttpClient` for common HTTP verbs
3. A serializer abstraction with a default `System.Text.Json` implementation
4. Optional delegating handlers for request-scoped correlation propagation and URL masking

If a user asks for discovery, advanced resiliency, or generated API clients, ask which companion package or external library should provide that behavior.

## Fast Start Recipes

### Recipe 1: Minimal Registration

Use this when the application needs a single outbound HTTP abstraction.

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient();

genocs.Build();

var app = builder.Build();
app.Run();
```

Effect:
- binds `HttpClientOptions` from the `httpClient` section
- registers `IHttpClient` with the typed implementation `GenocsHttpClient`
- registers the default `SystemTextJsonHttpClientSerializer`
- adds empty correlation factories if no custom factories are already registered

### Recipe 2: Call A JSON API And Preserve Status Codes

Use this when the caller needs both the deserialized payload and the raw `HttpResponseMessage`.

Relative path segments require a configured **`HttpClient.BaseAddress`** for the Genocs client (set when registering the typed client):

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient(httpClientBuilder: b =>
        b.ConfigureHttpClient(c => c.BaseAddress = new Uri("https://catalog.internal/api/v1/")));

genocs.Build();
```

Complete host registration as in Recipe 1 (`WebApplication.CreateBuilder`, `builder.Build()`, etc.) where applicable.

Then call sites can pass relative URIs:

```csharp
using Genocs.Http;

public sealed class CatalogGateway(IHttpClient httpClient)
{
    public async Task<HttpResult<ProductDto>> GetProductAsync(Guid id, CancellationToken cancellationToken)
        => await httpClient.GetResultAsync<ProductDto>($"catalog/products/{id}", cancellationToken: cancellationToken);
}

public sealed record ProductDto(Guid Id, string Name);
```

Alternatively, pass a **full absolute** URI string to `GetResultAsync` (or other helpers) and omit `BaseAddress` for that call.

Use `*ResultAsync` methods when non-success responses should not be turned into exceptions by the public verb helpers.

**Migration note:** Older versions prepended `http://` to strings that did not start with `http`. That behavior is removed; use absolute URIs or `BaseAddress` + relative paths instead.

### Recipe 3: Customize Serialization

Use this when the downstream API requires non-default JSON settings.

```csharp
using System.Text.Json;
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpClientSerializer>(
    new SystemTextJsonHttpClientSerializer(
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        }));

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient();

genocs.Build();
```

This works because `GenocsHttpClient` accepts an `IHttpClientSerializer` dependency, but note that `AddHttpClient()` also registers a default serializer as a singleton.

### Recipe 4: Add Correlation Headers And URL Masking

Use this when outbound calls must include correlation metadata and logs must hide sensitive URL fragments.

```csharp
using Genocs.Core.Builders;
using Genocs.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICorrelationIdFactory, MyCorrelationIdFactory>();
builder.Services.AddSingleton<ICorrelationContextFactory, MyCorrelationContextFactory>();

IGenocsBuilder genocs = builder.AddGenocs()
    .AddHttpClient(maskedRequestUrlParts: ["token", "password"]);

genocs.Build();
```

This only masks URL fragments in the HTTP client logging pipeline. It does not redact request bodies or arbitrary log properties.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddHttpClient(...)` | Register Genocs outbound HTTP support | Registers one typed `IHttpClient` and binds `HttpClientOptions` from configuration | Assuming it registers multiple downstream clients automatically |
| `RemoveHttpClient()` | Remove the typed `IHttpClient` mapping from the internal registry | Workaround for a typed-client mapping issue | Treating it as normal lifecycle management |
| `IHttpClient` | Send outbound HTTP requests | String URIs follow `System.Uri` rules; relative paths need `BaseAddress`. Supports raw response, typed `*Async<T>`, string `*ResultAsync<T>`, and `HttpRequestMessage` overloads | Assuming every overload preserves non-success responses like `*ResultAsync` |
| `HttpResult<T>` | Keep a typed payload with the raw response | `HasResult` is false when deserialization did not produce a payload | Assuming it throws on non-success status codes |
| `IHttpClientSerializer` | Replace request and response serialization behavior | Used for both payload serialization and stream deserialization | Forgetting to register a serializer compatible with the API contract |
| `SystemTextJsonHttpClientSerializer` | Use default JSON serialization | Uses camelCase, case-insensitive property matching, numeric strings, and camelCase enum text | Assuming it matches every external API by default |
| `ICorrelationContextFactory` | Provide the outbound correlation-context header value per request | Empty fallback returns null and header emission is skipped | Assuming correlation data exists without a custom implementation |
| `ICorrelationIdFactory` | Provide the outbound correlation ID header value per request | Empty fallback returns null and header emission is skipped | Assuming a correlation ID will always be sent |

## Request And Response Semantics

### Success Handling

- String-based `GetAsync<T>`, `PostAsync<T>`, `PutAsync<T>`, `PatchAsync<T>`, and `DeleteAsync<T>` use the exception-oriented path and throw when the response is not successful.
- String-based `GetResultAsync<T>` and the other `*ResultAsync<T>(string, ...)` methods return `HttpResult<T>` even when the status code is not successful.
- `SendAsync(HttpRequestMessage)` sends once and returns the raw `HttpResponseMessage`.
- Typed methods that return `T?` consume and dispose transient responses; methods returning `HttpResponseMessage` or `HttpResult<T>` leave response ownership with the caller.

### Exception Handling

- String-based helpers that use the exception-oriented send path (`GetAsync`, `PostAsync`, typed `GetAsync<T>`, etc.) throw when the final HTTP status is not successful.
- String-based `*ResultAsync<T>(string, ...)` methods do **not** throw solely because the status code indicates failure; they return `HttpResult<T>` with the raw `HttpResponseMessage`.
- `SendAsync<T>(HttpRequestMessage, ...)` throws for non-success responses and does not retry by replaying the same request instance.
- `SendResultAsync<T>(HttpRequestMessage, ...)` preserves non-success responses in `HttpResult<T>` (no throw solely for status).
- Retry logic is exception-driven on the Polly-wrapped string-URI helper paths. HTTP error status codes are not retried as exceptions on the string `*ResultAsync` path.
- Cancellation-driven failures are not retried; cancellation should terminate work immediately.

### URI Handling

- String URI arguments must be valid **absolute** or **relative** URIs as accepted by `System.Uri` (`Uri.TryCreate` with `UriKind.RelativeOrAbsolute`). Invalid strings throw `ArgumentException` (`paramName: uri`).
- **Absolute** URIs (for example `https://api.contoso.com/v1/items`) are sent unchanged; the client does not rewrite schemes or hosts.
- **Relative** paths (for example `items/5` or `catalog/products/1`) combine with `HttpClient.BaseAddress` when it is configured (typically via `AddHttpClient(..., httpClientBuilder: b => b.ConfigureHttpClient(c => c.BaseAddress = ...))`). This matches standard `HttpClient` / `Uri` resolution; ensure `BaseAddress` ends with `/` when you intend path segments to append predictably.
- The client does **not** prepend `http://` to strings that lack a scheme. Callers must use an absolute URI or set `BaseAddress` and pass relative paths (see Recipe 2).

## Configuration Ownership

`Genocs.Http` reads the `httpClient` section into `HttpClientOptions`.

```json
{
  "httpClient": {
    "enabled": true,
    "type": "consul",
    "retries": 3,
    "retryUnsafeHttpMethods": false,
    "services": {
      "orders": "http://orders-service"
    },
    "removeCharsetFromContentType": true,
    "correlationContextHeader": "x-correlation-context",
    "correlationIdHeader": "x-correlation-id",
    "requestMasking": {
      "enabled": true,
      "urlParts": ["token", "password"],
      "maskTemplate": "*****"
    }
  }
}
```

What this package actively uses:

- `retries`
- `retryUnsafeHttpMethods`
- `removeCharsetFromContentType`
- `correlationContextHeader`
- `correlationIdHeader`
- `requestMasking.enabled`
- `requestMasking.urlParts`
- `requestMasking.maskTemplate`

What this package defines but does not actively implement by itself:

- `enabled`
- `type`
- `services`

Treat those latter fields as package-shared configuration shape unless another package explicitly documents runtime behavior for them.

## Package Quality Gates (Maintainers)

Genocs.Http applies package-scoped quality gates in `Genocs.Http.csproj`:

- nullable warnings are treated as errors (`WarningsAsErrors` includes `nullable`)
- .NET analyzer warnings are treated as errors (`CodeAnalysisTreatWarningsAsErrors=true`)

This is intentionally package-scoped so Genocs.Http public-surface regressions are caught early without forcing the entire repository to adopt the same warning baseline immediately.

## Package Ownership Boundaries

`Genocs.Http` owns:

- typed `IHttpClient` registration (`AddHttpClient(...)`)
- outbound request execution and retry behavior for string-URI helper paths
- serializer abstraction and default `System.Text.Json` implementation
- per-request correlation header enrichment hooks
- URL masking for HTTP client log URI rendering

`Genocs.Http` does not own by itself:

- service discovery or load-balancing execution from `httpClient.type` / `httpClient.services`
- advanced resilience orchestration beyond built-in retry behavior (for example circuit breaker, fallback, hedging)
- request/response body redaction or arbitrary structured log-property redaction
- downstream service registration topology beyond the single typed `IHttpClient` registration

When users need these capabilities, route guidance to companion packages or external infrastructure explicitly.

## Migration Guide (April 2026)

Use this checklist when upgrading from older Genocs.Http behavior.

1. Result wrappers and non-success status handling
- Before: some paths could surface non-success responses as exceptions unexpectedly.
- Now: string `*ResultAsync<T>(...)` methods preserve non-success responses in `HttpResult<T>`.
- Action: switch status-inspection flows to `*ResultAsync<T>` and inspect `HttpResult<T>.Response`.

2. URI handling and `BaseAddress`
- Before: host-like strings could be rewritten with an implicit `http://` prefix.
- Now: no implicit scheme rewrite; URIs follow `System.Uri` absolute/relative rules only.
- Action: pass absolute URIs explicitly, or set `HttpClient.BaseAddress` and pass relative paths.

3. Retry boundaries and write-method defaults
- Before: retry behavior could be interpreted as broad exception retry.
- Now: retries are transport-focused; non-success HTTP statuses and deserialization failures are not retried; `POST`/`PUT`/`PATCH` retries require `retryUnsafeHttpMethods=true`.
- Action: review write-path resilience and avoid unintended retry amplification with extra pipeline policies.

4. `HttpRequestMessage` overload replay behavior
- Before: callers could assume internal replay safety for request-message overloads.
- Now: `HttpRequestMessage` overloads are single-send and do not replay the same request instance.
- Action: move replay-safe resilience to the handler/pipeline layer if needed.

5. Cancellation and response ownership
- Before: cancellation/retry and response lifecycle boundaries were less explicit.
- Now: cancellation-driven failures are not retried; typed methods returning `T?` dispose transient responses.
- Action: propagate cancellation tokens end-to-end and use `HttpResult<T>` / raw `HttpResponseMessage` when caller-owned response lifecycle is required.

6. Correlation and masking semantics
- Before: correlation and masking behavior could be interpreted as broad/global mutation.
- Now: correlation headers are injected per request with caller-header precedence; masking is exact-token URI-log replacement only.
- Action: keep caller-supplied correlation headers authoritative when needed, and use dedicated redaction solutions for bodies/headers.

## Public Capability Map

### Registration And Integration

- `AddHttpClient(...)`
- `RemoveHttpClient()`
- `HttpClientOptions`

### Outbound Request APIs

- `IHttpClient.GetAsync(...)`
- `IHttpClient.PostAsync(...)`
- `IHttpClient.PutAsync(...)`
- `IHttpClient.PatchAsync(...)`
- `IHttpClient.DeleteAsync(...)`
- `IHttpClient.SendAsync(...)`
- `IHttpClient.SendResultAsync(...)`

### Serialization

- `IHttpClientSerializer`
- `SystemTextJsonHttpClientSerializer`

### Response Wrapping

- `HttpResult<T>`

### Correlation Extension Points

- `ICorrelationContextFactory`
- `ICorrelationIdFactory`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `httpClient.type` enables service discovery or load balancing.
2. Do not assume `httpClient.services` rewrites host names or resolves logical services.
3. Do not assume non-success responses are preserved unless you use string `*ResultAsync<T>`, `SendResultAsync<T>(HttpRequestMessage, ...)`, or methods that return `HttpResponseMessage` without applying the exception-oriented typed path.
4. Do not assume retries cover every failure mode; they run on exceptions only.
5. Do not assume request masking redacts bodies, headers, or arbitrary structured log properties.
6. Do not assume correlation headers are sent unless header names are configured and value factories are meaningful, and note that caller-provided request headers take precedence.
7. Do not assume the package defines timeouts, circuit breakers, fallback policies, or hedging.
8. Do not assume multiple named typed clients are registered just because `clientName` is configurable.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Http`, answer these questions:

1. Is `Genocs.Core` already installed so `IGenocsBuilder` is available?
2. Does the caller need exception-based flow or status-aware result handling?
3. Are downstream URIs absolute, or is `HttpClient.BaseAddress` set so relative paths resolve correctly (there is no implicit `http://` prefix)?
4. Does the API require custom serialization behavior?
5. Are correlation headers required, and if so, where do their values come from?
6. Is masking needed only for URL fragments, or is full payload redaction required from another layer?
7. Are additional `HttpClientFactory` policies being configured through the optional `httpClientBuilder` callback?

If any answer is unknown, prefer minimal registration plus `*ResultAsync<T>` methods for safer caller behavior.

## Common Tasks And Safe Responses

### Task: "Register an outbound HTTP client"

Safe response:
- call `builder.AddGenocs().AddHttpClient()`
- keep configuration in `httpClient`
- avoid promising service discovery unless another package is confirmed

### Task: "Call an API and inspect failures"

Safe response:
- use `GetResultAsync<T>` or another `*ResultAsync<T>` overload
- inspect `Response.StatusCode` instead of relying on exceptions alone

### Task: "Customize JSON behavior"

Safe response:
- register a custom `IHttpClientSerializer`
- ensure request and response contracts match the downstream API

### Task: "Add correlation IDs to outbound requests"

Safe response:
- configure `correlationIdHeader` and or `correlationContextHeader`
- register custom factory implementations

### Task: "Hide secrets in logs"

Safe response:
- use `requestMasking` only for URL-part masking
- ask for a separate logging or redaction solution for bodies and headers
- explain that masking is exact-token replacement over logged URI text, and replacements are not reparsed as `Uri`

## Failure Modes And Troubleshooting

1. Non-success responses are turning into exceptions.
Fix: Use `GetResultAsync<T>` or another `*ResultAsync<T>` overload when the caller must inspect status codes without exception flow.

2. Retries are not happening.
Fix: Ensure `httpClient.retries` is greater than zero, the failure path actually throws an exception, and `httpClient.retryUnsafeHttpMethods=true` is set when expecting retries for `POST`/`PUT`/`PATCH`.

3. Correlation headers are missing.
Fix: Configure the header names and register non-empty implementations of `ICorrelationContextFactory` or `ICorrelationIdFactory`.

4. Correlation headers look stale or do not match the current request scope.
Fix: Use factory implementations that resolve values per request context (for example through `IHttpContextAccessor`) and prefer explicit caller-supplied request headers when correlation ownership should be set at the call site.

5. URL masking is enabled but logs still show sensitive data.
Fix: Verify the sensitive values appear in the URL, not in the body or headers, and ensure `requestMasking.urlParts` contains exact case-sensitive tokens present in the logged URI string.

6. Requests are going to the wrong host or path.
Fix: Use fully qualified absolute URIs, or set `BaseAddress` on the named client and pass relative paths. Do not rely on implicit scheme prefixing; configure HTTPS explicitly when required.

7. Response content deserializes to `null` unexpectedly.
Fix: Check the serializer contract, the response body format, and whether the API returned a non-success response that caused a default result.

8. Content type includes an unwanted charset.
Fix: Set `removeCharsetFromContentType` to `true` so JSON payloads omit the charset parameter.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Logging`
- `Genocs.Telemetry`
- `Genocs.WebApi`
- `Genocs.Messaging`

## One-Line Recommendation For Agents

If you only know that `Genocs.Http` is installed, generate one typed outbound client through `AddHttpClient()`, configure `BaseAddress` when using relative URI strings, prefer `*ResultAsync<T>` when failure inspection matters, and do not promise discovery, advanced resiliency, or redaction features that this package does not implement.
