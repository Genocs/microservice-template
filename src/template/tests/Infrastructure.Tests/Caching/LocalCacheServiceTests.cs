using Genocs.Microservice.Template.Infrastructure.Caching;

namespace Genocs.Microservice.Template.Infrastructure.Tests.Caching;

public class LocalCacheServiceTests(LocalCacheService cacheService) : CacheServiceTests(cacheService);
