using System.Reflection;
using System.Runtime.CompilerServices;
using Genocs.Microservice.Template.Infrastructure.Auth;
using Genocs.Microservice.Template.Infrastructure.BackgroundJobs;
using Genocs.Microservice.Template.Infrastructure.Caching;
using Genocs.Microservice.Template.Infrastructure.Common;
using Genocs.Microservice.Template.Infrastructure.Cors;
using Genocs.Microservice.Template.Infrastructure.FileStorage;
using Genocs.Microservice.Template.Infrastructure.Localization;
using Genocs.Microservice.Template.Infrastructure.Mailing;
using Genocs.Microservice.Template.Infrastructure.Mapping;
using Genocs.Microservice.Template.Infrastructure.Middleware;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;
using Genocs.Microservice.Template.Infrastructure.Notifications;
using Genocs.Microservice.Template.Infrastructure.OpenApi;
using Genocs.Microservice.Template.Infrastructure.Persistence;
using Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;
using Genocs.Microservice.Template.Infrastructure.SecurityHeaders;
using Genocs.Microservice.Template.Infrastructure.Validations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Infrastructure.Test")]

namespace Genocs.Microservice.Template.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var applicationAssembly = typeof(Application.Startup).GetTypeInfo().Assembly;
        MapsterSettings.Configure();
        return services
            .AddGnxApiVersioning()
            .AddGnxAuth(config)
            .AddBackgroundJobs(config)
            .AddCaching(config)
            .AddCorsPolicy(config)
            .AddExceptionMiddleware()
            .Behaviors(applicationAssembly)
            .AddHealthCheck()
            .AddPOLocalization(config)
            .AddMailing(config)
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()))
            .AddMultitenancy()
            .AddNotifications(config)
            .AddOpenApiDocumentation(config)
            .AddPersistence()
            .AddRequestLogging(config)
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices();
    }

    private static IServiceCollection AddGnxApiVersioning(this IServiceCollection services)
    {
        // Core API Versioning services with support for Minimal APIs
        // API version-aware extensions for MVC Core with controllers (not full MVC)
        services
                .AddApiVersioning()
                .AddMvc()
                .AddApiExplorer();

        // API version-aware API Explorer extensions
        services.AddEndpointsApiExplorer();
        return services;
    }

    private static IServiceCollection AddHealthCheck(this IServiceCollection services)
        => services.AddHealthChecks().AddCheck<TenantHealthCheck>("Tenant").Services;

    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(cancellationToken);
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
        builder
            .UseRequestLocalization()
            .UseStaticFiles()
            .UseSecurityHeaders(config)
            .UseFileStorage()
            .UseExceptionMiddleware()
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseGnxCurrentUser()
            .UseMultiTenancy()
            .UseAuthorization()
            .UseRequestLogging(config)
            .UseHangfireDashboard(config)
            .UseOpenApiDocumentation(config);

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        builder.MapHealthCheck();
        builder.MapNotifications();
        return builder;
    }

    private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("/hc");
}