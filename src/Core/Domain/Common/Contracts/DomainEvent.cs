using Genocs.Microservice.Shared.Events;

namespace Genocs.Microservice.Domain.Common.Contracts;

public abstract class DomainEvent : IEvent
{
    public DateTime TriggeredOn { get; protected set; } = DateTime.UtcNow;
}