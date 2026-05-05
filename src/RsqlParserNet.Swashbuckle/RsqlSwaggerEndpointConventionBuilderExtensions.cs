using Microsoft.AspNetCore.Builder;
using RsqlParserNet.OpenApi;

namespace RsqlParserNet.Swashbuckle;

/// <summary>
/// Provides endpoint builder helpers for Swashbuckle RSQL query parameter documentation.
/// </summary>
public static class RsqlSwaggerEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Marks an endpoint so the RSQL Swashbuckle operation filter documents its query parameters.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="configure">Optional query parameter documentation configuration.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static TBuilder WithRsqlSwaggerQueryParameters<TBuilder>(
        this TBuilder builder,
        Action<RsqlOpenApiQueryOptions>? configure = null)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new RsqlOpenApiQueryOptions();
        configure?.Invoke(options);

        return builder.WithMetadata(new RsqlSwaggerQueryMetadata(options));
    }
}
