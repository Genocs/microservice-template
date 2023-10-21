namespace Genocs.Microservice.Template.Domain.Common.Contracts;

/// <summary>
/// Apply this marker interface only to aggregate root entities
/// Repositories will only work with aggregate roots, not their children.
/// </summary>
public interface IAggregateRoot : IEntity
{
}