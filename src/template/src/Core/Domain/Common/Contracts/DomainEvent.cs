using Genocs.Microservice.Template.Shared.Events;

namespace Genocs.Microservice.Template.Domain.Common.Contracts;

public abstract class DomainEvent : IEvent
{
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}