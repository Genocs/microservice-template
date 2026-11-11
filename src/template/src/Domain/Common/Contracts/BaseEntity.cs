using System.ComponentModel.DataAnnotations.Schema;
using Genocs.Common.CQRS.Events;
using Genocs.Core.Domain.Entities;
using IGeneratesDomainEvents = Genocs.Common.Domain.Entities.IGeneratesDomainEvents;

namespace Genocs.Microservice.Template.Domain.Common.Contracts;

public abstract class BaseEntity : BaseEntity<DefaultIdType>
{
    protected BaseEntity()
        => Id = Guid.NewGuid();
}

public abstract class BaseEntity<TId> : IEntity<TId>, IGeneratesDomainEvents
{
    public TId Id { get; protected set; } = default!;

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = [];

    IReadOnlyCollection<IEvent> IGeneratesDomainEvents.DomainEvents => DomainEvents;

    public void AddDomainEvent(IEvent @event)
    {
        if (@event is not DomainEvent typedEvent)
        {
            throw new ArgumentException($"Domain event must derive from {nameof(DomainEvent)}.", nameof(@event));
        }

        DomainEvents.Add(typedEvent);
    }

    public void ClearDomainEvents() => DomainEvents.Clear();
}