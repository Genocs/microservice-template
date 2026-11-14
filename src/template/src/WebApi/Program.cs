using Genocs.Core.Builders;
using Genocs.Logging;
using Genocs.Microservice.Template.Application;
using Genocs.Microservice.Template.Infrastructure;
using Genocs.Microservice.Template.WebApi.Configurations;
using Genocs.Microservice.Template.WebApi.Controllers;
using Serilog;

[assembly: ApiConventionType(typeof(GNXApiConventions))]

StaticLogger.EnsureInitialized();
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations();

    IGenocsBuilder genocsBuilder = builder
            .AddGenocs()
            .AddCorrelationContextLogging();

    builder.Host.UseLogging();

    builder.Services.AddControllers();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    var app = builder.Build();
    genocsBuilder.Build(app.Services);

    await app.Services.InitializeDatabasesAsync();

    app.UseCorrelationContextLogging();
    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
