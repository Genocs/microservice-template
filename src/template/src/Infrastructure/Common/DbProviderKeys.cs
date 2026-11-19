namespace Genocs.Microservice.Template.Infrastructure.Common;

internal class DbProviderKeys
{
#if PostgreSQL
    public const string Npgsql = "postgresql";
#endif
#if MSSQL
    public const string SqlServer = "mssql";
#endif
#if MySQL
    public const string MySql = "mysql";
#endif
#if Oracle
    public const string Oracle = "oracle";
#endif
#if SQLite
    public const string SqLite = "sqlite";
#endif
}
