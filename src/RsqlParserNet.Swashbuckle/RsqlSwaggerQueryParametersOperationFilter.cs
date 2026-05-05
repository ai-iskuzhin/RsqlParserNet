using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.OpenApi;
using RsqlParserNet.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RsqlParserNet.Swashbuckle;

/// <summary>
/// Documents RSQL query parameters for endpoints marked with <see cref="RsqlSwaggerQueryMetadata"/>.
/// </summary>
public sealed class RsqlSwaggerQueryParametersOperationFilter : IOperationFilter
{
    /// <summary>
    /// Applies RSQL query parameter documentation to marked operations.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to update.</param>
    /// <param name="context">The Swashbuckle operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var metadata = GetMetadata(context.ApiDescription.ActionDescriptor)?.LastOrDefault();
        if (metadata is null)
        {
            return;
        }

        RsqlOpenApiOperationDocumenter.Apply(operation, metadata.Options);
    }

    private static IEnumerable<RsqlSwaggerQueryMetadata> GetMetadata(ActionDescriptor actionDescriptor)
    {
        return actionDescriptor.EndpointMetadata.OfType<RsqlSwaggerQueryMetadata>();
    }
}
