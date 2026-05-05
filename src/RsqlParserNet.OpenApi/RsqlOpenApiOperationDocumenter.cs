using Microsoft.OpenApi;

namespace RsqlParserNet.OpenApi;

/// <summary>
/// Adds RSQL query parameter documentation to OpenAPI operations.
/// </summary>
public static class RsqlOpenApiOperationDocumenter
{
    /// <summary>
    /// Adds configured RSQL query parameters to an OpenAPI operation.
    /// </summary>
    /// <param name="operation">The OpenAPI operation to update.</param>
    /// <param name="options">Optional query documentation options.</param>
    public static void Apply(OpenApiOperation operation, RsqlOpenApiQueryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var effectiveOptions = options ?? new RsqlOpenApiQueryOptions();
        operation.Parameters ??= [];

        if (effectiveOptions.IncludeFilter)
        {
            AddParameter(
                operation,
                effectiveOptions.FilterParameterName,
                effectiveOptions.FilterDescription,
                JsonSchemaType.String,
                format: null);
        }

        if (effectiveOptions.IncludeSort)
        {
            AddParameter(
                operation,
                effectiveOptions.SortParameterName,
                effectiveOptions.SortDescription,
                JsonSchemaType.String,
                format: null);
        }

        if (effectiveOptions.IncludePagination)
        {
            AddParameter(
                operation,
                effectiveOptions.PageParameterName,
                effectiveOptions.PageDescription,
                JsonSchemaType.Integer,
                format: "int32");
            AddParameter(
                operation,
                effectiveOptions.PageSizeParameterName,
                effectiveOptions.PageSizeDescription,
                JsonSchemaType.Integer,
                format: "int32");
        }
    }

    private static void AddParameter(
        OpenApiOperation operation,
        string name,
        string description,
        JsonSchemaType schemaType,
        string? format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var parameters = operation.Parameters ??= [];
        if (parameters.Any(parameter =>
            string.Equals(parameter.Name, name, StringComparison.Ordinal)
            && parameter.In == ParameterLocation.Query))
        {
            return;
        }

        parameters.Add(new OpenApiParameter
        {
            Name = name,
            In = ParameterLocation.Query,
            Required = false,
            Description = description,
            Schema = new OpenApiSchema
            {
                Type = schemaType,
                Format = format
            }
        });
    }
}
