namespace Genocs.Microservice.Template.Domain.Common.Contracts;

/// <summary>
/// This interface is used to mark entities as soft deletable.
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// When this entity was deleted.
    /// </summary>
    DateTime? DeletedOn { get; set; }

    /// <summary>
    /// Who deleted this entity.
    /// </summary>
    DefaultIdType? DeletedBy { get; set; }
}