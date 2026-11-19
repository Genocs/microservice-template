using Genocs.Common.Persistence.Initialization;
using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Domain.Catalog;
using Genocs.Microservice.Template.Infrastructure.Common;
using Genocs.Microservice.Template.Infrastructure.Persistence.ConnectionString;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;
using Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;
using Genocs.Persistence.EFCore.Extensions;
using Genocs.Persistence.EFCore.Initialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#if MySQL
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif
using Serilog;

namespace Genocs.Microservice.Template.Infrastructure.Persistence;

internal static class Startup
{
    private static readonly ILogger _logger = Log.ForContext(typeof(Startup));

    internal static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services
            .AddOptions<DatabaseSettings>()
            .BindConfiguration(nameof(DatabaseSettings))
            .PostConfigure(databaseSettings =>
            {
                _logger.Information("Current DB Provider: {dbProvider}", databaseSettings.DBProvider);
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services
            .AddDbContext<ApplicationDbContext>((p, m) =>
            {
                var databaseSettings = p.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                m.UseDatabase(databaseSettings.DBProvider, databaseSettings.ConnectionString);
            })
            .AddTransient<IDatabaseInitializer, DatabaseInitializer>()
            .AddTransient<Initialization.ApplicationDbInitializer>()
            .AddTransient<Initialization.ApplicationDbSeeder>()
            .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
            .AddTransient<CustomSeederRunner>()
            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>()
            .AddTransient<IConnectionStringValidator, ConnectionStringValidator>()
            .AddGenocsRepositories<ApplicationDbContext>(typeof(Brand).Assembly);
    }

    internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
    {
        return dbProvider.ToLowerInvariant() switch
        {
#if PostgreSQL
            DbProviderKeys.Npgsql => builder.UseNpgsql(connectionString, e =>
                                 e.MigrationsAssembly("Migrators.PostgreSQL")),
#endif
#if MSSQL
            DbProviderKeys.SqlServer => builder.UseSqlServer(connectionString, e =>
                                 e.MigrationsAssembly("Migrators.MSSQL")),
#endif
#if MySQL
            DbProviderKeys.MySql => builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), e =>
                                 e.MigrationsAssembly("Migrators.MySQL")
                                  .SchemaBehavior(MySqlSchemaBehavior.Ignore)),
#endif
#if Oracle
            DbProviderKeys.Oracle => builder.UseOracle(connectionString, e =>
                                 e.MigrationsAssembly("Migrators.Oracle")),
#endif
#if SQLite
            DbProviderKeys.SqLite => builder.UseSqlite(connectionString, e =>
                                 e.MigrationsAssembly("Migrators.SqLite")),
#endif
            _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
        };
    }
}