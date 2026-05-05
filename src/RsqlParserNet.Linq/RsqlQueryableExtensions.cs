namespace RsqlParserNet.Linq;

/// <summary>
/// Provides LINQ integration helpers for parsed RSQL queries.
/// </summary>
public static class RsqlQueryableExtensions
{
    /// <summary>
    /// Parses and applies an RSQL expression to an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="configure">Configures allowlisted selector mappings.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <returns>The filtered queryable.</returns>
    public static IQueryable<T> ApplyRsql<T>(
        this IQueryable<T> source,
        string expression,
        Action<RsqlLinqOptions<T>> configure,
        RsqlParseOptions? parseOptions = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(configure);

        var query = parseOptions is null
            ? RsqlParser.Parse(expression)
            : RsqlParser.Parse(expression, parseOptions);

        return source.ApplyRsql(query, configure);
    }

    /// <summary>
    /// Parses and applies an RSQL expression to an <see cref="IQueryable{T}"/> using reusable profile configuration.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <returns>The filtered queryable.</returns>
    public static IQueryable<T> ApplyRsql<T>(
        this IQueryable<T> source,
        string expression,
        RsqlLinqProfile<T> profile,
        RsqlParseOptions? parseOptions = null)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplyRsql(expression, options => options.ApplyProfile(profile), parseOptions);
    }

    /// <summary>
    /// Applies a parsed RSQL query to an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="query">The parsed RSQL query.</param>
    /// <param name="configure">Configures allowlisted selector mappings.</param>
    /// <returns>The filtered queryable.</returns>
    public static IQueryable<T> ApplyRsql<T>(
        this IQueryable<T> source,
        RsqlQuery query,
        Action<RsqlLinqOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configure);

        var predicate = RsqlPredicateBuilder.BuildPredicate(query, configure);
        return source.Where(predicate);
    }

    /// <summary>
    /// Applies a parsed RSQL query to an <see cref="IQueryable{T}"/> using reusable profile configuration.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="query">The parsed RSQL query.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <returns>The filtered queryable.</returns>
    public static IQueryable<T> ApplyRsql<T>(
        this IQueryable<T> source,
        RsqlQuery query,
        RsqlLinqProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplyRsql(query, options => options.ApplyProfile(profile));
    }
}
