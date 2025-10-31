using Genocs.Microservice.Template.Infrastructure.Caching;

namespace Genocs.Microservice.Template.Infrastructure.Tests.Caching;

public class DistributedCacheServiceTests : CacheServiceTests
{
    public DistributedCacheServiceTests(DistributedCacheService cacheService)
        : base(cacheService)
    {
    }
}