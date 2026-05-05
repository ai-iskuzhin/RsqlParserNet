using System.Linq.Expressions;

namespace RsqlParserNet.Linq;

/// <summary>
/// Provides sorting helpers for queryable result sets.
/// </summary>
public static class RsqlQueryableSortExtensions
{
    /// <summary>
    /// Applies an allowlisted sort request to a queryable source.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="sort">The sort request.</param>
    /// <param name="configure">Configures allowlisted selector mappings.</param>
    /// <returns>The sorted queryable.</returns>
    public static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        RsqlSortRequest sort,
        Action<RsqlLinqOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sort);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new RsqlLinqOptions<T>();
        configure(options);

        return source.ApplySort(sort, options);
    }

    /// <summary>
    /// Applies allowlisted sort requests to a queryable source.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="sorts">The sort requests in priority order.</param>
    /// <param name="configure">Configures allowlisted selector mappings.</param>
    /// <returns>The sorted queryable.</returns>
    public static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        IEnumerable<RsqlSortRequest> sorts,
        Action<RsqlLinqOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sorts);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new RsqlLinqOptions<T>();
        configure(options);

        return source.ApplySort(sorts, options);
    }

    /// <summary>
    /// Applies an allowlisted sort request to a queryable source using reusable profile configuration.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="sort">The sort request.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <returns>The sorted queryable.</returns>
    public static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        RsqlSortRequest sort,
        RsqlLinqProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplySort(sort, options => options.ApplyProfile(profile));
    }

    /// <summary>
    /// Applies allowlisted sort requests to a queryable source using reusable profile configuration.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="sorts">The sort requests in priority order.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <returns>The sorted queryable.</returns>
    public static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        IEnumerable<RsqlSortRequest> sorts,
        RsqlLinqProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return source.ApplySort(sorts, options => options.ApplyProfile(profile));
    }

    private static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        RsqlSortRequest sort,
        RsqlLinqOptions<T> options,
        bool isThenBy = false)
    {
        if (!options.Fields.TryGetValue(sort.Field, out var mapping))
        {
            throw new RsqlLinqException($"Sort field '{sort.Field}' is not allowlisted.");
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = new ParameterReplaceVisitor(mapping.Parameters[0], parameter).Visit(mapping.Body)
            ?? throw new RsqlLinqException("Sort mapping expression could not be rewritten.");
        var keySelector = Expression.Lambda(body, parameter);
        var methodName = (sort.IsDescending, isThenBy) switch
        {
            (true, true) => nameof(Queryable.ThenByDescending),
            (true, false) => nameof(Queryable.OrderByDescending),
            (false, true) => nameof(Queryable.ThenBy),
            (false, false) => nameof(Queryable.OrderBy)
        };
        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), body.Type],
            source.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }

    private static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        IEnumerable<RsqlSortRequest> sorts,
        RsqlLinqOptions<T> options)
    {
        var sortList = sorts as IReadOnlyList<RsqlSortRequest> ?? sorts.ToArray();
        if (sortList.Count == 0)
        {
            throw new ArgumentException("At least one sort request is required.", nameof(sorts));
        }

        IOrderedQueryable<T>? ordered = null;
        for (var index = 0; index < sortList.Count; index++)
        {
            ordered = (ordered ?? source).ApplySort(sortList[index], options, isThenBy: index > 0);
        }

        return ordered!;
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
