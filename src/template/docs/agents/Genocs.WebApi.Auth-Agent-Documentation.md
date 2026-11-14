# Genocs.WebApi.Auth Agent Reference

## Repo Reality

There is no package or project named `Genocs.WebApi.Auth` in this repository.

The effective Web API authentication surface is split across two actual NuGet packages:

- `Genocs.Auth` for JWT-based authentication, token creation, and access-token revocation checks
- `Genocs.WebApi.Security` for inbound client-certificate authentication in ASP.NET Core Web APIs

This document treats those two packages as the practical equivalent of a Web API auth module and explains every supported provider path from a NuGet consumer perspective.

## Agent Operating Mode

- Assume the packages are consumed from NuGet only.
- Do not assume repository or source-code visibility.
- Treat documented extension methods, option types, middleware, and interfaces as the only safe API surface.
- Generate ASP.NET Core authentication wiring only for the providers documented here.
- Do not invent provider-specific helpers for Auth0, Azure AD, Keycloak, Firebase, or API keys beyond what the packages actually expose.
- If package composition is unclear, ask whether `Genocs.Core`, `Genocs.WebApi`, and host authentication or authorization middleware are already in use.

## Package Map

| Package | Role | Main entry points | Use it for |
|---|---|---|---|
| `Genocs.Auth` | JWT authentication and token services | `AddJwt()`, `AddOpenIdJwt()`, `AddPrivateKeyJwt()`, `UseAccessTokenValidator()` | bearer-token validation, token creation, token payload parsing, in-memory token revocation checks |
| `Genocs.WebApi.Security` | Client-certificate authentication for ASP.NET Core Web APIs | `AddCertificateAuthentication()`, `UseCertificateAuthentication()` | mutual TLS or forwarded-certificate validation, ACL-based certificate authorization |

## Provider Matrix

| Provider path | Package | Registration API | Best fit | Key configuration |
|---|---|---|---|---|
| Symmetric JWT | `Genocs.Auth` | `AddJwt()` | first-party services that share a secret | `jwt.issuerSigningKey`, issuer or audience validation settings |
| X.509 JWT | `Genocs.Auth` | `AddJwt()` with `jwt.certificate.*` | JWT validation with certificate material, and token issuance only when the certificate contains a private key | `jwt.certificate.location` or `jwt.certificate.rawData` |
| OIDC discovery JWT | `Genocs.Auth` | `AddOpenIdJwt()` | external identity providers that expose `/.well-known/openid-configuration` | `jwt.issuer`, `jwt.metadataAddress`, `jwt.audience` |
| RSA XML key JWT | `Genocs.Auth` | `AddPrivateKeyJwt()` | JWT validation with RSA XML key material | `jwt.issuerSigningKey` as `<RSAKeyValue>...</RSAKeyValue>` |
| Client certificate | `Genocs.WebApi.Security` | `AddCertificateAuthentication()` | inbound service-to-service auth via client certificates or forwarded cert headers | `security.certificate.*` |
| JWT or API key hybrid | `Genocs.Auth` | manual middleware plus attribute usage only | advanced opt-in for endpoints that must accept bearer token or `x-gnx-apikey` | `Authorization:Enabled`, `Authorization:ApiKeys`, `Authorization:DevApiKey` |

## What This Auth Surface Is For

Use these packages when you need to:

- protect ASP.NET Core Web API endpoints with bearer-token validation
- issue JWTs from application code through `IJwtHandler`
- validate JWTs against symmetric keys, X.509 certificates, OIDC discovery documents, or RSA XML key material
- revoke already-issued access tokens through an `IAccessTokenService`
- protect inbound service endpoints with client certificates
- apply certificate ACL rules by subject, issuer, thumbprint, serial number, and permissions

## What It Does Not Do By Itself

Do not assume this auth surface can:

- provide dedicated first-class registration methods for Auth0, Azure AD, Keycloak, Okta, or Firebase
- configure OAuth authorization-code flows or interactive web sign-in
- persist revoked tokens across multiple instances unless you replace the default in-memory implementation
- generate API keys or register API-key auth through a polished builder extension
- configure reverse-proxy trust automatically for certificate forwarding
- secure endpoints unless the host also calls `UseAuthentication()` and `UseAuthorization()` where appropriate
- expose OpenAPI security metadata by itself

## Safe Default Mental Model

Treat the auth surface as three layers:

1. `Genocs.Auth` handles bearer-token validation and optional token tooling
2. `Genocs.WebApi.Security` handles client-certificate validation for inbound requests
3. The ASP.NET Core host still owns pipeline order, endpoint authorization, and reverse-proxy trust

If a user asks for a named cloud identity provider, map it to the closest supported provider type rather than inventing a provider-specific package API.

## Choose The Right Provider

| Situation | Prefer | Why |
|---|---|---|
| Your services issue and validate their own shared-secret JWTs | `AddJwt()` with `issuerSigningKey` | simplest built-in path |
| Your services validate or issue JWTs using X.509 material | `AddJwt()` with `certificate.*` | same API, certificate-backed key |
| Your tokens come from an OIDC-compliant external identity provider | `AddOpenIdJwt()` | provider keys and metadata come from discovery |
| You receive JWTs signed with RSA XML key material | `AddPrivateKeyJwt()` | explicit RSA-key validation path |
| Your Web API requires client certificates | `AddCertificateAuthentication()` and `UseCertificateAuthentication()` | dedicated certificate flow |
| One endpoint must accept either JWT or API key | manual `JwtOrApiKeyAuthenticationMiddleware` plus `ApiKeyOrJwtAuthorizeAttribute` | advanced helper exists, but it is not a primary registration path |

## Fast Start Recipes

### Recipe 1: Symmetric JWT For First-Party Services

Use this when one service issues and validates its own bearer tokens with a shared secret.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddJwt();

genocs.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseAccessTokenValidator();
app.Run();
```

Minimum configuration:

```json
{
  "jwt": {
    "enabled": true,
    "issuer": "https://identity.internal",
    "issuerSigningKey": "super-secret-key",
    "audience": "orders-api",
    "validIssuer": "https://identity.internal",
    "validAudience": "orders-api",
    "validateIssuer": true,
    "validateAudience": true,
    "validateLifetime": true
  }
}
```

Effect:

- registers JWT bearer authentication
- registers `IJwtHandler` and `IAccessTokenService`
- enables token blacklisting checks through `UseAccessTokenValidator()`
- adds authorization services automatically

### Recipe 2: JWT With X.509 Certificate

Use this when token validation should rely on certificate material instead of a plain shared secret.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddJwt();

genocs.Build();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Configuration from file path:

```json
{
  "jwt": {
    "enabled": true,
    "issuer": "https://identity.internal",
    "validIssuer": "https://identity.internal",
    "validAudience": "orders-api",
    "validateIssuer": true,
    "validateAudience": true,
    "validateLifetime": true,
    "certificate": {
      "location": "certs/signing.pfx",
      "password": "change-me"
    }
  }
}
```

Configuration from inline base64 raw data:

```json
{
  "jwt": {
    "enabled": true,
    "validIssuer": "https://identity.internal",
    "validAudience": "orders-api",
    "certificate": {
      "rawData": "BASE64_CERTIFICATE_BYTES",
      "password": "change-me"
    }
  }
}
```

Important behavior:

- if the certificate contains a private key, `IJwtHandler` can issue tokens with it
- if the certificate contains only a public key, validation is the safe assumption and token issuance may fail
- when no algorithm is supplied, the package switches to `RS256` behavior automatically for certificate-backed keys

### Recipe 3: OIDC Provider Authentication

Use this when an external provider exposes an OpenID Connect discovery document.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddOpenIdJwt();

genocs.Build();

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Generic OIDC configuration:

```json
{
  "jwt": {
    "challenge": "Bearer",
    "issuer": "https://issuer.example.com",
    "metadataAddress": "/.well-known/openid-configuration",
    "audience": "orders-api",
    "includeErrorDetails": true,
    "refreshOnIssuerKeyNotFound": true
  }
}
```

How named providers map to this path:

- Auth0: set `issuer` to your tenant base URL
- Azure AD or Entra ID: set `issuer` to the tenant authority URL
- Keycloak: set `issuer` to the realm base URL
- Firebase or any other OIDC-compatible provider: use the provider issuer and discovery path it exposes

Important behavior:

- the package concatenates `jwt.issuer` and `jwt.metadataAddress`
- this path validates tokens from discovered keys rather than local signing material
- unlike `AddJwt()`, this method does not register `IJwtHandler`, `IAccessTokenService`, or authorization services

### Recipe 4: RSA XML Key JWT Validation

Use this when the signing key is provided as RSA XML key material.

```csharp
using Genocs.Auth;
using Genocs.Core.Builders;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddPrivateKeyJwt();

genocs.Build();

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
```

Configuration:

```json
{
  "jwt": {
    "issuerSigningKey": "<RSAKeyValue><Modulus>...</Modulus><Exponent>...</Exponent></RSAKeyValue>",
    "validIssuer": "https://identity.internal",
    "validAudience": "orders-api",
    "validateIssuer": true,
    "validateAudience": true,
    "validateLifetime": true,
    "validateIssuerSigningKey": true
  }
}
```

Important behavior:

- the expected key format is XML under `<RSAKeyValue>`
- this path is validation-oriented; it does not register `IJwtHandler` or token revocation services
- missing `issuerSigningKey` throws at startup

### Recipe 5: Client-Certificate Authentication For Web APIs

Use this when callers authenticate with client certificates instead of bearer tokens.

```csharp
using Genocs.Core.Builders;
using Genocs.WebApi.Security;

var builder = WebApplication.CreateBuilder(args);

IGenocsBuilder genocs = builder
    .AddGenocs()
    .AddCertificateAuthentication();

genocs.Build();

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseCertificateAuthentication();
app.UseAuthorization();
app.Run();
```

Configuration:

```json
{
  "security": {
    "certificate": {
      "enabled": true,
      "header": "Certificate",
      "allowSubdomains": true,
      "allowedDomains": ["internal.example.com"],
      "allowedHosts": ["localhost", "127.0.0.1"],
      "skipRevocationCheck": false,
      "acl": {
        "orders-api": {
          "validIssuer": "CN=corp-root-ca",
          "validThumbprint": "ABC123...",
          "validSerialNumber": "00112233",
          "permissions": ["orders.read", "orders.write"]
        }
      }
    }
  }
}
```

Effect:

- registers ASP.NET Core certificate authentication and certificate forwarding
- reads the forwarded certificate from the configured header, defaulting to `Certificate`
- validates the certificate chain unless `skipRevocationCheck` is enabled
- optionally enforces ACL rules by subject, issuer, thumbprint, serial number, and permissions

### Recipe 6: Manual JWT-Or-API-Key Hybrid

Use this only when an endpoint must accept either bearer token auth or an `x-gnx-apikey` header.

```csharp
using Genocs.Auth;
using Genocs.Auth.Attributes;

var app = builder.Build();
app.UseMiddleware<JwtOrApiKeyAuthenticationMiddleware>();

[ApiKeyOrJwtAuthorize]
public sealed class SystemController : ControllerBase
{
}
```

Configuration:

```json
{
  "Authorization": {
    "Enabled": true,
    "ApiKeys": ["prod-key-1", "prod-key-2"],
    "DevApiKey": "local-dev-key"
  }
}
```

Important behavior:

- this is a public helper, but not a first-class builder-based provider path
- it relies on manual middleware registration
- it sets claims itself when the API key is valid
- it is safest to treat this as an advanced compatibility mode, not the default auth architecture

## Core Entry Points

| API | Package | Use it for | Important behavior | Common mistake |
|---|---|---|---|---|
| `AddJwt()` | `Genocs.Auth` | Register JWT bearer auth with local validation settings | Registers auth, authorization, `IJwtHandler`, `IAccessTokenService`, and revocation middleware services | Assuming it always uses a symmetric key when a certificate is configured |
| `AddOpenIdJwt()` | `Genocs.Auth` | Validate JWTs from an OIDC provider | Uses discovery metadata from `issuer + metadataAddress` | Assuming it registers token issuance or authorization services |
| `AddPrivateKeyJwt()` | `Genocs.Auth` | Validate JWTs with RSA XML key material | Requires `issuerSigningKey` in RSA XML format | Assuming PEM or JWK formats work here |
| `UseAccessTokenValidator()` | `Genocs.Auth` | Reject blacklisted tokens after authentication | Checks `IAccessTokenService` unless the request path is anonymous | Assuming revocation is distributed across instances by default |
| `IJwtHandler` | `Genocs.Auth` | Create and inspect JWTs in application code | Uses the registered signing key and validation parameters | Assuming it exists for every provider path |
| `IAccessTokenService` | `Genocs.Auth` | Blacklist and test token activity | Default implementation is in-memory only | Assuming revocation survives process restarts or scales out automatically |
| `AddCertificateAuthentication()` | `Genocs.WebApi.Security` | Register client-certificate auth and forwarding | Adds certificate auth, forwarding, middleware, and permission validation | Forgetting to call `UseCertificateAuthentication()` |
| `UseCertificateAuthentication()` | `Genocs.WebApi.Security` | Validate inbound certificates in the request pipeline | Uses forwarded cert header plus ACL validation | Assuming it alone handles all ASP.NET Core authentication middleware needs |
| `ICertificatePermissionValidator` | `Genocs.WebApi.Security` | Enforce permission-based certificate access | Default implementation always returns `true` | Assuming ACL permissions are enforced without a custom validator |
| `JwtOrApiKeyAuthenticationMiddleware` | `Genocs.Auth` | Support manual bearer-or-api-key auth | Reads `x-gnx-apikey` or `Authorization` header | Treating it as a polished package registration feature |

## Provider-Specific Guidance

### Symmetric JWT

Safe assumptions:

- use `AddJwt()`
- store the secret in `jwt.issuerSigningKey`
- use this when the service or an internal identity component owns the key

Do not assume:

- that this is appropriate for third-party identity providers
- that rotating the secret is coordinated automatically across all services

### X.509 JWT

Safe assumptions:

- use `AddJwt()` with `jwt.certificate.location` or `jwt.certificate.rawData`
- use a certificate with private key when issuing tokens through `IJwtHandler`
- use public-key-only material for validation-only scenarios

Do not assume:

- that public-key-only certificates can sign tokens successfully
- that certificate loading is hot-reloaded automatically

### OIDC Providers

Safe assumptions:

- use `AddOpenIdJwt()` for any provider with an OpenID discovery endpoint
- map provider-specific setup to `issuer`, `metadataAddress`, and `audience`

Do not assume:

- that provider-specific claims are normalized automatically
- that authorization services are registered for you

### RSA XML Key Path

Safe assumptions:

- the expected key format is XML RSA key material
- this path is for validation wiring, not full token tooling

Do not assume:

- PEM, DER, or JWK inputs are supported directly
- `IJwtHandler` is registered for this path

### Client Certificate Path

Safe assumptions:

- the cert can come from ASP.NET Core connection state or forwarded header handling
- ACL checks can constrain subject, issuer, thumbprint, serial number, and permissions

Do not assume:

- that permission checks do anything useful with the default validator
- that `allowedHosts` is a security feature for remote callers; it bypasses certificate checks for matching hosts or forwarded-for values

## Configuration Ownership

### `Genocs.Auth`

Owns the `jwt` section.

Important fields the package actively uses:

- `enabled`
- `allowAnonymousEndpoints`
- `certificate.location`
- `certificate.rawData`
- `certificate.password`
- `algorithm`
- `issuer`
- `issuerSigningKey`
- `authority`
- `audience`
- `challenge`
- `metadataAddress`
- `saveToken`
- `saveSigninToken`
- `requireAudience`
- `requireHttpsMetadata`
- `requireExpirationTime`
- `requireSignedTokens`
- `expiryMinutes`
- `expiry`
- `validAudience`
- `validAudiences`
- `validIssuer`
- `validIssuers`
- `validateActor`
- `validateAudience`
- `validateIssuer`
- `validateLifetime`
- `validateTokenReplay`
- `validateIssuerSigningKey`
- `refreshOnIssuerKeyNotFound`
- `includeErrorDetails`
- `authenticationType`
- `nameClaimType`
- `roleClaimType`

### `Genocs.WebApi.Security`

Owns the `security.certificate` section.

Important fields the package actively uses:

- `enabled`
- `header`
- `allowSubdomains`
- `allowedDomains`
- `allowedHosts`
- `acl`
- `skipRevocationCheck`

## Public Capability Map

### JWT Registration And Token Services

- `AddJwt()`
- `AddOpenIdJwt()`
- `AddPrivateKeyJwt()`
- `UseAccessTokenValidator()`
- `IJwtHandler`
- `IAccessTokenService`
- `JsonWebToken`
- `JsonWebTokenPayload`
- `JwtOptions`

### Certificate Authentication

- `AddCertificateAuthentication()`
- `UseCertificateAuthentication()`
- `ICertificatePermissionValidator`
- `SecurityOptions`

### Advanced Manual Helpers

- `JwtAuthAttribute`
- `JwtOrApiKeyAuthenticationMiddleware`
- `ApiKeyOrJwtAuthorizeAttribute`

## Source-Blind Guardrails For Agents

When you cannot inspect source code, follow these rules:

1. Do not refer to a nonexistent `Genocs.WebApi.Auth` package as if it were published.
2. Do not assume every named provider has its own extension method; most external providers map to `AddOpenIdJwt()`.
3. Do not assume `AddOpenIdJwt()` or `AddPrivateKeyJwt()` register `IJwtHandler`, `IAccessTokenService`, or authorization services.
4. Do not assume revoked-token checks work across multiple app instances with the default implementation.
5. Do not assume a certificate without a private key can issue tokens successfully.
6. Do not assume `ICertificatePermissionValidator` enforces permissions unless a custom implementation is registered.
7. Do not assume `allowedHosts` tightens security; it bypasses cert validation for matching hosts.
8. Do not assume PEM or JWK key formats are supported by `AddPrivateKeyJwt()`.
9. Do not assume the manual JWT-or-API-key helper is a first-class provider registration path.
10. Do not assume certificate forwarding is safe without trusted proxy configuration.

## Agent Decision Checklist

Before generating code that depends on this auth surface, answer these questions:

1. Is the app consuming `Genocs.Auth`, `Genocs.WebApi.Security`, or both?
2. Is the issuer local, external-OIDC, certificate-backed, or RSA-key-backed?
3. Does the app need to issue tokens or only validate them?
4. Is token revocation required across multiple instances?
5. Does the host already call `UseAuthentication()` and `UseAuthorization()`?
6. Is a reverse proxy forwarding certificates or bearer tokens?
7. Are permission-based certificate checks required, and if so, who implements `ICertificatePermissionValidator`?
8. Are provider-specific claims mapped correctly for the app’s authorization rules?

If any answer is unknown, prefer the simplest matching provider path and avoid claiming features that require custom infrastructure.

## Common Tasks And Safe Responses

### Task: "Set up JWT auth for an internal service"

Safe response:

- use `AddJwt()`
- configure `issuerSigningKey`, issuer, and audience values
- add `UseAuthentication()` and `UseAuthorization()`

### Task: "Integrate Auth0, Azure AD, Keycloak, or Firebase"

Safe response:

- use `AddOpenIdJwt()`
- configure `issuer`, `metadataAddress`, and `audience`
- mention that provider-specific claim mapping may still need host-level authorization work

### Task: "Use a certificate for JWT validation"

Safe response:

- use `AddJwt()` with `jwt.certificate.*`
- mention that token issuance needs a private key

### Task: "Use mutual TLS for service-to-service calls"

Safe response:

- use `AddCertificateAuthentication()` and `UseCertificateAuthentication()`
- configure ACL values and a custom `ICertificatePermissionValidator` if permissions matter

### Task: "Blacklist tokens immediately"

Safe response:

- use `UseAccessTokenValidator()`
- mention that the built-in implementation is process-local in memory

### Task: "Accept either API key or JWT"

Safe response:

- mention the manual middleware and attribute path
- state clearly that this is an advanced helper, not a builder-registered provider mode

## Failure Modes And Troubleshooting

1. JWT-authenticated requests always return `401`.
Fix: Re-check issuer, audience, key material, and middleware order. Ensure `UseAuthentication()` runs before authorization and endpoint execution.

2. OIDC authentication fails to discover signing keys.
Fix: Verify that `jwt.issuer + jwt.metadataAddress` resolves to the correct OpenID configuration URL.

3. `IJwtHandler` is missing from DI.
Fix: Use `AddJwt()`. The OIDC and RSA XML registration paths do not register token tooling.

4. Revoked tokens still work on another instance.
Fix: Replace the default in-memory `IAccessTokenService` with a distributed implementation.

5. Certificate-authenticated requests return `401`.
Fix: Confirm certificate forwarding is configured correctly, the certificate chain builds successfully, and `UseCertificateAuthentication()` is in the pipeline.

6. Certificate-authenticated requests return `403`.
Fix: Re-check ACL subject, issuer, thumbprint, serial number, and permission validator behavior.

7. A certificate permission list exists but every allowed certificate still passes.
Fix: Register a custom `ICertificatePermissionValidator`. The default implementation always returns `true`.

8. RSA-key startup fails.
Fix: Ensure `jwt.issuerSigningKey` is present and formatted as XML RSA key material.

9. API-key hybrid auth behaves inconsistently.
Fix: Verify manual middleware registration, `Authorization` config keys, and endpoint use of `ApiKeyOrJwtAuthorizeAttribute`.

## Related Packages To Ask About

- `Genocs.Core`
- `Genocs.WebApi`
- `Genocs.WebApi.OpenApi`
- `Genocs.WebApi.CQRS`
- `Genocs.Logging`
- `Genocs.Telemetry`

## One-Line Recommendation For Agents

If the user asks for `Genocs.WebApi.Auth`, treat it as the combination of `Genocs.Auth` for JWT-based providers and `Genocs.WebApi.Security` for client-certificate auth, choose the provider path that matches the actual key or identity source, and avoid claiming cloud-provider-specific helpers that the packages do not expose.