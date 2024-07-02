using Finbuckle.MultiTenant;
using Genocs.Microservice.Template.Shared.Multitenancy;

namespace Genocs.Microservice.Template.Infrastructure.Multitenancy;

public class GNXTenantInfo : TenantInfo
{
    public GNXTenantInfo()
    {
    }

    public GNXTenantInfo(string id, string name, string? connectionString, string adminEmail, string? issuer = null)
    {
        Id = id;
        Identifier = id;
        Name = name;
        ConnectionString = connectionString ?? string.Empty;
        AdminEmail = adminEmail;
        IsActive = true;
        Issuer = issuer;

        // Add Default 1 Month Validity for all new tenants. Something like a DEMO period for tenants.
        ValidUpTo = DateTime.UtcNow.AddMonths(1);
    }

    public string ConnectionString { get; set; } = default!;

    public string AdminEmail { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime ValidUpTo { get; private set; }

    /// <summary>
    /// Used by AzureAd Authorization to store the AzureAd Tenant Issuer to map against.
    /// </summary>
    public string? Issuer { get; set; }

    public void AddValidity(int months) =>
        ValidUpTo = ValidUpTo.AddMonths(months);

    public void SetValidity(in DateTime validTill) =>
        ValidUpTo = ValidUpTo < validTill
            ? validTill
            : throw new Exception("Subscription cannot be backdated.");

    public void Activate()
    {
        if (Id == MultitenancyConstants.Root.Id)
        {
            throw new InvalidOperationException("Invalid Tenant");
        }

        IsActive = true;
    }

    public void Deactivate()
    {
        if (Id == MultitenancyConstants.Root.Id)
        {
            throw new InvalidOperationException("Invalid Tenant");
        }

        IsActive = false;
    }
}