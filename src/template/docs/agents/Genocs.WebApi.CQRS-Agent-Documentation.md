# Genocs.WebApi.CQRS Agent Reference

## Agent Operating Mode

- Assume `Genocs.WebApi.CQRS` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, interfaces, and middleware behavior as the only safe API surface.
- Generate Web API endpoint wiring that bridges to existing CQRS dispatchers. Do not invent dispatcher registration that this package does not provide.
- If package composition is unclear, ask whether `Genocs.Core` and `Genocs.WebApi` are already installed, because this package layers on top of both.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.WebApi.CQRS` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Bridge Genocs.WebApi endpoint mapping to Genocs CQRS dispatchers |
| Main value | CQRS-aware endpoint builder, `HttpContext` dispatch helpers, optional public command and event contracts endpoint, unified `IDispatcher` facade |
| Requires | `Genocs.WebApi`, `Genocs.Core`, and actual command or query dispatcher registrations |

## What This Package Is For

Use `Genocs.WebApi.CQRS` when you need to:

- expose HTTP endpoints that forward commands to `ICommandDispatcher`
- expose HTTP endpoints that forward queries to `IQueryDispatcher`
- keep endpoint definitions declarative through a CQRS-specific endpoint builder
- add `HttpContext` helpers for command send and query execution
- expose a runtime JSON snapshot of public command and event contracts for tooling or agents
- register a single `IDispatcher` facade that wraps existing command, query, and event dispatchers

## What This Package Does Not Do By Itself

Do not assume `Genocs.WebApi.CQRS` can:

- register `ICommandDispatcher`, `IQueryDispatcher`, or `IEventDispatcher`
- register command, query, or event handlers automatically
- expose queries in the public contracts endpoint
- create OpenAPI documents or client SDKs
- replace `Genocs.WebApi.AddWebApi()`
- replace `Genocs.Core.AddDispatchers()` or other Core CQRS registration methods
- provide brokered messaging or external event transport
- discover assemblies that are not already loaded into the AppDomain

## Safe Default Mental Model

Treat `Genocs.WebApi.CQRS` as four things:

1. A thin adapter from HTTP endpoints to existing Genocs CQRS dispatchers
2. A CQRS-specific builder on top of `Genocs.WebApi.IEndpointsBuilder`
3. A convenience `IDispatcher` facade over command, query, and event dispatching
4. A middleware that emits a snapshot of loaded command and event contract shapes

If a user asks for CQRS runtime registration, handler scanning, or HTTP conventions outside those boundaries, identify the missing companion package first.

## Fast Start Recipes

### Recipe 1: Minimal CQRS Endpoint Setup

Use this when the application already wants Genocs WebApi routing and Genocs Core dispatchers.

```csharp
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDispatchers()
    .AddHandlers("MyService");

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddWebApi()
    .AddInMemoryDispatcher();

genocs.Build();

var app = builder.Build();

app.MapDispatcherEndpoints(endpoints => endpoints
    .Post<CreateOrder>("/orders")
    .Get<GetOrder, OrderDto>("/orders/{id}"));

app.Run();

public sealed record CreateOrder(Guid CustomerId, decimal Amount) : Genocs.Common.CQRS.Commands.ICommand;
public sealed record GetOrder(Guid Id) : Genocs.Common.CQRS.Queries.IQuery<OrderDto>;
public sealed record OrderDto(Guid Id, string Status);
```

Effect:

- command endpoints deserialize request bodies and send commands through `ICommandDispatcher`
- query endpoints bind route or query values and execute queries through `IQueryDispatcher`
- `AddInMemoryDispatcher()` also registers a unified `IDispatcher` facade for non-HTTP consumers

### Recipe 2: Customize Behavior Before And After Dispatch

Use this when an endpoint needs audit, validation, or response customization around CQRS execution.

```csharp
using Genocs.WebApi.CQRS;

app.MapDispatcherEndpoints(endpoints => endpoints
    .Get<GetOrder, OrderDto>(
        "/orders/{id}",
        beforeDispatch: async (query, context, cancellationToken) =>
        {
            await Task.CompletedTask;
        },
        afterDispatch: async (query, result, context, cancellationToken) =>
        {
            if (result is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await context.Response.WriteJsonAsync(result);
        }));
```

Without `afterDispatch`, typed query endpoints automatically return `404` when the query result is `null` and otherwise write JSON.

### Recipe 3: Add CQRS Dispatch To Existing WebApi Endpoints

Use this when the application already maps endpoints through `Genocs.WebApi.UseEndpoints(...)` and only some routes should dispatch through CQRS.

```csharp
using Genocs.WebApi;
using Genocs.WebApi.CQRS;

app.UseEndpoints(endpoints =>
{
    endpoints
        .Dispatch(dispatcher => dispatcher
            .Post<CreateOrder>("/orders")
            .Get<GetOrder, OrderDto>("/orders/{id}"));
});
```

This keeps one endpoint-mapping strategy while mixing CQRS routes with non-CQRS routes.

### Recipe 4: Expose Public Command And Event Contracts

Use this when another service, tool, or agent needs a JSON snapshot of public message shapes.

```csharp
using Genocs.Common.Types;
using Genocs.WebApi.CQRS;

app.UsePublicContracts<PublicContractAttribute>("/_contracts");
```

Or expose all loaded command and event contracts, even when they are not decorated:

```csharp
app.UsePublicContracts(attributeRequired: false, endpoint: "/_contracts");
```

This endpoint includes commands and events only. It does not include queries.

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddInMemoryDispatcher()` | Register the unified `IDispatcher` facade | Adds `IDispatcher` as a singleton backed by `InMemoryDispatcher` | Assuming it also registers `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher` |
| `MapDispatcherEndpoints(...)` | Map CQRS-focused endpoints directly on endpoint route builders | Uses host-owned endpoint routing and does not mutate middleware ordering | Assuming it configures `UseRouting()` or `UseAuthorization()` automatically |
| `UseDispatcherEndpoints(...)` | Legacy compatibility entry point | Obsolete. Delegates to endpoint-route mapping on hosts implementing `IEndpointRouteBuilder` | Treating it as the preferred API for new hosts |
| `Dispatch(...)` | Add CQRS endpoints inside an existing `IEndpointsBuilder` chain | Builds a `DispatcherEndpointsBuilder` over the current WebApi endpoint builder | Assuming it performs routing or authorization by itself |
| `UsePublicContracts<T>()` | Expose only types decorated with a specific attribute | Uses the supplied attribute type and defaults to the `/_contracts` endpoint | Assuming it exposes queries too |
| `UsePublicContracts(bool attributeRequired, string endpoint)` | Expose public contracts with or without attribute filtering | Uses `PublicContractAttribute` when filtering is enabled | Assuming the endpoint updates dynamically as later assemblies load |
| `HttpContext.SendAsync<T>()` | Send a command from custom endpoint code | Resolves `ICommandDispatcher` from request services | Using it without registering command dispatching first |
| `HttpContext.QueryAsync<TResult>()` | Execute a query from custom endpoint code | Resolves `IQueryDispatcher` from request services | Assuming null results map themselves to HTTP statuses |
| `HttpContext.QueryAsync<TQuery, TResult>()` | Execute a strongly typed query | Uses the typed generic dispatcher overload | Assuming it will work without a typed handler registration |
| `IDispatcher` | Depend on one facade for send, publish, and query | Delegates to existing command, event, and query dispatchers | Treating it as a replacement for Core dispatcher registration |
| `IDispatcherEndpointsBuilder` | Define declarative CQRS GET, POST, PUT, and DELETE routes | Wraps `Genocs.WebApi.IEndpointsBuilder` and preserves auth, roles, and policy options | Assuming PATCH or controller-style model binding exists |

## Endpoint Builder Semantics

### Supported Verbs

- `Get(...)`
- `Post(...)`
- `Put(...)`
- `Delete(...)`

### Typed Query Endpoints

- `Get<TQuery, TResult>(...)` requires `TQuery : class, IQuery<TResult>`.
- The request object is bound by the underlying `Genocs.WebApi` GET binding behavior, which uses route and query values rather than JSON body binding.
- `beforeDispatch` runs before the query dispatcher is called.
- `afterDispatch` runs after the query returns and receives the possibly null result.
- cancellation token parameters are supplied from `HttpContext.RequestAborted`.
- If `afterDispatch` is omitted and the result is `null`, the endpoint returns `404`.
- If `afterDispatch` is omitted and the result is not `null`, the endpoint writes the result as JSON.

### Typed Command Endpoints

- `Post<T>(...)`, `Put<T>(...)`, and `Delete<T>(...)` require `T : class, ICommand`.
- The request object is bound by the underlying `Genocs.WebApi` behavior for that HTTP verb.
- `beforeDispatch` runs before `ICommandDispatcher.SendAsync(...)`.
- `afterDispatch` runs after command dispatch completes.
- cancellation token parameters are supplied from `HttpContext.RequestAborted`.
- When dispatch succeeds and no custom response is written, the package sets the response status code to `200`.
- There is no default `201 Created`, `202 Accepted`, or response payload.

### Authorization Parameters

All builder methods preserve the underlying WebApi endpoint options:

- `auth`
- `roles`
- `policies`
- `endpoint` customization callback

Authorization metadata only works when the host has authentication and authorization configured separately.

## Public Contracts Middleware Semantics

`UsePublicContracts(...)` adds middleware that intercepts one exact request path and writes a JSON document containing:

- `commands`
- `events`

Important behavior:

- the middleware scans only assemblies already loaded in the current AppDomain
- it ignores interfaces
- it excludes `RejectedEvent`
- it creates default instances of discovered types and serializes those instances
- duplicate command or event type names use deterministic key fallback (simple name, then full name, then assembly-qualified fragment)
- the contracts snapshot is scoped to middleware instance initialization, not static process-wide state
- the response is always JSON and does not perform content negotiation
- queries are not included

This makes the middleware useful for schema discovery, but not for dynamic runtime introspection of newly loaded assemblies.

## Configuration Ownership

`Genocs.WebApi.CQRS` does not define its own configuration section.

Runtime behavior comes from companion packages instead:

| Source | What it affects |
|---|---|
| `Genocs.Core` CQRS registrations | Whether commands, queries, and events can actually be dispatched |
| `Genocs.WebApi` `webApi` section | Request binding behavior for typed endpoint DTOs |
| Host authentication and authorization setup | Whether `auth`, `roles`, and `policies` behave as expected |

If a user asks for configuration-driven behavior, point them to the underlying package that owns that setting.

## Public Capability Map

### Registration And Pipeline

- `AddInMemoryDispatcher()`
- `MapDispatcherEndpoints(...)`
- `UseDispatcherEndpoints(...)`
- `Dispatch(...)`
- `UsePublicContracts<T>()`
- `UsePublicContracts(bool attributeRequired, string endpoint)`
- `UsePublicContracts(string endpoint, Type attributeType = null, bool attributeRequired = true)`

### Dispatcher Abstractions

- `IDispatcher`
- `InMemoryDispatcher`

### Endpoint Composition

- `IDispatcherEndpointsBuilder`
- `DispatcherEndpointsBuilder`

### HttpContext Helpers

- `SendAsync<T>(this HttpContext context, T command)`
- `QueryAsync<TResult>(this HttpContext context, IQuery<TResult> query)`
- `QueryAsync<TQuery, TResult>(this HttpContext context, TQuery query)`

### Middleware

- `PublicContractsMiddleware`

## Maintainer Validation

From repository root, run:

```bash
make validate-webapi-cqrs
```

This target validates CQRS package build baseline and executes the CQRS unit test project.

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `AddInMemoryDispatcher()` registers the Core dispatchers that it depends on.
2. Do not assume command endpoints return a payload or `201 Created`; they default to `200` only.
3. Do not assume query endpoints return `200` on null results; they default to `404` when `afterDispatch` is not provided.
4. Do not assume `UsePublicContracts(...)` exposes queries.
5. Do not assume contract discovery includes assemblies that are not yet loaded.
6. Do not assume authorization flags work unless the host already configured authentication and authorization.
7. Do not assume the public contracts endpoint is versioned, filtered by namespace, or secured automatically.
8. Do not assume duplicate type names across assemblies are safe; the middleware treats them as a runtime error.

## Agent Decision Checklist

Before generating code that depends on `Genocs.WebApi.CQRS`, answer these questions:

1. Is `Genocs.WebApi` already installed and used for endpoint mapping?
2. Are Core CQRS dispatchers already registered in DI?
3. Are handler assemblies already loaded so dispatching can resolve handlers?
4. Should command endpoints return only `200`, or should a custom response be written in `afterDispatch`?
5. Should null query results become `404`, or should the endpoint shape that response differently?
6. Should the public contracts endpoint expose only decorated types or every loaded command and event contract?
7. Does the contracts endpoint need authentication or a custom route outside the default `/_contracts` path?

If any answer is unknown, prefer explicit Core dispatcher registration, explicit endpoint responses, and conservative contract exposure.

## Common Tasks And Safe Responses

### Task: "Expose a command over HTTP"

Safe response:

- register the Core command dispatcher first
- map a typed `Post<T>` or `Put<T>` route
- add `afterDispatch` if the endpoint must return anything other than `200`

### Task: "Expose a read model over HTTP"

Safe response:

- map `Get<TQuery, TResult>`
- rely on the default `404` for null only if that matches the API contract
- otherwise use `afterDispatch` to shape the response explicitly

### Task: "Add CQRS dispatch inside an existing WebApi route group"

Safe response:

- keep the existing `UseEndpoints(...)` call
- use `Dispatch(...)` on the current `IEndpointsBuilder`
- avoid mixing legacy `UseDispatcherEndpoints(...)` with route-builder mapping in the same host

### Task: "Expose message contracts for another agent"

Safe response:

- use `UsePublicContracts(...)`
- choose whether attribute filtering is required
- mention that only commands and events are included

### Task: "Use one dependency instead of three dispatchers"

Safe response:

- inject `IDispatcher`
- still register the underlying command, query, and event dispatchers from `Genocs.Core`

## Failure Modes And Troubleshooting

1. Requests hit CQRS routes but DI throws for missing dispatchers.
Fix: Register `ICommandDispatcher`, `IQueryDispatcher`, and `IEventDispatcher` through `Genocs.Core` before using this package.

2. A typed query endpoint returns `404` unexpectedly.
Fix: The default typed GET behavior returns `404` when the query result is `null`. Add `afterDispatch` if the endpoint should return a different status or body.

3. A typed command endpoint returns `200` but no body.
Fix: This is the default behavior. Add `afterDispatch` and write the response explicitly if the API contract requires a payload or different status code.

4. The public contracts endpoint is empty or incomplete.
Fix: Ensure the relevant assemblies are loaded before the middleware initializes, and confirm the attribute filter matches the intended contracts.

5. The public contracts payload includes fallback keys for duplicate simple names.
Fix: This is expected behavior. Prefer unique contract type names when external tooling depends on simple-name keys.

6. Authorization metadata is present but requests still behave anonymously or fail unexpectedly.
Fix: Configure authentication and authorization in the host separately. This package only forwards endpoint authorization metadata.

7. Routing conflicts appear after adding CQRS endpoints.
Fix: Use one deliberate endpoint-mapping strategy and avoid overlapping paths between `MapDispatcherEndpoints(...)` and other endpoint registrations.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.OpenApi`
- `Genocs.Auth`
- `Genocs.Logging`
- `Genocs.Telemetry`

## One-Line Recommendation For Agents

If you only know that `Genocs.WebApi.CQRS` is installed, generate HTTP endpoints that bridge to already-registered Core dispatchers, use explicit response shaping when the default `200` or `404` behavior is not acceptable, and treat the public contracts endpoint as a static commands-and-events snapshot rather than a full runtime schema system.

