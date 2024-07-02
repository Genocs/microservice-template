using Figgle;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace Genocs.Microservice.Template.Infrastructure.Logging.Serilog;

public static class Extensions
{
    public static void RegisterSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<LoggerSettings>().BindConfiguration(nameof(LoggerSettings));

        _ = builder.Host.UseSerilog((_, sp, serilogConfig) =>
        {
            var loggerSettings = sp.GetRequiredService<IOptions<LoggerSettings>>().Value;
            string appName = loggerSettings.AppName;
            string elasticSearchUrl = loggerSettings.ElasticSearchUrl;
            bool writeToFile = loggerSettings.WriteToFile;
            bool structuredConsoleLogging = loggerSettings.StructuredConsoleLogging;
            string minLogLevel = loggerSettings.MinimumLogLevel;
            ConfigureEnrichers(serilogConfig, appName);
            ConfigureConsoleLogging(serilogConfig, structuredConsoleLogging);
            ConfigureWriteToFile(serilogConfig, writeToFile);
            ConfigureElasticSearch(builder.Environment.EnvironmentName, serilogConfig, appName, elasticSearchUrl);
            SetMinimumLogLevel(serilogConfig, minLogLevel);
            OverrideMinimumLogLevel(serilogConfig);
        });
    }

    public static IHostBuilder UseLogging(this IHostBuilder builder, string environment = "debug")
    {
        builder.UseSerilog((_, sp, serilogConfig) =>
         {
             var loggerSettings = sp.GetRequiredService<IOptions<LoggerSettings>>().Value;
             string appName = loggerSettings.AppName;
             string elasticSearchUrl = loggerSettings.ElasticSearchUrl;
             bool writeToFile = loggerSettings.WriteToFile;
             bool structuredConsoleLogging = loggerSettings.StructuredConsoleLogging;
             string minLogLevel = loggerSettings.MinimumLogLevel;
             ConfigureEnrichers(serilogConfig, appName);
             ConfigureConsoleLogging(serilogConfig, structuredConsoleLogging);
             ConfigureWriteToFile(serilogConfig, writeToFile);
             ConfigureElasticSearch(environment, serilogConfig, appName, elasticSearchUrl);
             SetMinimumLogLevel(serilogConfig, minLogLevel);
             OverrideMinimumLogLevel(serilogConfig);
         });

        return builder;
    }

    private static void ConfigureEnrichers(LoggerConfiguration serilogConfig, string appName)
    {
        serilogConfig
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", appName)
                        .Enrich.WithExceptionDetails()
                        .Enrich.WithMachineName()
                        .Enrich.WithProcessId()
                        .Enrich.WithThreadId()
                        .Enrich.FromLogContext();
    }

    private static void ConfigureConsoleLogging(LoggerConfiguration serilogConfig, bool structuredConsoleLogging)
    {
        if (structuredConsoleLogging)
        {
            serilogConfig.WriteTo.Async(wt => wt.Console(new CompactJsonFormatter()));
        }
        else
        {
            serilogConfig.WriteTo.Async(wt => wt.Console());
        }
    }

    private static void ConfigureWriteToFile(LoggerConfiguration serilogConfig, bool writeToFile)
    {
        if (writeToFile)
        {
            serilogConfig.WriteTo.File(
             new CompactJsonFormatter(),
             "Logs/logs.json",
             restrictedToMinimumLevel: LogEventLevel.Information,
             rollingInterval: RollingInterval.Day,
             retainedFileCountLimit: 5);
        }
    }

    private static void ConfigureElasticSearch(string environment, LoggerConfiguration serilogConfig, string appName, string? elasticSearchUrl)
    {
        if (!string.IsNullOrWhiteSpace(elasticSearchUrl))
        {
            string? formattedAppName = appName?.ToLower().Replace(".", "-").Replace(" ", "-");
            string indexFormat = $"{formattedAppName}-logs-{environment.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}";
            serilogConfig.WriteTo.Async(writeTo =>
            writeTo.Elasticsearch(new(new Uri(elasticSearchUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = indexFormat,
                MinimumLogEventLevel = LogEventLevel.Information,
            })).Enrich.WithProperty("Environment", environment);
        }
    }

    private static void OverrideMinimumLogLevel(LoggerConfiguration serilogConfig)
    {
        serilogConfig
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                     .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
                     .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error);
    }

    private static void SetMinimumLogLevel(LoggerConfiguration serilogConfig, string minLogLevel)
    {
        switch (minLogLevel.ToLower())
        {
            case "debug":
                serilogConfig.MinimumLevel.Debug();
                break;
            case "information":
                serilogConfig.MinimumLevel.Information();
                break;
            case "warning":
                serilogConfig.MinimumLevel.Warning();
                break;
            default:
                serilogConfig.MinimumLevel.Information();
                break;
        }
    }
}
