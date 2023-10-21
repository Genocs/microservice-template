using Genocs.Microservice.Template.Application.Multitenancy;
using Genocs.Microservice.Template.Infrastructure.Persistence;
using Genocs.Microservice.Template.Shared.Authorization;
using Genocs.Microservice.Template.Shared.Multitenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Genocs.Microservice.Template.Infrastructure.Multitenancy;

internal static class Startup
{
    internal static IServiceCollection AddMultitenancy(this IServiceCollection services)
    {
        return services
            .AddDbContext<TenantDbContext>((p, m) =>
            {
                // TODO: We should probably add specific dbprovider/connectionstring setting for the tenantDb with a fallback to the main databasesettings
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            })
            .AddMultiTenant<GNXTenantInfo>()
                .WithClaimStrategy(GNXClaims.Tenant)
                .WithHeaderStrategy(MultitenancyConstants.TenantIdName)
                .WithQueryStringStrategy(MultitenancyConstants.TenantIdName)
                .WithEFCoreStore<TenantDbContext, GNXTenantInfo>()
                .Services
            .AddScoped<ITenantService, TenantService>();
    }

    internal static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app) =>
        app.UseMultiTenant();

    private static FinbuckleMultiTenantBuilder<GNXTenantInfo> WithQueryStringStrategy(this FinbuckleMultiTenantBuilder<GNXTenantInfo> builder, string queryStringKey) =>
        builder.WithDelegateStrategy(context =>
        {
            if (context is not HttpContext httpContext)
            {
                return Task.FromResult((string?)null);
            }

            httpContext.Request.Query.TryGetValue(queryStringKey, out var tenantIdParam);

            return Task.FromResult((string?)tenantIdParam.ToString());
        });
}