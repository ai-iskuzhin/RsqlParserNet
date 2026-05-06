using NJsonSchema;
using NSwag;
using RsqlParserNet.OpenApi;

namespace RsqlParserNet.NSwag;

/// <summary>
/// Adds RSQL query parameter documentation to NSwag operations.
/// </summary>
public static class RsqlNSwagOperationDocumenter
{
    /// <summary>
    /// Adds configured RSQL query parameters to an NSwag operation.
    /// </summary>
    /// <param name="operation">The NSwag operation to update.</param>
    /// <param name="options">Optional query documentation options.</param>
    public static void Apply(OpenApiOperation operation, RsqlOpenApiQueryOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var effectiveOptions = options ?? new RsqlOpenApiQueryOptions();

        if (effectiveOptions.IncludeFilter)
        {
            AddParameter(
                operation,
                effectiveOptions.FilterParameterName,
                effectiveOptions.FilterDescription,
                JsonObjectType.String,
                format: null);
        }

        if (effectiveOptions.IncludeSort)
        {
            AddParameter(
                operation,
                effectiveOptions.SortParameterName,
                effectiveOptions.SortDescription,
                JsonObjectType.String,
                format: null);
        }

        if (effectiveOptions.IncludePagination)
        {
            AddParameter(
                operation,
                effectiveOptions.PageParameterName,
                effectiveOptions.PageDescription,
                JsonObjectType.Integer,
                format: "int32");
            AddParameter(
                operation,
                effectiveOptions.PageSizeParameterName,
                effectiveOptions.PageSizeDescription,
                JsonObjectType.Integer,
                format: "int32");
        }
    }

    private static void AddParameter(
        OpenApiOperation operation,
        string name,
        string description,
        JsonObjectType schemaType,
        string? format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (operation.Parameters.Any(parameter =>
            string.Equals(parameter.Name, name, StringComparison.Ordinal)
            && parameter.Kind == OpenApiParameterKind.Query))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = name,
            Kind = OpenApiParameterKind.Query,
            IsRequired = false,
            Description = description,
            Schema = new JsonSchema
            {
                Type = schemaType,
                Format = format
            }
        });
    }
}
