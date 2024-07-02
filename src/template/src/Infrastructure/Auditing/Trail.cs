using Genocs.Microservice.Template.Domain.Common.Contracts;

namespace Genocs.Microservice.Template.Infrastructure.Auditing;

/// <summary>
/// Entity to store the trail of changes.
/// </summary>
public class Trail : BaseEntity
{
    /// <summary>
    /// The user id.
    /// </summary>
    public DefaultIdType UserId { get; set; }

    /// <summary>
    /// The type of trail.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The table name.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// The date and time of the change.
    /// </summary>
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}