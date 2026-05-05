using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;

namespace RsqlParserNet.OpenApi;

/// <summary>
/// Provides endpoint builder helpers for documenting RSQL query parameters.
/// </summary>
public static class RsqlOpenApiEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds RSQL filter, sort, page, and page size query parameter documentation to an endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="configure">Optional query parameter documentation configuration.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static TBuilder WithRsqlQueryParameters<TBuilder>(
        this TBuilder builder,
        Action<RsqlOpenApiQueryOptions>? configure = null)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new RsqlOpenApiQueryOptions();
        configure?.Invoke(options);

        return builder.AddOpenApiOperationTransformer((operation, _, _) =>
        {
            RsqlOpenApiOperationDocumenter.Apply(operation, options);
            return Task.CompletedTask;
        });
    }
}
