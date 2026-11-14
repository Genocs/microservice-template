# Genocs library setup for the microservice template

This note records what blocked the template after it moved onto Genocs 9.1.0, whether `Genocs.Logging` can replace the template Serilog host, and which library changes are worth making so the next host does not repeat this work.

Packages inspected: `Genocs.Core` 9.1.0, `Genocs.Common` 9.1.0, `Genocs.Logging` 9.1.0, `Genocs.Persistence.EFCore` 9.1.0.

## Verdict

`Genocs.Logging` should replace the template Serilog bootstrap. It already covers every sink the template code actually turns on, and it binds its own configuration. The template HTTP request and response middleware should stay until the library can redact sensitive routes and attach the current user and tenant.

The persistence and convention-registration gaps are worth fixing in the library. Until those exist, a host that keeps its own `DbContext` has to supply a local repository and must not let MediatR scan Genocs assemblies.

## Logging

### What the template does today

`Program.cs` calls `Genocs.Logging.StaticLogger.EnsureInitialized()`, then `IHostBuilder.UseLogging(environment)` from `Infrastructure/Logging/Serilog/Extensions.cs`. That method is not the Genocs one. Both types are named `UseLogging`, and the Infrastructure namespace wins.

The template method builds Serilog with:

- console output, plain or compact JSON
- a rolling JSON file when `WriteToFile` is true
- Elasticsearch when `ElasticSearchUrl` is non-empty, with no separate enable flag
- enrichers for application name, exception details, machine, process, thread, and log context
- level overrides for Microsoft, Hangfire, hosting lifetime, and Entity Framework

`logger.json` uses the section `loggerSettings`. Only `RegisterSerilog` binds that section, and `Program.cs` does not call `RegisterSerilog`. `UseLogging` resolves `IOptions<LoggerSettings>` without binding, so the file is not applied and the host runs on the class defaults (`WriteToFile` false, empty Elasticsearch URL, minimum level Information). `SetMinimumLogLevel` also calls `ToLower()` on the level string, so a null value fails startup.

`Infrastructure.csproj` references `AWS.Logger.SeriLog` and `Serilog.Sinks.MSSqlServer`. Neither is used by `Extensions.cs`.

HTTP logging is separate. `RequestLoggingMiddleware` and `ResponseLoggingMiddleware` log method, path, JSON body, user id, email, and tenant. They redact paths that contain `tokens` and skip response capture for paths that contain `jobs`.

### What Genocs.Logging 9.1.0 provides

The package README is the contract. Entry points:

- `UseLogging`
- `AddCorrelationContextLogging` and `UseCorrelationContextLogging`
- `AddCommandHandlersLogging` and `AddEventHandlersLogging`
- `MapLogLevelHandler`

The configuration section is `logger`. Sinks are opt-in: console, local file, Elasticsearch, Seq, Loki, Azure Application Insights, and OTLP logs. `logger.enabled` defaults to true. Elasticsearch, Seq, and Loki stay off unless their own `enabled` flag is set and a URL is present. Payload capture is off unless `logger.httpPayload.enabled` is true, and body length is capped.

Distributed traces and metrics stay in `Genocs.Telemetry`. `Genocs.Logging` does not export them.

`AddCommandHandlersLogging` and `AddEventHandlersLogging` decorate Genocs CQRS handlers. They do not decorate MediatR `IRequestHandler` or `INotificationHandler`. The template dispatches through MediatR, so those decorators do not replace the existing handler logs.

### Replacement

Use Genocs for the host pipeline. Drop `Infrastructure/Logging/Serilog/Extensions.cs`, `LoggerSettings`, and the `loggerSettings` section. Keep `StaticLogger.EnsureInitialized()` and `Log.CloseAndFlush()` in `Program.cs`. Call the Genocs `UseLogging` once, and remove the Infrastructure `using` that hides it.

A development `logger` section that matches the current behavior:

```json
{
  "logger": {
    "enabled": true,
    "console": {
      "enabled": true,
      "enableStructured": false
    },
    "file": {
      "enabled": true,
      "path": "Logs/logs.json",
      "interval": "Day"
    },
    "elk": {
      "enabled": false,
      "url": ""
    }
  }
}
```

Confirm the file property names against `LocalFileOptions` when wiring this. The XML documents `Enabled`, `Path`, and `Interval`. The section name on `LoggerOptions` is `logger`.

Keep the two HTTP middleware classes. Genocs correlation middleware adds baggage and an optional body capture. It does not redact token routes, skip Hangfire job responses, or push `UserId`, email, and tenant from `ICurrentUser`. Running both body-capture middlewares would buffer every request twice.

Package cleanup after the switch: remove `AWS.Logger.SeriLog` and `Serilog.Sinks.MSSqlServer` unless a host feature starts using them. `Genocs.Logging` already brings Serilog, the enrichers, Elasticsearch, Seq, and the async sink.

## Setup friction found while starting the API

These are the reasons a consumer still cannot call one Genocs method and run.

### Persistence is bound to the library DbContext

`Genocs.Persistence.EFCore.Repositories.ApplicationDbRepository<T>` is built for `Genocs.Persistence.EFCore.Context.ApplicationDbContext`. The template context is a different type. It owns Finbuckle multitenancy, auditing, and domain-event dispatch, so it cannot be swapped for the library context.

`ApplicationDbInitializer` has the same coupling. Its constructor asks for the library context, the library seeder, `IEFCoreDbProvider`, and `DatabaseOptions`.

The template therefore keeps its own context and registers a local `ApplicationDbRepository<T>` that implements `Genocs.Core.Domain.Repositories.IRepository<T>` and `IReadRepository<T>` on top of Ardalis `RepositoryBase<T>`.

### Aggregate registration scans the wrong assembly

`IAggregateRoot` lives in `Genocs.Common`. `typeof(IAggregateRoot).Assembly` is that package, not the host domain. A loop written that way never sees `Brand` or `Product`, so `IReadRepository<T>` and `IRepositoryWithEvents<T>` are missing and development service validation fails for every catalog handler.

`EventAddingRepositoryDecorator<T>` appends events through `IGeneratesDomainEvents.AddDomainEvent`. The template entity base only had `List<DomainEvent>`. The host now implements `IGeneratesDomainEvents` explicitly because the property type is `IReadOnlyCollection<IEvent>`, not `List<DomainEvent>`.

There is no library helper that does this scan against a caller-supplied assembly.

### Library handlers are registered by a wide MediatR scan

`Genocs.Persistence.EFCore` contains `GetAuditLogsRequestHandler`. It depends on `Genocs.Core.Domain.Entities.Auditing.IAuditService`. That is a different type from `Genocs.Microservice.Template.Application.Auditing.IAuditService`.

`AddMediatR(AppDomain.CurrentDomain.GetAssemblies())` registers that handler. The host then fails validation because the Genocs audit service is not registered. The template now registers MediatR only from the Application assembly and the Infrastructure assembly.

### Convention registration misses Genocs service interfaces

`AddServices(typeof(ITransientService))` only registers a class when one of its interfaces is assignable to `ITransientService`. `Genocs.Common.Interfaces.INotificationSender` does not extend that marker, so `NotificationSender` was invisible and every handler that takes `INotificationSender` failed validation.

`ITransientService` and `IScopedService` are marked obsolete in favor of `Genocs.Common.Dependency.ITransientDependency` and `IScopedDependency`. The template still uses the obsolete markers. New library interfaces do not implement either set, so convention registration and the new dependency model disagree.

`INotificationSender` is now registered explicitly next to SignalR.

### Host composition is split

`Program.cs` calls `AddGenocs()` and later `genocsBuilder.Build(app.Services)`. It does not call `UseGenocs`. Logging, persistence, and MediatR are each configured by the template beside that call. A reader cannot tell which pipeline owns startup.

## Library changes worth making

Do these in `genocs-library`. Each one removes a template workaround.

1. **Repository registration for a caller DbContext.** Add something in the shape of `AddGenocsRepositories<TContext>(this IServiceCollection services, Assembly domainAssembly)` that:
   - registers `IRepository<>` and `IReadRepository<>` against an implementation constructed with `TContext`
   - finds concrete `IAggregateRoot` types in `domainAssembly`, not in `Genocs.Common`
   - registers `IRepositoryWithEvents<>` with `EventAddingRepositoryDecorator<>`
   - keeps `TContext : DbContext` so a multitenant context still works

2. **Document or supply the domain-event base.** Either publish a small base type that implements `IGeneratesDomainEvents` and stores `DomainEvent`, or state next to the decorator that the aggregate must implement `IGeneratesDomainEvents` and that `EntityCreatedEvent<T>` requires `Genocs.Common.Domain.Entities.IEntity`. The template base had to learn this by failing at runtime.

3. **Stop shipping mandatory MediatR handlers inside persistence.** Make `GetAuditLogsRequestHandler` internal, move it to an opt-in extension, or give it a service the library registers itself. A public `IRequestHandler` in a referenced package is activated by any host that scans loaded assemblies.

4. **Make service interfaces participate in convention registration.** `INotificationSender` should extend `ITransientDependency` (and the obsolete `ITransientService` until callers migrate). Do the same for any other interface a host is expected to implement and discover. Keep one marker model.

5. **Logging host sample and level overrides.** Add a minimal `Program.cs` plus `logger` JSON to the logging README that shows `StaticLogger`, Genocs `UseLogging`, and `UseCorrelationContextLogging` without a second Serilog extension of the same name. Expose minimum-level overrides for `Microsoft`, `Microsoft.EntityFrameworkCore`, and `Hangfire` on `LoggerOptions`. The template hard-codes those overrides today, and Genocs does not document an equivalent.

6. **Optional HTTP log scope for the current user.** Extend correlation logging so a caller can push user id, email, and tenant, and can exclude path fragments from body capture. That is the only template logging behavior Genocs does not already cover. Until it exists, the template middleware stays.

Items 1 through 4 are the ones that unblock a new service. Item 5 is documentation plus a small options addition. Item 6 is optional and should not block the logging switch.

## Suggested order

1. In the template, switch the host to Genocs `UseLogging` and the `logger` section. Leave the HTTP middleware in place. Delete the unused Serilog extension and the unused AWS and SQL Server sink packages. Run the Web API and confirm console and file output.
2. In the library, add `AddGenocsRepositories<TContext>` and a test that a domain assembly outside `Genocs.Common` gets `IReadRepository<T>` and `IRepositoryWithEvents<T>`. Point the template at that helper and delete the local registration loop.
3. In the library, hide or opt in `GetAuditLogsRequestHandler`. Add a test that referencing `Genocs.Persistence.EFCore` and calling `AddMediatR` on that assembly does not require a service the package does not register.
4. In the library, align `INotificationSender` with `ITransientDependency`. The template can then drop the explicit `AddTransient` once it scans the new marker.
5. Add the logging README sample and the level-override options. Revisit HTTP user scope only if more than one Genocs host needs the template middleware.

## What not to move into the library

- The template `ApplicationDbContext`, tenant store, and seeders. They are product behavior.
- The local `ISoftDelete` shape (`DeletedOn`, `DeletedBy`). Genocs `ISoftDelete` is `IsDeleted`. Changing the template filter to the library contract is a schema change, not a setup fix.
- MediatR pipeline behaviors for validation. Genocs CQRS logging decorators solve a different pipeline.
