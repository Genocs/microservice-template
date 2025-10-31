namespace Genocs.Microservice.Template.Domain.Catalog;

public class Brand(string name, string? description) : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = name;
    public string? Description { get; private set; } = description;

    public Brand Update(string? name, string? description)
    {
        if (name is not null && Name?.Equals(name) is not true) Name = name;
        if (description is not null && Description?.Equals(description) is not true) Description = description;
        return this;
    }
}