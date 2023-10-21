using Microsoft.AspNetCore.Identity;

namespace Genocs.Microservice.Template.Infrastructure.Identity;

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public string? CreatedBy { get; init; }
    public DateTime CreatedOn { get; init; }
}