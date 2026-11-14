using System.Data;
using Dapper;
using Finbuckle.MultiTenant;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Repository;

public class DapperRepository : IDapperRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DapperRepository(ApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
        => (await _dbContext.Connection.QueryAsync<T>(
                new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken)))
            .AsList();

    public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (_dbContext.Model.GetMultiTenantEntityTypes().Any(t => t.ClrType == typeof(T)))
        {
            sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
        }

        T? result = await _dbContext.Connection.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));

        return result!;
    }

    public Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        if (_dbContext.Model.GetMultiTenantEntityTypes().Any(t => t.ClrType == typeof(T)))
        {
            sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
        }

        return _dbContext.Connection.QuerySingleAsync<T>(
            new CommandDefinition(sql, param, transaction, cancellationToken: cancellationToken));
    }
}
