using AgenticEngineeringSystem.Core.UrlShortener;

using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class CreateShortUrlRequestSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != typeof(CreateShortUrlRequest))
            return;

        if (schema is not OpenApiSchema concrete)
            return;

        if (!concrete.Properties!.TryGetValue("expiresAt", out var property))
            return;

        if (property is OpenApiSchema propertySchema)
        {
            propertySchema.Examples ??= [];
            propertySchema.Examples.Add(DateTimeOffset.Now.AddDays(1));
        }
    }
}