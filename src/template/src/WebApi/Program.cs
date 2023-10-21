using Genocs.Microservice.Template.Application;
using Genocs.Microservice.Template.Infrastructure;
using Genocs.Microservice.Template.Infrastructure.Common;
using Genocs.Microservice.Template.Infrastructure.Logging.Serilog;
using Genocs.Microservice.Template.WebApi.Configurations;
using Genocs.Microservice.Template.WebApi.Controllers;
using Serilog;

[assembly: ApiConventionType(typeof(GNXApiConventions))]

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