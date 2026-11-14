using Genocs.Core.Domain.Entities;

namespace Genocs.Microservice.Template.Domain.Identity;

public abstract class ApplicationUserEvent(string userId) : DomainEvent
{
    public string UserId { get; set; } = userId;
}

public class ApplicationUserCreatedEvent(string userId) : ApplicationUserEvent(userId);

public class ApplicationUserUpdatedEvent(string userId, bool rolesUpdated = false) : ApplicationUserEvent(userId)
{
    public bool RolesUpdated { get; set; } = rolesUpdated;
}