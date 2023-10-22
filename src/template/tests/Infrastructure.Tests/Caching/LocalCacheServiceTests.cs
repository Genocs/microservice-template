using Genocs.Microservice.Template.Infrastructure.Caching;

namespace Genocs.Microservice.Template.Infrastructure.Tests.Caching;

public class LocalCacheServiceTests : CacheServiceTests
{
    public LocalCacheServiceTests(LocalCacheService cacheService)
        : base(cacheService)
    {
    }
}