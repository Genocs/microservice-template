using Finbuckle.MultiTenant.Abstractions;
using Genocs.Microservice.Template.Application.Common.Exceptions;
using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Application.Multitenancy;
using Genocs.Microservice.Template.Infrastructure.Persistence;
using Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;
using Mapster;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Genocs.Microservice.Template.Infrastructure.Multitenancy;

internal class TenantService : ITenantService
{
    private readonly IMultiTenantStore<GNXTenantInfo> _tenantStore;
    private readonly IConnectionStringSecurer _csSecurer;
    private readonly IDatabaseInitializer _dbInitializer;
    private readonly IStringLocalizer _t;
    private readonly DatabaseSettings _dbSettings;

    public TenantService(
                        IMultiTenantStore<GNXTenantInfo> tenantStore,
                        IConnectionStringSecurer csSecurer,
                        IDatabaseInitializer dbInitializer,
                        IStringLocalizer<TenantService> localizer,
                        IOptions<DatabaseSettings> dbSettings)
    {
        if (dbSettings is null)
        {
            throw new ArgumentNullException(nameof(dbSettings));
        }

        _tenantStore = tenantStore ?? throw new ArgumentNullException(nameof(tenantStore));
        _csSecurer = csSecurer ?? throw new ArgumentNullException(nameof(csSecurer));
        _dbInitializer = dbInitializer ?? throw new ArgumentNullException(nameof(dbInitializer));
        _t = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _dbSettings = dbSettings.Value;

        if (_dbSettings is null)
        {
            throw new ArgumentNullException(nameof(_dbSettings));
        }
    }

    public async Task<List<TenantDto>> GetAllAsync()
    {
        var tenants = (await _tenantStore.GetAllAsync()).Adapt<List<TenantDto>>();
        tenants.ForEach(t => t.ConnectionString = _csSecurer.MakeSecure(t.ConnectionString));
        return tenants;
    }

    public async Task<bool> ExistsWithIdAsync(string id) =>
        await _tenantStore.TryGetAsync(id) is not null;

    public async Task<bool> ExistsWithNameAsync(string name) =>
        (await _tenantStore.GetAllAsync()).Any(t => t.Name == name);

    public async Task<TenantDto> GetByIdAsync(string id) =>
        (await GetTenantInfoAsync(id))
            .Adapt<TenantDto>();

    public async Task<string> CreateAsync(CreateTenantRequest request, CancellationToken cancellationToken)
    {
        if (request.ConnectionString?.Trim() == _dbSettings.ConnectionString.Trim()) request.ConnectionString = string.Empty;

        var tenant = new GNXTenantInfo(request.Id, request.Name, request.ConnectionString, request.AdminEmail, request.Issuer);
        await _tenantStore.TryAddAsync(tenant);

        // TODO: run this in a hangfire job? will then have to send mail when it's ready or not
        try
        {
            await _dbInitializer.InitializeApplicationDbForTenantAsync(tenant, cancellationToken);
        }
        catch
        {
            await _tenantStore.TryRemoveAsync(request.Id);
            throw;
        }

        return tenant.Id;
    }

    public async Task<string> ActivateAsync(string id)
    {
        var tenant = await GetTenantInfoAsync(id);

        if (tenant.IsActive)
        {
            throw new ConflictException(_t["Tenant is already Activated."]);
        }

        tenant.Activate();

        await _tenantStore.TryUpdateAsync(tenant);

        return _t["Tenant {0} is now Activated.", id];
    }

    public async Task<string> DeactivateAsync(string id)
    {
        var tenant = await GetTenantInfoAsync(id);
        if (!tenant.IsActive)
        {
            throw new ConflictException(_t["Tenant is already Deactivated."]);
        }

        tenant.Deactivate();
        await _tenantStore.TryUpdateAsync(tenant);
        return _t["Tenant {0} is now Deactivated.", id];
    }

    public async Task<string> UpdateSubscriptionAsync(string id, DateTime extendedExpiryDate)
    {
        var tenant = await GetTenantInfoAsync(id);
        tenant.SetValidity(extendedExpiryDate);
        await _tenantStore.TryUpdateAsync(tenant);
        return _t["Tenant {0}'s Subscription Upgraded. Now Valid till {1}.", id, tenant.ValidUpTo];
    }

    private async Task<GNXTenantInfo> GetTenantInfoAsync(string id) =>
        await _tenantStore.TryGetAsync(id)
            ?? throw new NotFoundException(_t["{0} {1} Not Found.", typeof(GNXTenantInfo).Name, id]);
}