# Genocs.WebApi Agent Reference

## Agent Operating Mode

- Assume `Genocs.WebApi` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, interfaces, and public helper types as the only safe API surface.
- Generate ASP.NET Core host composition, endpoint mapping, request-binding, and error-handling code only.
- Do not invent controller discovery, OpenAPI generation, authentication schemes, or CQRS transport behavior that this package does not provide.
- If package composition is unclear, ask whether `Genocs.Core` is installed because `AddWebApi(...)` extends `IGenocsBuilder`.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.WebApi` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | ASP.NET Core Web API conventions, endpoint DSL, and request/response helpers for Genocs applications |
| Main value | Web API bootstrap, minimal endpoint mapping DSL, JSON formatter registration, forwarded-header support, request dispatcher abstractions, exception-to-response middleware |
| Requires | `Genocs.Core` and ASP.NET Core host composition |

## What This Package Is For

Use `Genocs.WebApi` when you need to:

- register Genocs Web API services through `AddWebApi()`
- define lightweight endpoints through `UseEndpoints(...)` and `IEndpointsBuilder`
- read JSON bodies and query or route values into DTOs
- write consistent JSON responses through response helper methods
- centralize exception mapping with `AddErrorHandler<T>()` and `UseErrorHandler()`
- trust forwarded headers behind a reverse proxy
- dispatch request DTOs to `IRequestHandler<TRequest, TResult>` implementations

## What This Package Does Not Do By Itself

Do not assume `Genocs.WebApi` can:

- map MVC controllers automatically
- generate OpenAPI documents by itself
- register authentication schemes or JWT validation
- register CQRS command, query, or event handlers from `Genocs.Core`
- replace ASP.NET Core endpoint routing with a complete framework abstraction
- validate proxy sources safely by default when forwarded headers are enabled broadly
- provide a production-ready exception mapping policy unless you implement one

## Safe Default Mental Model

Treat `Genocs.WebApi` as five things:

1. A Genocs builder extension that registers Web API services and JSON formatters
2. A small endpoint-mapping DSL over ASP.NET Core routing
3. A request and response helper layer for JSON, route values, and common HTTP statuses
4. A request-dispatch abstraction for `IRequest` and `IRequestHandler<TRequest, TResult>`
5. An optional exception middleware hook that needs a mapper implementation to be useful

If a user asks for Swagger, auth, telemetry, or CQRS execution beyond these boundaries, identify the missing companion package first.

## Fast Start Recipes

### Recipe 1: Minimal Endpoint DSL Setup

Use this when the application wants Genocs endpoint helpers instead of controllers.

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs()
    .AddWebApi();

genocs.Build();

var app = builder.Build();

app.UseEndpoints(endpoints =>
{
    endpoints.Get("/health", async context =>
        await context.Response.Ok(new { status = "ok" }));
});

app.Run();
```

Effect:
- registers a JSON serializer and custom MVC input and output formatters
- enables endpoint DSL mapping through `IEndpointsBuilder`
- registers `IRequestDispatcher` and scans loaded assemblies for `IRequestHandler<,>` implementations
- stores endpoint metadata in `WebApiEndpointDefinitions`

### Recipe 2: Bind A Request DTO From Query String

Use this when a GET endpoint should materialize a DTO from query and route values.

```csharp
using Genocs.WebApi;

app.UseEndpoints(endpoints =>
{
    endpoints.Get<SearchProducts>("/products", async (request, context) =>
    {
        await context.Response.Ok(new { request.Term, request.Page });
    });
});

public sealed record SearchProducts(string Term, int Page = 0);
```

For `Get<T>` and `Delete<T>`, the package uses query and route binding rather than JSON body binding.

### Recipe 3: Bind A POST Body And Return Created

Use this when the endpoint receives JSON and returns a resource location.

```csharp
using Genocs.WebApi;

app.UseEndpoints(endpoints =>
{
    endpoints.Post<CreateProduct>("/products", async (request, context) =>
    {
        await context.Response.Created($"/products/{request.Id}", request);
    });
});

public sealed record CreateProduct(Guid Id, string Name);
```

If `webApi.bindRequestFromRoute` is enabled, route values can also populate matching DTO members during JSON body binding.

### Recipe 4: Add Centralized Exception Mapping

Use this when exceptions should be converted into JSON responses consistently.

```csharp
using System.Net;
using Genocs.Core.Builders;
using Genocs.WebApi;
using Genocs.WebApi.Exceptions;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder.AddGenocs()
    .AddWebApi()
    .AddErrorHandler<MyExceptionToResponseMapper>();

genocs.Build();

var app = builder.Build();
app.UseErrorHandler();
app.Run();

public sealed class MyExceptionToResponseMapper : IExceptionToResponseMapper
{
    public ExceptionResponse? Map(Exception exception)
        => new(new { message = exception.Message }, HttpStatusCode.BadRequest);
}
```

Without a mapper, the built-in fallback returns no mapped payload.

### Recipe 5: Dispatch A Request To A Handler

Use this when endpoint code should hand off to a request handler abstraction.

```csharp
using Genocs.WebApi;
using Genocs.WebApi.Requests;

app.UseEndpoints(endpoints =>
{
    endpoints.Post<CreateOrder>("/orders", async (request, context) =>
    {
        var result = await context.DispatchAsync<CreateOrder, OrderCreated>(request);
        await context.Response.Ok(result);
    });
});

public sealed record CreateOrder(Guid CustomerId) : IRequest;
public sealed record OrderCreated(Guid Id);
```

This uses `IRequestHandler<TRequest, TResult>`, not the CQRS handler interfaces from `Genocs.Core`.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddWebApi(...)` | Register Genocs Web API services | Registers JSON serializer, MVC core services, custom input and output formatters, request handlers, `IRequestDispatcher`, and endpoint definition storage | Assuming it maps endpoints or controllers automatically |
| `UseEndpoints(...)` | Map endpoints with the Genocs DSL | Calls `UseRouting()`, optional `UseAuthorization()`, optional custom middleware, then ASP.NET Core `UseEndpoints(...)` | Calling `UseRouting()` again around it without understanding pipeline order |
| `AddErrorHandler<T>()` | Register exception-to-response mapping | Registers `ErrorHandlerMiddleware` and your `IExceptionToResponseMapper` implementation | Assuming it also adds the middleware to the pipeline |
| `UseErrorHandler()` | Activate exception middleware | Catches exceptions and maps them to JSON or empty responses | Forgetting to register a mapper first |
| `UseAllForwardedHeaders()` | Accept forwarded headers from proxies | Enables `ForwardedHeaders.All`; by default it preserves ASP.NET Core trusted proxy/network checks, and optionally clears known networks and proxies when explicitly requested | Using permissive trust mode on an untrusted network edge |
| `IEndpointsBuilder` | Define GET, POST, PUT, and DELETE endpoints | Supports optional auth, roles, policies, and endpoint customization callbacks | Assuming it covers PATCH, HEAD, or full minimal API surface |
| `ReadJsonAsync<T>()` | Deserialize a JSON request body | Returns `default` and writes 400 on invalid input; can merge route values into DTO fields | Assuming exceptions bubble instead of a 400 response being written |
| `ReadQuery<T>()` | Materialize a DTO from route and query values | Uses serializer-based object reconstruction | Assuming full model-binding parity with MVC |
| `DispatchAsync<TRequest, TResult>()` | Resolve and execute a request handler from `HttpContext` | Requires an `IRequestHandler<TRequest, TResult>` registration | Assuming it uses `IDispatcher` from `Genocs.Core` |
| Response helpers such as `Ok()`, `Created()`, `NoContent()` | Write standard HTTP responses | Most helpers set only status code; JSON is written only when a payload is supplied | Assuming helpers add content automatically |

## Endpoint DSL Semantics

### Supported Verbs

- `Get(...)`
- `Post(...)`
- `Put(...)`
- `Delete(...)`

### Binding Behavior

- `Get<T>` and `Delete<T>` use `ReadQuery<T>()`, which combines route values and query string values.
- `Post<T>` and `Put<T>` use `ReadJsonAsync<T>()`, which reads the JSON body.
- If `bindRequestFromRoute` is enabled, `ReadJsonAsync<T>()` also writes matching route values into DTO backing fields after deserialization.
- `Get<TRequest, TResult>` exists to capture both request and response endpoint metadata, but the provided callback still receives `TRequest` and `HttpContext` only.

### Authorization Behavior

- If `policies` are supplied, the endpoint requires those authorization policies.
- Else if `roles` is supplied, the endpoint requires those roles.
- Else if `auth` is `true`, the endpoint requires authorization.
- Otherwise the endpoint is explicitly marked anonymous.

## Configuration Ownership

`Genocs.WebApi` reads the `webApi` section into `WebApiOptions`.

```json
{
  "webApi": {
    "bindRequestFromRoute": true
  }
}
```

What this package actively uses:

- `bindRequestFromRoute`

What it means:

- when enabled, route values are written into matching DTO fields during JSON body binding

Important detail:

- this route merge relies on matching generated backing-field names, so it is safest with standard auto-property DTOs and record types

## Public Capability Map

### Host And Pipeline Registration

- `AddWebApi(...)`
- `AddErrorHandler<T>()`
- `UseEndpoints(...)`
- `UseErrorHandler()`
- `UseAllForwardedHeaders()`

### Endpoint Composition

- `IEndpointsBuilder`
- `EndpointsBuilder`
- `WebApiEndpointDefinitions`
- `WebApiEndpointDefinition`
- `WebApiEndpointParameter`
- `WebApiEndpointResponse`

### Request Dispatching

- `IRequest`
- `IRequestHandler<TRequest, TResult>`
- `IRequestDispatcher`
- `RequestDispatcher`
- `DispatchAsync<TRequest, TResult>(...)`

### Request And Response Helpers

- `ReadJsonAsync<T>()`
- `ReadQuery<T>()`
- `Args()` and `Args<T>()`
- `Ok()`
- `Created()`
- `Accepted()`
- `NoContent()`
- `MovedPermanently()`
- `Redirect()`
- `BadRequest()`
- `Unauthorized()`
- `Forbidden()`
- `NotFound()`
- `InternalServerError()`
- `WriteJsonAsync<T>()`
- `Bind(...)`
- `BindId(...)`

### Exception Mapping

- `IExceptionToResponseMapper`
- `ExceptionResponse`

### Options And Utilities

- `WebApiOptions`
- `JsonParser`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `AddWebApi()` maps controllers or endpoints automatically.
2. Do not assume `UseErrorHandler()` does anything useful without a real `IExceptionToResponseMapper`.
3. Do not assume `UseEndpoints(...)` is the same as minimal APIs on `WebApplication`; it wraps classic ASP.NET Core endpoint routing.
4. Do not assume PATCH support exists in `IEndpointsBuilder`.
5. Do not assume request DTO binding behaves exactly like MVC model binding.
6. Do not assume `DispatchAsync<TRequest, TResult>()` integrates with `Genocs.Core` dispatchers.
7. Do not assume forwarded headers are safe to trust on every deployment edge.
8. Do not use `WebApiConfigureOptions` directly; it is public but not a usable consumer entry point.

## Agent Decision Checklist

Before generating code that depends on `Genocs.WebApi`, answer these questions:

1. Is the host using ASP.NET Core endpoint routing?
2. Does the app want Genocs endpoint DSL methods, controllers, or both?
3. Should request DTOs be read from query, route, body, or a mix?
4. Is centralized exception mapping required?
5. Is authorization already configured in the host if endpoint auth flags will be used?
6. Is the app behind a trusted reverse proxy before enabling `UseAllForwardedHeaders()` broadly?
7. Are `IRequestHandler<,>` handlers already present in loaded assemblies if request dispatch is needed?

If any answer is unknown, prefer a minimal anonymous endpoint plus explicit response helpers.

## Common Tasks And Safe Responses

### Task: "Add a lightweight endpoint"

Safe response:
- call `AddWebApi()` during service registration
- call `UseEndpoints(...)` in the pipeline
- use `Ok()` or other response helpers explicitly

### Task: "Read route and query parameters into a DTO"

Safe response:
- use `Get<T>` or `Delete<T>`
- model the DTO with auto-properties or a record shape

### Task: "Read a JSON body and merge route values"

Safe response:
- use `Post<T>` or `Put<T>`
- enable `webApi.bindRequestFromRoute` only when the DTO shape is compatible

### Task: "Handle errors consistently"

Safe response:
- implement `IExceptionToResponseMapper`
- register it with `AddErrorHandler<T>()`
- add `UseErrorHandler()` before endpoint execution

### Task: "Dispatch an endpoint request to a handler"

Safe response:
- define a request type implementing `IRequest`
- implement `IRequestHandler<TRequest, TResult>`
- call `context.DispatchAsync<TRequest, TResult>(request)`

## Failure Modes And Troubleshooting

1. Endpoints never execute.
Fix: Call `UseEndpoints(...)` after the app is built and before `Run()`. Do not assume `AddWebApi()` maps anything by itself.

2. Exceptions still produce default responses.
Fix: Register `AddErrorHandler<T>()` with a real mapper and add `UseErrorHandler()` to the pipeline.

3. A request DTO is null and the response is already 400.
Fix: Check JSON validity, data-annotation validation results, and the serializer contract used by `ReadJsonAsync<T>()`.

4. Route values do not populate the request body DTO.
Fix: Enable `webApi.bindRequestFromRoute` and ensure the DTO uses standard auto-properties or record-generated backing fields.

5. Authorization metadata is present but requests still fail unexpectedly.
Fix: Confirm authentication and authorization services are configured separately in the host. This package only adds endpoint authorization metadata and `AddAuthorization()`.

6. Forwarded headers change client IP or scheme unexpectedly.
Fix: Keep `UseAllForwardedHeaders()` default strict behavior unless your reverse proxy boundary is trusted; only opt in to `resetKnownNetworksAndProxies: true` for controlled environments.

7. `DispatchAsync<TRequest, TResult>()` cannot resolve a handler.
Fix: Ensure the relevant assembly is already loaded and contains an `IRequestHandler<TRequest, TResult>` implementation that can be scanned during `AddWebApi()`.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi.OpenApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Auth`
- `Genocs.Logging`
- `Genocs.Telemetry`

## Maintainer Validation Command

When working in the repository, validate package warning baseline and regression tests with:

```bash
make validate-webapi
```

## One-Line Recommendation For Agents

If you only know that `Genocs.WebApi` is installed, generate explicit ASP.NET Core endpoint-routing code with `AddWebApi()` and `UseEndpoints(...)`, rely on the package’s JSON and response helpers, and ask before assuming controllers, Swagger, auth, or broader CQRS infrastructure.
