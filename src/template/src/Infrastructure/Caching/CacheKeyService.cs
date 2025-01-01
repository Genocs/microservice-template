using Finbuckle.MultiTenant.Abstractions;
using Genocs.Microservice.Template.Application.Common.Caching;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;

namespace Genocs.Microservice.Template.Infrastructure.Caching;

public class CacheKeyService(IMultiTenantContextAccessor<GNXTenantInfo> multiTenantContextAccessor) : ICacheKeyService
{
    private readonly ITenantInfo? _currentTenant = multiTenantContextAccessor?.MultiTenantContext?.TenantInfo;

    public string GetCacheKey(string name, object id, bool includeTenantId = true)
    {
        string tenantId = includeTenantId
            ? _currentTenant?.Id ?? throw new InvalidOperationException("GetCacheKey: includeTenantId set to true and no ITenantInfo available.")
            : "GLOBAL";
        return $"{tenantId}-{name}-{id}";
    }
}