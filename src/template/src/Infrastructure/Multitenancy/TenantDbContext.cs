using Finbuckle.MultiTenant.Stores;
using Genocs.Microservice.Template.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Microservice.Template.Infrastructure.Multitenancy;

public class TenantDbContext : EFCoreStoreDbContext<GNXTenantInfo>
{
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<GNXTenantInfo>().ToTable("Tenants", SchemaNames.MultiTenancy);
    }
}