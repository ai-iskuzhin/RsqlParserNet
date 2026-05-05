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

    /// <summary>
    /// Registers RSQL page query binding options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional page query binding option configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddRsqlPageQuery(
        this IServiceCollection services,
        Action<RsqlPageQueryOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is null)
        {
            services.AddOptions<RsqlPageQueryOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Registers RSQL sort query binding options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional sort query binding option configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddRsqlSortQuery(
        this IServiceCollection services,
        Action<RsqlSortQueryOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configure is null)
        {
            services.AddOptions<RsqlSortQueryOptions>();
        }
        else
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Registers RSQL filter, sort, and page query binding options.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureFilter">Optional filter query binding option configuration.</param>
    /// <param name="configureSort">Optional sort query binding option configuration.</param>
    /// <param name="configurePage">Optional page query binding option configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddRsqlQueryRequest(
        this IServiceCollection services,
        Action<RsqlQueryFilterOptions>? configureFilter = null,
        Action<RsqlSortQueryOptions>? configureSort = null,
        Action<RsqlPageQueryOptions>? configurePage = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRsqlQueryFilter(configureFilter);
        services.AddRsqlSortQuery(configureSort);
        services.AddRsqlPageQuery(configurePage);

        return services;
    }
}
