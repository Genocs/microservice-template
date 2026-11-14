# Genocs Library Upgrade Agent Reference

## Agent Operating Mode

- Use this document when upgrading a host that consumes Genocs NuGet packages to the repository registration, domain-event, audit, dependency-marker, and logging contracts added for template hosts.
- Read the package agent reference as well when the change touches that package: [Genocs.Persistence.EFCore-Agent-Documentation.md](Genocs.Persistence.EFCore-Agent-Documentation.md) and [Genocs.Logging-Agent-Documentation.md](Genocs.Logging-Agent-Documentation.md).
- Assume the host consumes packages from NuGet. Change the host. Do not copy library source into the service.
- Keep one marker model: `ITransientDependency`. Do not add `ITransientService` to new or migrated service interfaces.
- Leave HTTP user-scope logging in the host until this library grows that capability. This upgrade does not replace that middleware.

## Purpose

This reference is for an AI agent performing the host upgrade and for later sessions that need the capability that upgrade depends on. It states what the library now does, what the host must delete, and what must stay.

## Packages in this upgrade

| Package | Host change |
|---|---|
| `Genocs.Persistence.EFCore` | Call `AddGenocsRepositories<TContext>` for a caller `DbContext`. Delete the local aggregate registration loop. |
| `Genocs.Core` | Use `AggregateRoot` or `AggregateRoot<TPrimaryKey>` for aggregates that collect domain events. |
| `Genocs.Common` | Discover `INotificationSender`, `IJobService`, and `ISerializerService` through `ITransientDependency`. |
| `Genocs.Logging` | Call `StaticLogger.EnsureInitialized` and `IHostBuilder.UseLogging`. Put level overrides in the `logger` section. |

## Capability reference

### Caller DbContext repositories

`AddGenocsRepositories<TContext>(this IServiceCollection services, Assembly domainAssembly)` in `Genocs.Persistence.EFCore.Extensions`:

- `TContext : DbContext`. A multitenant context is valid.
- `TContext` must be public. The open repository type is generated in another assembly and cannot name an internal context.
- Registers open `IRepository<>` and `IReadRepository<>` against one implementation constructed with `TContext`. The implementation is a generated subclass of `GenocsRepository<T, TContext>`.
- Scans only `domainAssembly` for exported, non-abstract, non-generic classes that implement both `IAggregateRoot` and `IGeneratesDomainEvents`.
- Registers closed `IRepositoryWithEvents<T>` for those types. The decorator is created from the resolved `IRepository<T>`.
- Does not scan `Genocs.Common`. Does not append `Assembly.GetEntryAssembly`.
- Uses `TryAdd` for the open generics. The first caller chooses the context. A later call does not replace it.

`AddEFCorePersistence` still registers those open generics against `ApplicationDbContext`. It scans `Assembly.GetEntryAssembly()` plus any assemblies passed to it, and registers `IRepositoryWithEvents<T>` for aggregates in those assemblies that implement `IGeneratesDomainEvents`.

Source: [src/Persistence.EFCore/Extensions/EFCoreExtensions.cs](../../src/Persistence.EFCore/Extensions/EFCoreExtensions.cs).

### Domain events

Do not add a host base type that reimplements event storage.

| Contract | Rule |
|---|---|
| `AggregateRoot` and `AggregateRoot<TPrimaryKey>` in `Genocs.Core` | Preferred aggregate bases. They implement `IGeneratesDomainEvents` and store `IEvent`. |
| `IRepository<T>` and `IReadRepository<T>` | Require `T : class, IAggregateRoot`. |
| `IRepositoryWithEvents<T>` | Registered only when `T` is also `IGeneratesDomainEvents`. |
| `EntityCreatedEvent<T>`, `EntityUpdatedEvent<T>`, `EntityDeletedEvent<T>` | Require `Genocs.Common.Domain.Entities.IEntity`. `IAggregateRoot` already extends `IEntity`. |
| Non-generic `IAggregateRoot` only | Receives read and write repositories. Does not receive `IRepositoryWithEvents<T>` because it has no event collection. |
| `IAggregateRoot<TKey>` | Includes `IGeneratesDomainEvents`. A normal `IAggregateRoot<Guid>` aggregate receives `IRepositoryWithEvents<T>`. |

The decorator constraint is `where T : class, IAggregateRoot, IGeneratesDomainEvents`. It is not `IAggregateRoot<T>` (the aggregate type as its own key).

### Audit request

`Genocs.Persistence.EFCore.Auditing.GetAuditLogsRequest` stays public. The package does not ship `GetAuditLogsRequestHandler`.

MediatR registers concrete handlers it finds in a scanned assembly, including internal handlers. A handler in this package would require `ICurrentUser` and `IAuditService`, and the package registers neither. The host owns the handler and both services.

### Dependency markers

These host-implemented interfaces extend `ITransientDependency` from `Genocs.Common.Dependency`:

- `INotificationSender`
- `IJobService`
- `ISerializerService`

`ITransientService` is obsolete and already extends `ITransientDependency`. Migrated interfaces do not also extend `ITransientService`.

`ICurrentUser` stays unmarked. It is per request and stays an explicit registration. `IAuditService` and `IDapperRepository` already extend `ITransientDependency`.

The library does not scan the host assembly. The host convention scanner must look for `ITransientDependency`. A scanner that only looks for `ITransientService` will not discover the three interfaces above.

### Logging

`LoggerOptions.MinimumLevelOverrides` is the host override map. `UseLogging` applies it. There is no separate property for `Microsoft`, `Microsoft.EntityFrameworkCore`, or `Hangfire`.

`StaticLogger.EnsureInitialized()` installs a bootstrap logger. Its overrides last until `builder.Host.UseLogging()` reads the `logger` section. The host calls that `UseLogging` extension once.

`logger.excludePaths` drops an entire log event whose `RequestPath` ends with a listed suffix. It does not skip HTTP body capture for a path fragment.

Correlation middleware copies Activity baggage into the log scope and can capture HTTP bodies through `logger.httpPayload`. It does not push user id, email, or tenant.

## What this upgrade does not include

- No HTTP log scope for the current user id, email, or tenant.
- No path-fragment exclusion for body capture.
- No library registration of `IAuditService` or `ICurrentUser`.
- No second domain-event base type.
- No replacement of `ApplicationDbContext` inside `AddEFCorePersistence`.

Keep the host HTTP middleware that pushes user and tenant scope, and any middleware that skips body capture for selected paths.

## Decision matrix

| Host situation | Do this |
|---|---|
| Domain `DbContext` is `ApplicationDbContext` | `AddEFCorePersistence(domainAssembly)` when the aggregate assembly is not the entry assembly. Delete the local repository loop. |
| Domain `DbContext` is a public caller or multitenant context | Call `services.AddGenocsRepositories<TContext>(domainAssembly)` before `AddEFCorePersistence`. |
| Aggregate collects events | Inherit `AggregateRoot<TPrimaryKey>` or implement `IAggregateRoot` and `IGeneratesDomainEvents`. |
| Aggregate is only a non-generic `IAggregateRoot` | Inject `IRepository<T>` or `IReadRepository<T>`. Do not inject `IRepositoryWithEvents<T>`. |
| Host sends `GetAuditLogsRequest` | Register a host `IRequestHandler<GetAuditLogsRequest, List<AuditDto>>` plus `ICurrentUser` and `IAuditService`. |
| Host implements `INotificationSender`, `IJobService`, or `ISerializerService` | Scan `ITransientDependency`. Delete the explicit `AddTransient` for that interface. |
| Host logs to console and file | `StaticLogger.EnsureInitialized()`, `builder.Host.UseLogging()`, and a `logger` section. |
| Host still logs user id, email, and tenant on each request | Keep the host middleware. |

## Migration order

Apply these steps in the host repository after the upgraded packages are referenced.

1. Switch the host to Genocs logging. Call `StaticLogger.EnsureInitialized()` before the host is built. Call `builder.Host.UseLogging()` once. Register correlation with `AddCorrelationContextLogging()` and `UseCorrelationContextLogging()`. Add `logger.minimumLevelOverrides` for `Microsoft`, `Microsoft.EntityFrameworkCore`, and `Hangfire`. Delete a second Serilog extension that is also named `UseLogging`. Delete unused sink packages the host no longer wires. Run the API and confirm console and file output. Leave the HTTP user-scope middleware in place.
2. Point repository registration at the library. For `ApplicationDbContext`, pass the domain assembly to `AddEFCorePersistence` and delete the local loop. For any other public `DbContext`, call `AddGenocsRepositories<TContext>(domainAssembly)` first, then delete the local loop.
3. Stop expecting an audit handler from `Genocs.Persistence.EFCore`. Do not call `AddMediatR` on that assembly in order to obtain `GetAuditLogsRequestHandler`. If the host handles `GetAuditLogsRequest`, register the handler in the host assembly.
4. Align convention registration with `ITransientDependency`. After the host scanner sees that marker, delete explicit `AddTransient` calls for `INotificationSender`, `IJobService`, and `ISerializerService`. Leave `ICurrentUser` explicitly registered with its request lifetime.
5. Confirm aggregates that should raise entity events implement `IGeneratesDomainEvents`, usually by inheriting `AggregateRoot<TPrimaryKey>`.

## Recipes

### ApplicationDbContext host

```csharp
using Genocs.Persistence.EFCore.Extensions;

builder.Services.AddSqlServerDbProvider();

builder.AddGenocs()
    .AddEFCorePersistence(typeof(Order).Assembly);
```

`Order` must be a public concrete aggregate in that assembly. `IRepository<Order>` and `IReadRepository<Order>` resolve for any `IAggregateRoot`. `IRepositoryWithEvents<Order>` resolves when `Order` also implements `IGeneratesDomainEvents`.

### Caller or multitenant DbContext

```csharp
using Genocs.Persistence.EFCore.Extensions;

builder.Services.AddGenocsRepositories<AppDbContext>(typeof(Order).Assembly);

builder.AddGenocs()
    .AddEFCorePersistence();
```

`AppDbContext` is public and is not `ApplicationDbContext`. The `AddGenocsRepositories` call comes first so `TryAdd` keeps `AppDbContext` on `IRepository<>` and `IReadRepository<>`.

### Aggregate

```csharp
using Genocs.Core.Domain.Entities;

public sealed class Order : AggregateRoot<Guid>
{
}
```

`AggregateRoot<Guid>` supplies `IEntity`, `IAggregateRoot`, `IGeneratesDomainEvents`, and the domain-event list. `EntityCreatedEvent<Order>` is then valid.

### Logging host

```csharp
using Genocs.Core.Builders;
using Genocs.Logging;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.AddGenocs()
    .AddCorrelationContextLogging();

builder.Host.UseLogging();

var app = builder.Build();
app.UseCorrelationContextLogging();
app.Run();
```

```json
{
  "logger": {
    "level": "Information",
    "minimumLevelOverrides": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Hangfire": "Warning"
    },
    "console": {
      "enabled": true
    },
    "file": {
      "enabled": true,
      "path": "logs/logs.txt",
      "interval": "Day"
    }
  }
}
```

### Service discovery

```csharp
using Genocs.Common.Dependency;
using Genocs.Common.Interfaces;

public sealed class SignalRNotificationSender : INotificationSender
{
    // host implementation
}
```

Register it by scanning `ITransientDependency`. Do not also call `AddTransient<INotificationSender, SignalRNotificationSender>()` once that scan is active.

## Do not assume

- Do not assume `AddEFCorePersistence` binds `IRepository<>` to a derived or multitenant context. It binds `ApplicationDbContext`.
- Do not assume a second `AddGenocsRepositories<TContext>` call replaces the first context.
- Do not assume an internal `DbContext` can be `TContext`.
- Do not assume internal or non-exported aggregate types are discovered. The scan uses `GetExportedTypes`.
- Do not assume every `IAggregateRoot` has `IRepositoryWithEvents<T>`.
- Do not assume `IAggregateRoot<T>` means the aggregate type is its own key. `IAggregateRoot<Guid>` is the normal key form and qualifies for the event repository.
- Do not assume `Genocs.Persistence.EFCore` registers an audit handler, `IAuditService`, or `ICurrentUser`.
- Do not assume making a handler internal hides it from MediatR assembly scanning.
- Do not assume `INotificationSender` is discovered by an `ITransientService` scan.
- Do not assume `ICurrentUser` should extend `ITransientDependency`.
- Do not assume `logger.excludePaths` or correlation middleware records the current user or skips body capture for a path fragment.
- Do not add a second `UseLogging` extension in the host.

## Verification

- Resolve `IReadRepository<T>` for a public aggregate in the domain assembly, outside `Genocs.Common`.
- Resolve `IRepositoryWithEvents<T>` for an aggregate that implements `IGeneratesDomainEvents`.
- Confirm an aggregate that only implements non-generic `IAggregateRoot` has no `IRepositoryWithEvents<T>` registration.
- For a caller context, resolve `IRepository<T>` and confirm the injected context is that public `TContext`.
- Build a host `ServiceCollection`, call MediatR `RegisterServicesFromAssembly` on `Genocs.Persistence.EFCore`, and resolve handlers without `ICurrentUser` or `IAuditService`.
- Confirm `INotificationSender`, `IJobService`, and `ISerializerService` are assignable to `ITransientDependency` and are not assignable to `ITransientService`.
- Start the API and confirm console and file logs. Confirm `Microsoft`, `Microsoft.EntityFrameworkCore`, and `Hangfire` follow `logger.minimumLevelOverrides`.
- Exercise one request that previously logged user id, email, and tenant, and confirm the host middleware still does that.

## Source of truth

- [src/Persistence.EFCore/Extensions/EFCoreExtensions.cs](../../src/Persistence.EFCore/Extensions/EFCoreExtensions.cs)
- [src/Persistence.EFCore/Repositories/EventAddingRepositoryDecorator.cs](../../src/Persistence.EFCore/Repositories/EventAddingRepositoryDecorator.cs)
- [src/Persistence.EFCore/Auditing/GetAuditLogsRequest.cs](../../src/Persistence.EFCore/Auditing/GetAuditLogsRequest.cs)
- [src/Core/Domain/Entities/AggregateRoot.cs](../../src/Core/Domain/Entities/AggregateRoot.cs)
- [src/Common/Interfaces/INotificationSender.cs](../../src/Common/Interfaces/INotificationSender.cs)
- [src/Common/Interfaces/IJobService.cs](../../src/Common/Interfaces/IJobService.cs)
- [src/Common/Interfaces/ISerializerService.cs](../../src/Common/Interfaces/ISerializerService.cs)
- [src/Logging/README.md](../../src/Logging/README.md)
- [Genocs.Persistence.EFCore-Agent-Documentation.md](Genocs.Persistence.EFCore-Agent-Documentation.md)
- [Genocs.Logging-Agent-Documentation.md](Genocs.Logging-Agent-Documentation.md)
