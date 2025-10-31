namespace Genocs.Microservice.Template.Domain.Common.Contracts;

/// <summary>
/// Generic interface for entities.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// The domain events that are raised by the entity.
    /// </summary>
    List<DomainEvent> DomainEvents { get; }
}

/// <summary>
/// Interface to define an entity of type TId.
/// </summary>
/// <typeparam name="TId">The entity key type.</typeparam>
public interface IEntity<TId> : IEntity
{
    TId Id { get; }
}