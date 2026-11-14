# Genocs.Http.RestEase Agent Reference

## Agent Operating Mode

- Assume `Genocs.Http.RestEase` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, options, builders, and exception types as the only safe API surface.
- Generate RestEase interface registration and consumption code only. Do not invent controller generation, OpenAPI generation, or general-purpose discovery behavior beyond what this package actually wires.
- If package composition is unclear, ask whether `Genocs.Core`, `Genocs.Http`, and the relevant service-discovery or load-balancing packages are already installed, because this package layers on top of them.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.Http.RestEase` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | RestEase-based typed HTTP client integration for Genocs hosts |
| Main value | `AddServiceClient<T>(...)` registration, `RestEaseOptions` configuration, fluent options builders, and a custom query serializer for flattened object and collection query parameters |
| Requires | `Genocs.Core`, `RestEase`, and usually `Genocs.Http` plus Fabio or Consul integration when using discovery or load-balancing modes |

## What This Package Is For

Use `Genocs.Http.RestEase` when you need to:

- register a RestEase interface as a DI-resolved typed client
- bind client location from the `restEase` section or from fluent in-code options
- resolve service endpoints through static host settings, Consul-backed wiring, or Fabio-backed wiring
- flatten complex query objects into RestEase query parameters using the package’s custom serializer
- keep client code interface-driven rather than hand-building request wrappers

## What This Package Does Not Do By Itself

Do not assume `Genocs.Http.RestEase` can:

- generate API interfaces for you
- validate that your RestEase attributes and routes are correct
- register multiple service clients safely through repeated `AddServiceClient<T>(...)` calls in the same host without caveats
- use the `restEase.enabled` option as a runtime gate
- provide general resiliency, retries, or serialization behavior on its own beyond what the underlying HTTP stack and RestEase already do
- infer a base URL unless the selected service entry or discovery integration provides one
- expose service discovery by itself without the supporting Fabio or Consul packages

## Safe Default Mental Model

Treat `Genocs.Http.RestEase` as four things:

1. A Genocs builder extension that registers one RestEase client interface
2. A bridge from named `HttpClientFactory` clients to `RestClient(...).For<T>()`
3. A small options model for choosing static, Consul, or Fabio resolution
4. A package with important registration constraints that must be accounted for in AI-generated code

If a user asks for many typed RestEase clients in one host, verify the registration strategy first rather than assuming repeated calls are safe.

## Fast Start Recipes

### Recipe 1: Register A Static RestEase Client From Configuration

Use this when one RestEase interface should target a fixed base URL described in configuration.

```csharp
using Genocs.Core.Builders;
using Genocs.Http.RestEase;
using RestEase;

public interface ICatalogApi
{
    [Get("/products/{id}")]
    Task<ProductDto> GetProductAsync([Path] Guid id);
}

public sealed record ProductDto(Guid Id, string Name);

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddServiceClient<ICatalogApi>("catalog");

genocs.Build();
```

Configuration:

```json
{
  "restEase": {
    "enabled": true,
    "services": [
      {
        "name": "catalog",
        "scheme": "https",
        "host": "catalog.internal",
        "port": 443
      }
    ]
  }
}
```

Effect:

- creates a named `HttpClient` whose name is `typeof(ICatalogApi).ToString()`
- looks up the `catalog` service in `restEase.services`
- sets the `HttpClient.BaseAddress`
- registers `ICatalogApi` as a transient RestEase proxy

### Recipe 2: Register A Client With Fluent Options

Use this when the host should not depend on configuration for RestEase service definition.

```csharp
using Genocs.Core.Builders;
using Genocs.Http.Configurations;
using Genocs.Http.RestEase;
using Genocs.LoadBalancing.Fabio.Configurations;
using Genocs.ServiceDiscovery.Consul.Configurations;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddServiceClient<ICatalogApi>(
        "catalog",
        options => options
            .WithLoadBalancer("fabio")
            .WithService(service => service
                .WithName("catalog")
                .WithScheme("https")
                .WithHost("catalog.internal")
                .WithPort(443)),
        consul => consul,
        fabio => fabio,
        new HttpClientOptions());

genocs.Build();
```

Use this when interface registration should be explicit in code.

### Recipe 3: Use Static Options Objects

Use this when the calling code already has fully materialized option objects.

```csharp
using Genocs.Core.Builders;
using Genocs.Http.Configurations;
using Genocs.Http.RestEase;
using Genocs.Http.RestEase.Configurations;
using Genocs.LoadBalancing.Fabio.Configurations;
using Genocs.ServiceDiscovery.Consul.Configurations;

var restEase = new RestEaseOptions
{
    LoadBalancer = "consul",
    Services =
    [
        new RestEaseOptions.Service
        {
            Name = "catalog",
            Scheme = "https",
            Host = "catalog.internal",
            Port = 443
        }
    ]
};

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddServiceClient<ICatalogApi>(
        "catalog",
        restEase,
        new ConsulOptions(),
        new FabioOptions(),
        new HttpClientOptions());

genocs.Build();
```

Use this when the host already composes option objects elsewhere.

### Recipe 4: Bind Complex Query Objects

Use this when a RestEase interface sends nested objects or collections in query parameters.

```csharp
using RestEase;

public interface IOrdersApi
{
    [Get("/orders")]
    Task<IReadOnlyList<OrderDto>> SearchAsync([Query] OrderSearch query);
}

public sealed record OrderSearch(string CustomerId, DateTime From, int[] Statuses);
public sealed record OrderDto(Guid Id, decimal Total);
```

Important behavior:

- the package’s custom query serializer flattens object properties without preserving the outer query object name
- array and enumerable values become indexed keys such as `Statuses[0]`, `Statuses[1]`
- `DateTime` values serialize using the RestEase format when provided, otherwise ISO 8601 round-trip format `o`
- null values become empty-string query values rather than being omitted in nested traversal cases

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddServiceClient<T>(builder, serviceName, sectionName, ...)` | Register a RestEase client using configuration sections | Reads `RestEaseOptions` from `restEase` by default and wires Fabio support | Assuming it can be called repeatedly for many clients without consequence |
| `AddServiceClient<T>(builder, serviceName, buildOptions, buildConsulOptions, buildFabioOptions, httpClientOptions)` | Register a client from fluent builders | Builds `RestEaseOptions` in code and wires Fabio support | Assuming only RestEase options matter and the other option builders are ignored |
| `AddServiceClient<T>(builder, serviceName, RestEaseOptions, ConsulOptions, FabioOptions, HttpClientOptions)` | Register a client from already-built option objects | Delegates to the same private registration path | Assuming this bypasses Fabio or Consul-related wiring |
| `RestEaseOptions` | Describe service resolution mode and static service entries | Uses section name `restEase`; `Enabled` exists but is not enforced by registration code | Assuming `Enabled = false` disables registration |
| `IRestEaseOptionsBuilder` | Build `RestEaseOptions` fluently | Supports `WithLoadBalancer(...)` and `WithService(...)` only | Assuming it exposes every `RestEaseOptions` property |
| `IRestEaseServiceBuilder` | Build one static service entry fluently | Supports name, scheme, host, and port | Assuming paths, headers, or auth can be configured here |
| `RestEaseServiceNotFoundException` | Detect missing static service configuration | Thrown when default static mode cannot find the named service | Assuming the package falls back to another service automatically |

## Registration Semantics

### Single Registry Key Constraint

The package uses one builder registry name: `http.restEase`.

Practical consequence:

- only the first `AddServiceClient<T>(...)` call succeeds for a given `IGenocsBuilder`
- later calls return immediately because `TryRegister("http.restEase")` fails

Treat this as a critical source-blind constraint.

Do not assume you can register multiple RestEase client interfaces by calling `AddServiceClient<T>(...)` repeatedly in the same host without verifying behavior or extending the package.

### Client Name Semantics

- the underlying named `HttpClient` name is `typeof(T).ToString()`
- the RestEase proxy is created with `new RestClient(httpClient).For<T>()`
- the interface `T` must be a RestEase-compatible interface, not a concrete class

### Load Balancer Modes

`RestEaseOptions.LoadBalancer` supports these behaviors:

- `consul`: uses `AddConsulHttpClient(clientName, serviceName)`
- `fabio`: uses `AddFabioHttpClient(clientName, serviceName)`
- anything else or null: uses the static `services` list in `RestEaseOptions`

In static mode, the package looks up a matching service by name and constructs `BaseAddress` from:

- `scheme`
- `host`
- `port`

If no matching service is found, it throws `RestEaseServiceNotFoundException`.

## Query Serialization Semantics

The package overrides RestEase query serialization through `QueryParamSerializer`.

Important behavior:

- scalar value types and strings serialize directly
- objects are flattened recursively by property name
- the serializer intentionally strips the outer parameter prefix, so a query object named `query` still emits keys like `Page`, not `query.Page`
- enumerables are indexed with bracket notation such as `Items[0]`
- null properties emit empty-string values in the produced query collection
- `DateTime` uses the supplied RestEase format or `o` when none is provided

This matters for server compatibility. Do not assume nested object query parameters keep their original parent name.

## Configuration Ownership

`Genocs.Http.RestEase` owns the `restEase` section through `RestEaseOptions`.

```json
{
  "restEase": {
    "enabled": true,
    "loadBalancer": "fabio",
    "services": [
      {
        "name": "catalog",
        "scheme": "https",
        "host": "catalog.internal",
        "port": 443
      }
    ]
  }
}
```

Fields in `RestEaseOptions`:

- `enabled`
- `loadBalancer`
- `services[].name`
- `services[].scheme`
- `services[].host`
- `services[].port`

Important behavior:

- `enabled` is present in the options model but is not checked by the registration methods in this package
- `loadBalancer` chooses static, Consul, or Fabio registration mode
- `services` matters only in the default static mode path

## Public Capability Map

### Registration

- `AddServiceClient<T>(...)`
- `RestEaseOptions`
- `IRestEaseOptionsBuilder`
- `IRestEaseServiceBuilder`

### Static Service Modeling

- `RestEaseOptions.Service`
- `WithName(...)`
- `WithScheme(...)`
- `WithHost(...)`
- `WithPort(...)`

### Error Signaling

- `RestEaseServiceNotFoundException`

### Query Behavior

- `QueryParamSerializer`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume repeated `AddServiceClient<T>(...)` calls register multiple RestEase clients safely in the same builder.
2. Do not assume `restEase.enabled` disables registration or runtime behavior in this package.
3. Do not assume `loadBalancer = consul` or `fabio` works without the supporting packages and configuration.
4. Do not assume object query parameters keep their outer parameter prefix.
5. Do not assume null query properties are omitted.
6. Do not assume this package configures auth headers, retry policies, or custom serializers for request or response bodies.
7. Do not assume a concrete class can be used as `T`; generate RestEase interfaces.
8. Do not assume static service configuration is optional when not using Consul or Fabio.

## Agent Decision Checklist

Before generating code that depends on `Genocs.Http.RestEase`, answer these questions:

1. Is the target client a RestEase interface?
2. Will the host register only one RestEase client through this package, or does the registration limitation need to be addressed?
3. Is service resolution static, Consul-based, or Fabio-based?
4. If static, is there a matching `restEase.services` entry for the chosen service name?
5. Does the downstream API expect flattened query parameters for object and collection filters?
6. Are `Genocs.Http`, Fabio, and Consul packages already part of the host composition where needed?

If any answer is unknown, prefer one static client registration with an explicit `restEase.services` entry and avoid promising multi-client behavior.

## Common Tasks And Safe Responses

### Task: "Register a typed REST client"

Safe response:

- define a RestEase interface
- call `AddServiceClient<T>(...)` once for the host
- configure the service name in `restEase.services` or select a supported discovery mode

### Task: "Use service discovery or load balancing"

Safe response:

- set `loadBalancer` to `consul` or `fabio`
- confirm the corresponding supporting packages and configuration are installed
- do not claim this package alone implements discovery

### Task: "Send nested filters in the query string"

Safe response:

- use `[Query]` on a DTO
- mention that properties flatten to simple keys and collection indexes
- verify the server expects that shape

### Task: "Register several RestEase clients"

Safe response:

- warn that the package uses a single registry key and repeated registration may be ignored
- ask whether the host needs a custom extension or a different registration strategy

## Failure Modes And Troubleshooting

1. The expected RestEase client is missing from DI.
Fix: Check whether another `AddServiceClient<T>(...)` call already consumed the package’s single registry key.

2. Startup throws a service-not-found exception.
Fix: Ensure the named service exists in `restEase.services` when not using `consul` or `fabio` mode.

3. Requests go to the wrong base address.
Fix: Re-check `scheme`, `host`, `port`, and the selected `loadBalancer` mode.

4. Nested query objects do not bind correctly on the server.
Fix: Re-check the flattened parameter shape produced by the package’s custom serializer and align the server contract.

5. `enabled = false` has no visible effect.
Fix: This package does not enforce `RestEaseOptions.Enabled` in registration logic.

6. Consul or Fabio mode fails even though the RestEase registration compiles.
Fix: Verify the supporting Genocs discovery or load-balancing packages and their configuration are present.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.Http`
- `Genocs.LoadBalancing.Fabio`
- `Genocs.ServiceDiscovery.Consul`
- `RestEase`

## One-Line Recommendation For Agents

If you only know that `Genocs.Http.RestEase` is installed, generate one RestEase interface registration through `AddServiceClient<T>(...)`, prefer explicit static service configuration, and warn before assuming multi-client registration or discovery behavior beyond the supporting Genocs packages.