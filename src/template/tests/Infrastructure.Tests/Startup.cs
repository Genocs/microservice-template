﻿using Genocs.Microservice.Template.Application.Common.Interfaces;
using Genocs.Microservice.Template.Application.Common.Persistence;
using Genocs.Microservice.Template.Infrastructure.Caching;
using Genocs.Microservice.Template.Infrastructure.Common.Services;
using Genocs.Microservice.Template.Infrastructure.Localization;
using Genocs.Microservice.Template.Infrastructure.Persistence.ConnectionString;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Genocs.Microservice.Template.Infrastructure.Tests;

public class Startup
{
    public static void ConfigureHost(IHostBuilder host) =>
        host.ConfigureHostConfiguration(config => config.AddJsonFile("appsettings.json"));

    public static void ConfigureServices(IServiceCollection services, HostBuilderContext context) =>
        services
            .AddTransient<IMemoryCache, MemoryCache>()
            .AddTransient<LocalCacheService>()
            .AddTransient<IDistributedCache, MemoryDistributedCache>()
            .AddTransient<ISerializerService, NewtonSoftService>()
            .AddTransient<DistributedCacheService>()
            .AddPOLocalization(context.Configuration)
            .AddTransient<IConnectionStringSecurer, ConnectionStringSecurer>();
}