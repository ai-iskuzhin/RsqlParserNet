using Microsoft.OpenApi;
using RsqlParserNet.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RsqlParserNet.Swashbuckle;

/// <summary>
/// Documents RSQL query parameters on every Swashbuckle-generated operation.
/// </summary>
public sealed class RsqlSwaggerAllQueryParametersOperationFilter : IOperationFilter
{
    private readonly RsqlOpenApiQueryOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlSwaggerAllQueryParametersOperationFilter"/> class.
    /// </summary>
    public RsqlSwaggerAllQueryParametersOperationFilter()
        : this(new RsqlOpenApiQueryOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlSwaggerAllQueryParametersOperationFilter"/> class.
    /// </summary>
    /// <param name="options">The query parameter documentation options.</param>
    public RsqlSwaggerAllQueryParametersOperationFilter(RsqlOpenApiQueryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <summary>
    /// Applies RSQL query parameter documentation to every operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to update.</param>
    /// <param name="context">The Swashbuckle operation filter context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        RsqlOpenApiOperationDocumenter.Apply(operation, _options);
    }
}
