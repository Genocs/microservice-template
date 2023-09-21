using Genocs.Microservice.Application;
using Genocs.Microservice.Host.Configurations;
using Genocs.Microservice.Host.Controllers;
using Genocs.Microservice.Infrastructure;
using Genocs.Microservice.Infrastructure.Common;
using Genocs.Microservice.Infrastructure.Logging.Serilog;
using Serilog;
using Serilog.Formatting.Compact;

[assembly: ApiConventionType(typeof(FSHApiConventions))]

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations().RegisterSerilog();
    builder.Services.AddControllers();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    var app = builder.Build();

    await app.Services.InitializeDatabasesAsync();

    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}