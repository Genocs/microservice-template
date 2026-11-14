using Genocs.Core.Domain.Entities;

namespace Genocs.Microservice.Template.Domain.Identity;

public abstract class ApplicationRoleEvent : DomainEvent
{
    public string RoleId { get; set; } = default!;
    public string RoleName { get; set; } = default!;
    protected ApplicationRoleEvent(string roleId, string roleName)
        => (RoleId, RoleName) = (roleId, roleName);
}

public class ApplicationRoleCreatedEvent(string roleId, string roleName) : ApplicationRoleEvent(roleId, roleName);

public class ApplicationRoleUpdatedEvent(string roleId, string roleName, bool permissionsUpdated = false) : ApplicationRoleEvent(roleId, roleName)
{
    public bool PermissionsUpdated { get; set; } = permissionsUpdated;
}

public class ApplicationRoleDeletedEvent(string roleId, string roleName) : ApplicationRoleEvent(roleId, roleName)
{
    public bool PermissionsUpdated { get; set; }
}