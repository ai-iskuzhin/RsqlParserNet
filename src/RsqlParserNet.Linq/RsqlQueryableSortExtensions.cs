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

    private static IOrderedQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        RsqlSortRequest sort,
        RsqlLinqOptions<T> options)
    {
        if (!options.Fields.TryGetValue(sort.Field, out var mapping))
        {
            throw new RsqlLinqException($"Sort field '{sort.Field}' is not allowlisted.");
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = new ParameterReplaceVisitor(mapping.Parameters[0], parameter).Visit(mapping.Body)
            ?? throw new RsqlLinqException("Sort mapping expression could not be rewritten.");
        var keySelector = Expression.Lambda(body, parameter);
        var methodName = sort.IsDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            [typeof(T), body.Type],
            source.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(call);
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
