using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Infrastructure.Common;
#if MSSQL
using Microsoft.Data.SqlClient;
#endif
#if SQLite
using Microsoft.Data.Sqlite;
#endif
using Microsoft.Extensions.Options;
#if MySQL
using MySqlConnector;
#endif
#if PostgreSQL
using Npgsql;
#endif
#if Oracle
using Oracle.ManagedDataAccess.Client;
#endif

namespace Genocs.Microservice.Template.Infrastructure.Persistence.ConnectionString;

public class ConnectionStringSecurer : IConnectionStringSecurer
{
    private const string HiddenValueDefault = "*******";
    private readonly DatabaseSettings _dbSettings;

    public ConnectionStringSecurer(IOptions<DatabaseSettings> dbSettings) =>
        _dbSettings = dbSettings.Value;

    public string? MakeSecure(string? connectionString, string? dbProvider)
    {
        if (connectionString == null || string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = _dbSettings.DBProvider;
        }

        return dbProvider?.ToLower() switch
        {
#if PostgreSQL
            DbProviderKeys.Npgsql => MakeSecureNpgsqlConnectionString(connectionString),
#endif
#if MSSQL
            DbProviderKeys.SqlServer => MakeSecureSqlConnectionString(connectionString),
#endif
#if MySQL
            DbProviderKeys.MySql => MakeSecureMySqlConnectionString(connectionString),
#endif
#if SQLite
            DbProviderKeys.SqLite => MakeSecureSqLiteConnectionString(connectionString),
#endif
#if Oracle
            DbProviderKeys.Oracle => MakeSecureOracleConnectionString(connectionString),
#endif
            _ => connectionString
        };
    }

#if Oracle
    private static string MakeSecureOracleConnectionString(string connectionString)
    {
        var builder = new OracleConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }
#endif
#if MySQL
    private static string MakeSecureMySqlConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }
#endif
#if MSSQL
    private static string MakeSecureSqlConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) || !builder.IntegratedSecurity)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID) || !builder.IntegratedSecurity)
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }
#endif
#if SQLite
    private static string MakeSecureSqLiteConnectionString(string connectionString)
    {
        var builder = new SqliteConnection(connectionString);

        return builder.ToString();
    }
#endif
#if PostgreSQL
    private static string MakeSecureNpgsqlConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) /* || !builder.IntegratedSecurity*/)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.Username) /* || !builder.IntegratedSecurity */)
        {
            builder.Username = HiddenValueDefault;
        }

        return builder.ToString();
    }
#endif
}
