using Genocs.Core.Domain.Entities;

namespace Genocs.Microservice.Template.Domain.Common.Contracts;

public abstract class BaseEntity : BaseEntity<DefaultIdType>
{
    protected BaseEntity()
        => Id = Guid.NewGuid();
}

public abstract class BaseEntity<TId> : Entity<TId>
{
}
