using Finbuckle.MultiTenant.Abstractions;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;
using Genocs.Microservice.Template.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;

internal class ApplicationDbInitializer
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITenantInfo _currentTenant;
    private readonly ApplicationDbSeeder _dbSeeder;
    private readonly ILogger<ApplicationDbInitializer> _logger;

    public ApplicationDbInitializer(ApplicationDbContext dbContext, IMultiTenantContextAccessor<GNXTenantInfo> multiTenantContextAccessor, ApplicationDbSeeder dbSeeder, ILogger<ApplicationDbInitializer> logger)
    {
        if (multiTenantContextAccessor is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor));
        }

        if (multiTenantContextAccessor.MultiTenantContext is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor.MultiTenantContext));
        }

        if (multiTenantContextAccessor?.MultiTenantContext?.TenantInfo is null)
        {
            throw new ArgumentNullException(nameof(multiTenantContextAccessor.MultiTenantContext.TenantInfo));
        }

        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentTenant = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
        _dbSeeder = dbSeeder ?? throw new ArgumentNullException(nameof(dbSeeder));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.GetMigrations().Any())
        {
            if ((await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                _logger.LogInformation("Applying Migrations for '{tenantId}' tenant.", _currentTenant?.Id);
                await _dbContext.Database.MigrateAsync(cancellationToken);
            }

            if (await _dbContext.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogInformation("Connection to {tenantId}'s Database Succeeded.", _currentTenant?.Id);

                await _dbSeeder.SeedDatabaseAsync(_dbContext, cancellationToken);
            }
        }
    }
}
