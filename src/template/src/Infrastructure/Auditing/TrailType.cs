namespace Genocs.Microservice.Template.Infrastructure.Auditing;

/// <summary>
/// The type of trail.
/// </summary>
public enum TrailType : byte
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
}