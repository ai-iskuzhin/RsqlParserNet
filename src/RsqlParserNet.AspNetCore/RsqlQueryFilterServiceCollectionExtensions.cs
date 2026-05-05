using Microsoft.Extensions.DependencyInjection;

namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Provides service registration helpers for ASP.NET Core RSQL query binding.
/// </summary>
public static class RsqlQueryFilterServiceCollectionExtensions
{
    /// <summary>
    /// Registers RSQL query filter binding options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional binding option configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddRsqlQueryFilter(
        this IServiceCollection services,
        Action<RsqlQueryFilterOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is null)
        {
            services.AddOptions<RsqlQueryFilterOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        return services;
    }
}
