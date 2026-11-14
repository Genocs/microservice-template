using Genocs.Core.Domain.Entities;

namespace Genocs.Microservice.Template.Domain.Common.Contracts;

public abstract class AuditableEntity : AuditableEntity<DefaultIdType>;

public abstract class AuditableEntity<T> : AggregateRoot<T>, IAuditableEntity, ISoftDelete
{
    public DefaultIdType CreatedBy { get; set; }
    public DateTime CreatedOn { get; private set; }
    public DefaultIdType LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public DefaultIdType? DeletedBy { get; set; }

    protected AuditableEntity()
    {
        CreatedOn = DateTime.UtcNow;
        if (typeof(T) == typeof(DefaultIdType))
        {
            Id = (T)(object)Guid.NewGuid();
        }
    }
}
