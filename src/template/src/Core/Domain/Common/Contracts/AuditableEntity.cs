namespace Genocs.Microservice.Template.Domain.Common.Contracts;

public abstract class AuditableEntity : AuditableEntity<DefaultIdType>;

public abstract class AuditableEntity<T> : BaseEntity<T>, IAuditableEntity, ISoftDelete
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
    }
}