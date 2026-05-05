using Microsoft.Extensions.DependencyInjection;
using RsqlParserNet.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RsqlParserNet.Swashbuckle;

/// <summary>
/// Provides Swashbuckle registration helpers for RSQL query parameter documentation.
/// </summary>
public static class RsqlSwaggerGenOptionsExtensions
{
    /// <summary>
    /// Registers an operation filter that documents endpoints marked with <c>WithRsqlSwaggerQueryParameters()</c>.
    /// </summary>
    /// <param name="options">The Swashbuckle options.</param>
    public static void AddRsqlQueryParametersOperationFilter(this SwaggerGenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.OperationFilter<RsqlSwaggerQueryParametersOperationFilter>();
    }

    /// <summary>
    /// Registers an operation filter that documents RSQL query parameters on every operation.
    /// </summary>
    /// <param name="options">The Swashbuckle options.</param>
    /// <param name="configure">Optional query parameter documentation configuration.</param>
    public static void AddRsqlQueryParametersToAllOperations(
        this SwaggerGenOptions options,
        Action<RsqlOpenApiQueryOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var queryOptions = new RsqlOpenApiQueryOptions();
        configure?.Invoke(queryOptions);

        options.OperationFilter<RsqlSwaggerAllQueryParametersOperationFilter>(queryOptions);
    }
}
