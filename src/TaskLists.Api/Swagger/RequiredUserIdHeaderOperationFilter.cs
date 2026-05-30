using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskLists.Api.Swagger;

public sealed class RequiredUserIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.RelativePath == "health")
        {
            return;
        }

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-Id",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Temporary current-user identifier for this assignment.",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
