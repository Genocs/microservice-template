# Genocs.WebApi.OpenApi Agent Reference

## Agent Operating Mode

- Assume `Genocs.WebApi.OpenApi` is consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, configuration types, and route behavior as the only safe API surface.
- Generate OpenAPI registration, document metadata, and docs UI setup only.
- Do not invent controllers, middleware, endpoint grouping, OAuth flows, or schema-generation features that this package does not expose.
- If package composition is unclear, ask whether `Genocs.WebApi` is already installed, because this package depends on WebApi endpoint-definition services at runtime.

## Package Identity

| Key | Value |
|---|---|
| Package | `Genocs.WebApi.OpenApi` |
| Target frameworks | `net10.0`, `net9.0`, `net8.0` |
| Primary role | Swagger/OpenAPI generation and UI hosting for Genocs Web API hosts |
| Main value | `AddOpenApiDocs(...)` for registration, `UseOpenApiDocs()` for runtime UI exposure, metadata binding through the `openapi` section, and patching of Genocs endpoint metadata into the generated document |
| Requires in practice | `Genocs.WebApi`, ASP.NET Core, and Swashbuckle dependencies included by this package |

## What This Package Is For

Use `Genocs.WebApi.OpenApi` when you need to:

- generate an OpenAPI document for a Genocs-based web host
- expose Swagger UI or ReDoc from the same ASP.NET Core app
- attach API title, version, description, server list, contact, license, and terms-of-service metadata
- add a Bearer security scheme to the generated document
- include XML comments from the entry assembly in the generated schema
- patch endpoint metadata collected by `Genocs.WebApi` into the final OpenAPI document

## What This Package Does Not Do By Itself

Do not assume `Genocs.WebApi.OpenApi` can:

- replace `Genocs.WebApi.AddWebApi()`
- register controllers or endpoint routes by itself
- secure the API or configure JWT authentication by itself
- add a complete OAuth2 or OpenID Connect UI flow to Swagger
- generate accurate request and response schemas for every Genocs endpoint style on every target framework
- make docs conditional at runtime after registration has been skipped
- expose multiple separately named OpenAPI registrations from the same `IGenocsBuilder`

## Safe Default Mental Model

Treat `Genocs.WebApi.OpenApi` as four things:

1. A registration layer over Swashbuckle through `AddOpenApiDocs(...)`
2. A runtime UI layer through `UseOpenApiDocs()`
3. A metadata model bound from the `openapi` section
4. A companion to `Genocs.WebApi`, not a standalone Web API package

If a user asks for API auth, endpoint mapping, or controllers, identify the missing companion package first.

## Fast Start Recipes

### Recipe 1: Minimal Swagger UI Setup

Use this when a Genocs web host should expose Swagger UI from configuration.

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddWebApi()
        .AddOpenApiDocs();

genocs.Build();

var app = builder.Build();
app.UseOpenApiDocs();
app.Run();
```

Configuration:

```json
{
    "openapi": {
        "enabled": true,
        "reDocEnabled": false,
        "name": "v1",
        "title": "Orders API",
        "version": "1.0.0",
        "description": "HTTP API for order management.",
        "routePrefix": "swagger",
        "includeSecurity": true
    }
}
```

Effect:

- registers Swagger generation services
- registers the `WebApiDocumentFilter` used to merge Genocs endpoint metadata into the document
- exposes the JSON document at `/swagger/v1/swagger.json`
- exposes Swagger UI at `/swagger`

### Recipe 2: ReDoc Instead Of Swagger UI

Use this when the host should serve ReDoc instead of Swagger UI.

```json
{
    "openapi": {
        "enabled": true,
        "reDocEnabled": true,
        "name": "v1",
        "title": "Orders API",
        "routePrefix": "docs"
    }
}
```

Effect:

- the JSON document is served at `/docs/v1/swagger.json`
- ReDoc is served at `/docs`

### Recipe 3: Fluent Registration In Code

Use this when the host wants to build the most common OpenAPI options in code rather than in configuration.

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
        .AddGenocs()
        .AddWebApi()
        .AddOpenApiDocs(options => options
                .Enable(true)
                .ReDocEnable(false)
                .WithName("v1")
                .WithTitle("Orders API")
                .WithVersion("1.0.0")
                .WithDescription("HTTP API for order management.")
                .WithRoutePrefix("swagger")
                .WithContactName("Platform Team")
                .IncludeSecurity(true));

genocs.Build();
```

Important limitation:

- the fluent builder now supports the full `OpenApiOptions` surface, including contact email/url, license name/url, terms of service, and server entries

### Recipe 4: Use The Root Path For Docs

Use this when the docs UI should be served from the app root instead of a nested prefix.

```json
{
    "openapi": {
        "enabled": true,
        "reDocEnabled": false,
        "name": "v1",
        "title": "Orders API",
        "routePrefix": ""
    }
}
```

Effect:

- Swagger UI is served from `/`
- the JSON document is served from `/v1/swagger.json`

## Core Entry Points

| API | Use it for | Important behavior | Common mistake |
|---|---|---|---|
| `AddOpenApiDocs()` | Register OpenAPI support from the default `openapi` section | Reads `OpenApiOptions.Position`, which is `openapi` | Assuming it works without `Genocs.WebApi` |
| `AddOpenApiDocs(string sectionName)` | Register OpenAPI support from a custom section | Returns without registration if the section does not bind to settings | Calling `UseOpenApiDocs()` after registration was skipped |
| `AddOpenApiDocs(Func<IOpenApiOptionsBuilder, IOpenApiOptionsBuilder>)` | Register OpenAPI options fluently in code | Supports the full `OpenApiOptions` surface, including servers and advanced metadata fields | Assuming server metadata must be configured only through raw `OpenApiOptions` |
| `AddOpenApiDocs(OpenApiOptions settings)` | Register OpenAPI support from a fully materialized options object | No-ops when `settings.Enabled` is `false` or the registry key is already used | Expecting disabled registration to still provide services for `UseOpenApiDocs()` |
| `UseOpenApiDocs()` | Expose the Swagger JSON document and either Swagger UI or ReDoc | Requires `OpenApiOptions` to have been registered successfully | Calling it when `AddOpenApiDocs(...)` was skipped, disabled, or missing |
| `IOpenApiOptionsBuilder` | Build OpenAPI settings fluently | Supports complete option coverage, including contact, license, terms, and servers | Forgetting to use `WithServers(...)` or `AddServer(...)` when server metadata is needed |

## Route And UI Semantics

### JSON Document Route

The package uses this template for the generated document:

- `/{routePrefix}/{documentName}/swagger.json`

Examples:

- `routePrefix = "swagger"`, `name = "v1"` -> `/swagger/v1/swagger.json`
- `routePrefix = "docs"`, `name = "orders"` -> `/docs/orders/swagger.json`
- `routePrefix = ""`, `name = "v1"` -> `/v1/swagger.json`

That template is the JSON route on the service. It does not change when `documentPath` changes. `documentPath` only changes the spec URL written into Swagger UI and ReDoc.

### Spec URL (`documentPath`)

Config key: `openapi.documentPath`. Default: `absolute`.

| `routePrefix` | `name` | `absolute` (default) | `relative` |
|---|---|---|---|
| `swagger` | `Notifications` | `/swagger/Notifications/swagger.json` | `Notifications/swagger.json` |
| `docs` | `v1` | `/docs/v1/swagger.json` | `v1/swagger.json` |
| empty | `v1` | `/v1/swagger.json` | `v1/swagger.json` |

`relative` is what makes Swagger UI work behind a path-stripping reverse proxy. Swashbuckle resolves a spec URL that does not start with `/` or `http` against the current page, so a UI opened at `.../notifications-service/swagger/index.html` loads `.../notifications-service/swagger/{name}/swagger.json`. The same relative value still works when the UI is opened directly at `/swagger/index.html`.

Unknown values fail at startup. The message lists `absolute` and `relative`.

If the UI still requests `/swagger/{name}/swagger.json` from the site root, `documentPath` is not `relative`.

### Forwarded prefix (`useForwardedPrefix`)

Config key: `openapi.useForwardedPrefix`. Default: `false`.

When `true`, a valid `X-Forwarded-Prefix` header replaces the OpenAPI `servers` list for that request with one entry whose `url` is the prefix (for example `/notifications-service`). A valid prefix starts with `/`, has no scheme, no `//`, no `?`, no `#`, and no trailing slash except the value `/`. When the header is missing or invalid, `servers` stay at the configured `openapi.servers` value. Those configured servers still control **Try it out** when the header is absent.

When `false`, the header is ignored.

This is opt-in because a client-supplied prefix would otherwise change the server URL. The trusted proxy must overwrite the header.

### UI Route

When `reDocEnabled` is `false`:

- Swagger UI is served from `/{routePrefix}`

When `reDocEnabled` is `true`:

- ReDoc is served from `/{routePrefix}`

When `routePrefix` is empty:

- the UI is served from `/`

### Static Files

`UseOpenApiDocs()` calls `UseStaticFiles()` internally.

Safe implication:

- you do not need a separate static-files call just for Swagger UI or ReDoc assets

Unsafe implication:

- do not assume this replaces other static-file setup your app may still need

## Document Generation Semantics

`AddOpenApiDocs(...)` configures Swashbuckle with:

- `AddEndpointsApiExplorer()`
- `AddSwaggerGen(...)`
- `EnableAnnotations()`
- `DocumentFilter<WebApiDocumentFilter>()`

### Relationship With Genocs.WebApi

This package depends on `WebApiEndpointDefinitions`, which is registered by `Genocs.WebApi`.

Practical consequence:

- if the host uses this package, also compose `AddWebApi()` unless you have intentionally registered equivalent services yourself
- `Genocs.WebApi` endpoint definitions are used to patch request and response metadata into the document

### Query Representation Policy

OpenApi query metadata follows an explicit policy:

- entries marked as `In = "query"` are emitted as OpenAPI query parameters
- request bodies are reserved for entries marked as `In = "body"`
- this applies even when the query contract type implements `IQuery`

Migration impact:

- consumers who previously relied on query contracts being emitted as `application/json` request bodies should update clients and examples to send query-string parameters instead

## Maintainer Validation Guidance

Use the package-level quality gate to validate OpenApi changes:

- `dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -f net10.0 -c Debug --nologo -warnaserror -p:BuildProjectReferences=false`
- `dotnet test src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj -c Debug --nologo`
- `make validate-webapi-openapi`

### XML Comments

The package always points Swashbuckle to:

- `{AppContext.BaseDirectory}/{EntryAssemblyName}.xml`

Safe guidance:

- enable XML documentation file generation in the host if you want comment-based enrichment
- keep the XML file in the published output when using this package

### Custom Operation IDs

The package customizes operation IDs only for controller action descriptors.

Behavior:

- group name `v1` -> operation ID is the action name
- any other group name -> operation ID is `_{ActionName}`
- non-controller actions fall back to default Swashbuckle behavior

This exists to support downstream tooling that depends on stable operation IDs.

## Security Metadata Semantics

When `includeSecurity` is `true`, the package adds a `Bearer` security definition.

Important version-sensitive behavior:

- on `net8.0` and `net9.0`, the package also adds a Swagger security requirement that references the Bearer scheme
- on `net10.0`, the security definition is added, but the security requirement block is currently commented out in the implementation

Safe guidance:

- treat `includeSecurity` as documentation metadata only
- do not assume it configures authentication for the app
- if the target is `net10.0`, do not assume the generated document will automatically mark operations as requiring Bearer auth

## Schema And Filter Limitations

### Genocs Endpoint Filter

`WebApiDocumentFilter` builds request and response entries from `WebApiEndpointDefinitions`.

Important behavior:

- request parameters marked as `body` become JSON request bodies
- request parameters marked as `query` usually become OpenAPI parameters
- if a query parameter type implements `IQuery`, the filter turns it into a JSON request body instead of query parameters

### Framework-Specific Schema Shape

On `net8.0` and `net9.0`:

- schemas created by the filter use the CLR type name and a serialized example string

On `net10.0`:

- the filter currently emits a string schema placeholder for both parameters and responses
- example payload wiring is not implemented there yet

Safe guidance:

- do not assume filter-generated schemas are precise for every endpoint on `net10.0`

## Configuration Ownership

`Genocs.WebApi.OpenApi` owns the `openapi` section through `OpenApiOptions`.

```json
{
    "openapi": {
        "enabled": true,
        "reDocEnabled": false,
        "name": "v1",
        "title": "Orders API",
        "version": "1.0.0",
        "description": "HTTP API for order management.",
        "routePrefix": "swagger",
        "documentPath": "absolute",
        "useForwardedPrefix": false,
        "contactName": "Platform Team",
        "contactEmail": "platform@example.com",
        "contactUrl": "https://example.com/platform",
        "licenseName": "MIT",
        "licenseUrl": "https://opensource.org/license/mit/",
        "termsOfService": "https://example.com/terms",
        "includeSecurity": true,
        "servers": [
            {
                "url": "https://api.example.com/orders",
                "description": "Production"
            }
        ]
    }
}
```

What the package actively uses:

- `enabled`
- `reDocEnabled`
- `name`
- `title`
- `version`
- `description`
- `routePrefix`
- `documentPath` (`absolute` by default, or `relative`)
- `useForwardedPrefix` (`false` by default)
- `contactName`
- `contactEmail`
- `contactUrl`
- `licenseName`
- `licenseUrl`
- `termsOfService`
- `includeSecurity`
- `servers[].url`
- `servers[].description`

Defaults applied by code when URLs are omitted:

- `termsOfService` -> `https://www.genocs.com/terms_and_conditions.html`
- `contactUrl` -> `https://www.genocs.com`
- `licenseUrl` -> `https://opensource.org/license/mit/`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not assume `UseOpenApiDocs()` is safe when registration was skipped. It requires `OpenApiOptions` in DI.
2. Do not assume `enabled = false` means `UseOpenApiDocs()` becomes a harmless no-op. In this package, disabled registration means the required service may never be added.
3. Do not assume `Genocs.WebApi.OpenApi` is standalone. It expects `Genocs.WebApi` endpoint-definition services.
4. Do not assume the fluent builder covers every option in `OpenApiOptions`.
5. Do not assume `includeSecurity` configures runtime authentication.
6. Do not assume `includeSecurity` produces the same OpenAPI security output on every target framework.
7. Do not assume filter-generated schemas on `net10.0` are detailed or example-rich.
8. Do not assume the package exposes multiple independently registered Swagger documents in one builder instance. Duplicate registration is blocked by the `docs.openapi` registry key.
9. Do not assume XML comments are optional when the host depends on comment-enriched docs. The package always points Swashbuckle at the entry assembly XML file path.
10. Do not assume the Swagger UI or ReDoc spec URL is always a host-root path. `documentPath` defaults to `absolute` (`/swagger/{name}/swagger.json`). Set `documentPath` to `relative` when the UI is served behind a path-stripping reverse proxy.
11. Do not assume `X-Forwarded-Prefix` changes **Try it out**. That happens only when `useForwardedPrefix` is `true` and the header is a single safe path prefix. Otherwise `openapi.servers` still control **Try it out**.

## Agent Decision Checklist

Before generating code that depends on `Genocs.WebApi.OpenApi`, answer these questions:

1. Is `Genocs.WebApi` already installed and composed in the host?
2. Should the UI be Swagger UI or ReDoc?
3. Should docs live under `/swagger`, `/docs`, or the app root?
4. Does the host actually want OpenAPI enabled in this environment?
5. Will the app call `UseOpenApiDocs()` only when registration succeeded?
6. Does the host need full metadata fields such as license, terms, servers, and contact URLs, meaning configuration should be preferred over the fluent builder?
7. Is the target framework `net10.0`, where security requirement and filter-schema behavior differ from `net8.0` and `net9.0`?
8. Does the host want XML comments in the generated document, and is the XML file included in output?

If any answer is unknown, prefer configuration-based setup, keep the route prefix explicit, and do not call `UseOpenApiDocs()` unless registration is confirmed.

## Common Tasks And Safe Responses

### Task: "Add Swagger to a Genocs service"

Safe response:

- add `.AddWebApi()`
- add `.AddOpenApiDocs()`
- call `.UseOpenApiDocs()` after building the app

### Task: "Serve ReDoc instead of Swagger UI"

Safe response:

- set `openapi.reDocEnabled = true`
- keep `UseOpenApiDocs()` as the pipeline call

### Task: "Document Bearer authentication"

Safe response:

- set `openapi.includeSecurity = true`
- note that this affects document metadata only
- add real auth separately through `Genocs.Auth` or another auth package

### Task: "Expose docs from the root path"

Safe response:

- set `routePrefix` to an empty string
- expect the JSON route to become `/{name}/swagger.json`

### Task: "Configure docs fully in code"

Safe response:

- use `AddOpenApiDocs(new OpenApiOptions { ... })` when the host needs fields not exposed by `IOpenApiOptionsBuilder`

## Failure Modes And Troubleshooting

1. The app throws when `UseOpenApiDocs()` runs.
Fix: Ensure `AddOpenApiDocs(...)` actually registered services. If docs were disabled, the section was missing, or duplicate registration was ignored, `OpenApiOptions` may not exist in DI.

2. Swagger UI or ReDoc is missing.
Fix: Confirm `AddOpenApiDocs(...)` ran successfully and that `UseOpenApiDocs()` is in the pipeline.

3. The JSON document path is not where expected.
Fix: Re-check `routePrefix` and `name`. The document route is `/{routePrefix}/{name}/swagger.json`, with empty-prefix normalization to `/{name}/swagger.json`. If the UI still requests `/swagger/{name}/swagger.json` from the site root, `documentPath` is not `relative`.

3a. **Try it out** leaves the gateway prefix.
Fix: Set `useForwardedPrefix` to `true` and have the trusted proxy set `X-Forwarded-Prefix` to a single path prefix such as `/notifications-service`. When that header is absent or invalid, `openapi.servers` still control **Try it out**.

4. Security controls do not appear as expected.
Fix: Confirm `includeSecurity = true`. On `net10.0`, remember that the Bearer definition is added but the security requirement block is not currently wired.

5. XML-comment-enriched docs fail or lack descriptions.
Fix: Generate the host XML documentation file and keep it in the output directory under the entry assembly name.

6. Some Genocs endpoint schemas look too generic.
Fix: Re-check the target framework. On `net10.0`, the custom filter currently emits string placeholder schemas for its patched endpoint entries.

7. A second OpenAPI registration appears ignored.
Fix: The package uses a single registry key, `docs.openapi`, so only the first successful registration is applied.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.Auth`
- `Genocs.WebApi.CQRS`
- `Genocs.Logging`
- `Genocs.Telemetry`

## One-Line Recommendation For Agents

If you only know that `Genocs.WebApi.OpenApi` is installed, compose it with `Genocs.WebApi`, register it once through `AddOpenApiDocs(...)`, prefer configuration when full metadata is needed, and call `UseOpenApiDocs()` only when registration definitely succeeded.





