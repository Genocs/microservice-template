﻿using Genocs.Microservice.Template.Infrastructure.Multitenancy;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Initialization;

internal interface IDatabaseInitializer
{
    Task InitializeDatabasesAsync(CancellationToken cancellationToken);
    Task InitializeApplicationDbForTenantAsync(GNXTenantInfo tenant, CancellationToken cancellationToken);
}