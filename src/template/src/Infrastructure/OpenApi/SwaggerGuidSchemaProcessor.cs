using NJsonSchema;
using NJsonSchema.Generation;

namespace Genocs.Microservice.Template.Infrastructure.OpenApi;
public class SwaggerGuidSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var type = context.ContextualType;
        var schema = context.Schema;

        // Check if the type is a Guid
        if (type == typeof(DefaultIdType))
        {
            schema.Type = JsonObjectType.String;
            schema.Format = "uuid";
        }
    }
}