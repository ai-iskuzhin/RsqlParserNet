using Microsoft.EntityFrameworkCore;
using RsqlParserNet.Linq;

namespace RsqlParserNet.EntityFrameworkCore;

/// <summary>
/// Provides Entity Framework Core execution helpers for RSQL queryables.
/// </summary>
public static class RsqlEntityFrameworkQueryableExtensions
{
    /// <summary>
    /// Executes a paged query asynchronously and returns items with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="page">The page request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result.</returns>
    public static async Task<RsqlPagedResult<T>> ToRsqlPageAsync<T>(
        this IQueryable<T> source,
        RsqlPageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(page);

        var totalItems = await source.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await source
            .ApplyPage(page)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return RsqlPagedResult<T>.Create(items, page, totalItems);
    }

    /// <summary>
    /// Applies a parsed RSQL query, executes the paged query asynchronously, and returns pagination metadata.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="query">The parsed RSQL query.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="page">The page request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result.</returns>
    public static Task<RsqlPagedResult<T>> ToRsqlPageAsync<T>(
        this IQueryable<T> source,
        RsqlQuery query,
        RsqlLinqProfile<T> profile,
        RsqlPageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplyRsql(query, profile).ToRsqlPageAsync(page, cancellationToken);
    }

    /// <summary>
    /// Applies sorting, executes the paged query asynchronously, and returns pagination metadata.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="sort">The sort request.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="page">The page request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result.</returns>
    public static Task<RsqlPagedResult<T>> ToRsqlPageAsync<T>(
        this IQueryable<T> source,
        RsqlSortRequest sort,
        RsqlLinqProfile<T> profile,
        RsqlPageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplySort(sort, profile).ToRsqlPageAsync(page, cancellationToken);
    }

    /// <summary>
    /// Applies a parsed RSQL query and sorting, executes the paged query asynchronously, and returns pagination metadata.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="query">The parsed RSQL query.</param>
    /// <param name="sort">The sort request.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="page">The page request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result.</returns>
    public static Task<RsqlPagedResult<T>> ToRsqlPageAsync<T>(
        this IQueryable<T> source,
        RsqlQuery query,
        RsqlSortRequest sort,
        RsqlLinqProfile<T> profile,
        RsqlPageRequest page,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(profile);

        return source
            .ApplyRsql(query, profile)
            .ApplySort(sort, profile)
            .ToRsqlPageAsync(page, cancellationToken);
    }

    /// <summary>
    /// Parses an RSQL expression, applies it, executes the paged query asynchronously, and returns pagination metadata.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="page">The page request.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result.</returns>
    public static Task<RsqlPagedResult<T>> ToRsqlPageAsync<T>(
        this IQueryable<T> source,
        string expression,
        RsqlLinqProfile<T> profile,
        RsqlPageRequest page,
        RsqlParseOptions? parseOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplyRsql(expression, profile, parseOptions).ToRsqlPageAsync(page, cancellationToken);
    }

    /// <summary>
    /// Parses an RSQL expression, applies sorting, executes the paged query asynchronously, and returns pagination metadata.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="sort">The sort request.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="page">The page request.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result.</returns>
    public static Task<RsqlPagedResult<T>> ToRsqlPageAsync<T>(
        this IQueryable<T> source,
        string expression,
        RsqlSortRequest sort,
        RsqlLinqProfile<T> profile,
        RsqlPageRequest page,
        RsqlParseOptions? parseOptions = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(profile);

        return source
            .ApplyRsql(expression, profile, parseOptions)
            .ApplySort(sort, profile)
            .ToRsqlPageAsync(page, cancellationToken);
    }
}
