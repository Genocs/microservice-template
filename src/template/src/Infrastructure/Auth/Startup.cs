using Genocs.Microservice.Template.Application.Common.Interfaces;
using Genocs.Microservice.Template.Infrastructure.Auth.AzureAd;
using Genocs.Microservice.Template.Infrastructure.Auth.Jwt;
using Genocs.Microservice.Template.Infrastructure.Auth.Permissions;
using Genocs.Microservice.Template.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Microservice.Template.Infrastructure.Auth;

internal static class Startup
{
    internal static IServiceCollection AddGnxAuth(this IServiceCollection services, IConfiguration config)
    {
        services
            .AddGnxCurrentUser()
            .AddGnxPermissions()

            // Must add identity before adding auth!
            .AddIdentity();
        services.Configure<SecuritySettings>(config.GetSection(nameof(SecuritySettings)));
        return config["SecuritySettings:Provider"]!.Equals("AzureAd", StringComparison.OrdinalIgnoreCase)
            ? services.AddAzureAdAuth(config)
            : services.AddJwtAuth();
    }

    internal static IApplicationBuilder UseGnxCurrentUser(this IApplicationBuilder app) =>
        app.UseMiddleware<CurrentUserMiddleware>();

    private static IServiceCollection AddGnxCurrentUser(this IServiceCollection services) =>
        services
            .AddScoped<CurrentUserMiddleware>()
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

    private static IServiceCollection AddGnxPermissions(this IServiceCollection services) =>
        services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
}