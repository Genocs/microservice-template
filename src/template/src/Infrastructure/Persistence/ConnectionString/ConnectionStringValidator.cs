using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Infrastructure.Common;
#if MSSQL
using Microsoft.Data.SqlClient;
#endif
#if SQLite
using Microsoft.Data.Sqlite;
#endif
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
#if MySQL
using MySqlConnector;
#endif
#if PostgreSQL
using Npgsql;
#endif

namespace Genocs.Microservice.Template.Infrastructure.Persistence.ConnectionString;

internal class ConnectionStringValidator : IConnectionStringValidator
{
    private readonly DatabaseSettings _dbSettings;
    private readonly ILogger<ConnectionStringValidator> _logger;

    public ConnectionStringValidator(IOptions<DatabaseSettings> dbSettings, ILogger<ConnectionStringValidator> logger)
    {
        _dbSettings = dbSettings.Value;
        _logger = logger;
    }

    public bool TryValidate(string connectionString, string? dbProvider = null)
    {
        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = _dbSettings.DBProvider;
        }

        try
        {
            switch (dbProvider?.ToLowerInvariant())
            {
#if PostgreSQL
                case DbProviderKeys.Npgsql:
                    var postgresqlcs = new NpgsqlConnectionStringBuilder(connectionString);
                    break;
#endif
#if MySQL
                case DbProviderKeys.MySql:
                    var mysqlcs = new MySqlConnectionStringBuilder(connectionString);
                    break;
#endif
#if MSSQL
                case DbProviderKeys.SqlServer:
                    var mssqlcs = new SqlConnectionStringBuilder(connectionString);
                    break;
#endif
#if SQLite
                case DbProviderKeys.SqLite:
                    var sqlite = new SqliteConnection(connectionString);
                    break;
#endif
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection String Validation Exception : {ex.Message}");
            return false;
        }
    }
}