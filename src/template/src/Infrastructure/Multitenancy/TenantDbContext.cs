using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Genocs.Microservice.Template.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Microservice.Template.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<GNXTenantInfo>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
#if PostgreSQL
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GNXTenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
    }
}