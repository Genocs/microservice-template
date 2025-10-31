namespace Genocs.Microservice.Template.Domain.Common.Contracts;

/// <summary>
/// Interface for entities that need to be audited.
/// </summary>
public interface IAuditableEntity
{
    public DefaultIdType CreatedBy { get; set; }
    public DateTime CreatedOn { get; }
    public DefaultIdType LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}