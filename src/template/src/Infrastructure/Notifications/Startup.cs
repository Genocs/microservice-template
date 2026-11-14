using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Genocs.Microservice.Template.Infrastructure.Notifications;

internal static class Startup
{
    internal static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration config)
    {
        var logger = Log.ForContext(typeof(Startup));

        SignalRSettings? settings = config.GetSection(SignalRSettings.Position).Get<SignalRSettings>();

        if (settings is null)
        {
            logger.Warning($"SignalR settings is to null. Please check '{SignalRSettings.Position}' section on config file.");
            return services;
        }

        if (!settings.UseBackplane)
        {
            logger.Warning($"SignalR settings has UseBackplane set to 'false'.");
            services.AddSignalR();
        }
        else
        {
            if (settings.BackPlane is null)
            {
                throw new InvalidOperationException("Backplane enabled, but no backplane settings in config.");
            }

            logger.Information($"SignalR Backplane Provider is '{settings.BackPlane.Provider}'.");

            switch (settings.BackPlane.Provider)
            {
                case "redis":
                    if (settings.BackPlane.StringConnection is null) throw new InvalidOperationException("Redis backplane provider: No connectionString configured.");
                    services.AddSignalR().AddStackExchangeRedis(settings.BackPlane.StringConnection, options =>
                    {
                        options.Configuration.AbortOnConnectFail = false;
                    });
                    break;

                default:
                    throw new InvalidOperationException($"SignalR backplane Provider {settings.BackPlane.Provider} is not supported.");
            }

        }

        return services;
    }

    internal static IEndpointRouteBuilder MapNotifications(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<NotificationHub>("/notifications", options =>
        {
            options.CloseOnAuthenticationExpiration = true;
        });

        return endpoints;
    }
}