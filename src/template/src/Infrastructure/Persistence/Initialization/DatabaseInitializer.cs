using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;
using Genocs.Microservice.Template.Shared.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;

internal class DatabaseInitializer : IDatabaseInitializer
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(TenantDbContext tenantDbContext, IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
    {
        _tenantDbContext = tenantDbContext ?? throw new ArgumentNullException(nameof(tenantDbContext));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        await InitializeTenantDbAsync(cancellationToken);

        foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(cancellationToken))
        {
            await InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }

        _logger.LogInformation("For documentations and guides, visit https://genocs-blog.netlify.app");
        _logger.LogInformation("To Sponsor this project, visit https://opencollective.com/genocs");
    }

    public async Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken)
    {
        // First create a new scope
        using var scope = _serviceProvider.CreateScope();

        // Then set current tenant so the right ConnectionString is used
        _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<GNXTenantInfo>()
            {
                TenantInfo = tenant
            };

        // Then run the initialization in the new scope
        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>()
            .InitializeAsync(cancellationToken);
    }

    private async Task InitializeTenantDbAsync(CancellationToken cancellationToken)
    {
        if (_tenantDbContext.Database.GetPendingMigrations().Any())
        {
            _logger.LogInformation("Applying Root Migrations.");
            await _tenantDbContext.Database.MigrateAsync(cancellationToken);
        }

        await SeedRootTenantAsync(cancellationToken);
    }

    private async Task SeedRootTenantAsync(CancellationToken cancellationToken)
    {
        if (await _tenantDbContext.TenantInfo.FindAsync(new object?[] { MultitenancyConstants.Root.Id }, cancellationToken: cancellationToken) is null)
        {
            var rootTenant = new GNXTenantInfo(
                                                MultitenancyConstants.Root.Id,
                                                MultitenancyConstants.Root.Name,
                                                string.Empty,
                                                MultitenancyConstants.Root.EmailAddress);

            rootTenant.SetValidity(DateTime.UtcNow.AddYears(1));

            _tenantDbContext.TenantInfo.Add(rootTenant);

            await _tenantDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}