using System.Globalization;
using System.Linq.Expressions;

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

        var options = new RsqlLinqOptions<T>();
        configure(options);

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = BuildExpression(query.Root, options, parameter);
        var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);

        return source.Where(predicate);
    }

    private static Expression BuildExpression<T>(RsqlNode node, RsqlLinqOptions<T> options, ParameterExpression parameter)
    {
        return node switch
        {
            RsqlComparisonNode comparison => BuildComparison(comparison, options, parameter),
            RsqlLogicalNode logical => BuildLogical(logical, options, parameter),
            _ => throw new RsqlLinqException($"Unsupported RSQL node type '{node.GetType().Name}'.")
        };
    }

    private static Expression BuildLogical<T>(RsqlLogicalNode logical, RsqlLinqOptions<T> options, ParameterExpression parameter)
    {
        if (logical.Children.Count == 0)
        {
            throw new RsqlLinqException("Logical node must contain at least one child.");
        }

        var expressions = logical.Children.Select(child => BuildExpression(child, options, parameter));
        return logical.Operator switch
        {
            RsqlLogicalOperator.And => expressions.Aggregate(Expression.AndAlso),
            RsqlLogicalOperator.Or => expressions.Aggregate(Expression.OrElse),
            _ => throw new RsqlLinqException($"Unsupported logical operator '{logical.Operator}'.")
        };
    }

    private static Expression BuildComparison<T>(
        RsqlComparisonNode comparison,
        RsqlLinqOptions<T> options,
        ParameterExpression parameter)
    {
        if (!options.Fields.TryGetValue(comparison.Selector, out var mapping))
        {
            throw new RsqlLinqException($"Selector '{comparison.Selector}' is not allowlisted.");
        }

        var left = ReplaceParameter(mapping, parameter);

        return comparison.Operator switch
        {
            RsqlComparisonOperator.Equal => BuildSingleValueComparison(comparison, left, Expression.Equal),
            RsqlComparisonOperator.NotEqual => BuildSingleValueComparison(comparison, left, Expression.NotEqual),
            RsqlComparisonOperator.GreaterThan => BuildSingleValueComparison(comparison, left, Expression.GreaterThan),
            RsqlComparisonOperator.GreaterThanOrEqual => BuildSingleValueComparison(comparison, left, Expression.GreaterThanOrEqual),
            RsqlComparisonOperator.LessThan => BuildSingleValueComparison(comparison, left, Expression.LessThan),
            RsqlComparisonOperator.LessThanOrEqual => BuildSingleValueComparison(comparison, left, Expression.LessThanOrEqual),
            RsqlComparisonOperator.In => BuildContainsComparison(comparison, left, negate: false),
            RsqlComparisonOperator.NotIn => BuildContainsComparison(comparison, left, negate: true),
            RsqlComparisonOperator.Custom => throw new RsqlLinqException(
                $"Custom operator '{comparison.OperatorText}' is not supported by the LINQ adapter."),
            _ => throw new RsqlLinqException($"Operator '{comparison.OperatorText}' is not supported by the LINQ adapter.")
        };
    }

    private static Expression BuildSingleValueComparison(
        RsqlComparisonNode comparison,
        Expression left,
        Func<Expression, Expression, BinaryExpression> factory)
    {
        if (comparison.Values.Count != 1)
        {
            throw new RsqlLinqException($"Operator '{comparison.OperatorText}' requires exactly one value.");
        }

        var right = ConvertValueExpression(comparison.Values[0], left.Type);

        try
        {
            return factory(left, right);
        }
        catch (InvalidOperationException exception)
        {
            throw new RsqlLinqException(
                $"Operator '{comparison.OperatorText}' cannot be applied to mapped type '{left.Type.Name}'.",
                exception);
        }
    }

    private static Expression BuildContainsComparison(RsqlComparisonNode comparison, Expression left, bool negate)
    {
        if (comparison.Values.Count == 0)
        {
            throw new RsqlLinqException($"Operator '{comparison.OperatorText}' requires at least one value.");
        }

        var values = comparison.Values.Select(value => ConvertValueExpression(value, left.Type));
        var array = Expression.NewArrayInit(left.Type, values);
        var contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [left.Type],
            array,
            left);

        return negate ? Expression.Not(contains) : contains;
    }

    private static Expression ReplaceParameter(LambdaExpression expression, ParameterExpression parameter)
    {
        return new ParameterReplaceVisitor(expression.Parameters[0], parameter).Visit(expression.Body)
            ?? throw new RsqlLinqException("Field mapping expression could not be rewritten.");
    }

    private static Expression ConvertValueExpression(RsqlValue value, Type targetType)
    {
        var converted = ConvertValue(value, targetType);
        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (converted is null)
        {
            return Expression.Constant(null, targetType);
        }

        var constant = Expression.Constant(converted, nonNullableType);
        return nonNullableType == targetType ? constant : Expression.Convert(constant, targetType);
    }

    private static object? ConvertValue(RsqlValue value, Type targetType)
    {
        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value.Kind == RsqlValueKind.Null)
        {
            if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null)
            {
                throw new RsqlLinqException($"Null cannot be compared with non-nullable mapped type '{targetType.Name}'.");
            }

            return null;
        }

        var text = value.Text ?? string.Empty;

        if (nonNullableType == typeof(string))
        {
            return text;
        }

        try
        {
            if (nonNullableType == typeof(bool))
            {
                return bool.Parse(text);
            }

            if (nonNullableType.IsEnum)
            {
                return Enum.Parse(nonNullableType, text, ignoreCase: true);
            }

            if (nonNullableType == typeof(Guid))
            {
                return Guid.Parse(text);
            }

            if (nonNullableType == typeof(DateTime))
            {
                return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            if (nonNullableType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            if (nonNullableType == typeof(DateOnly))
            {
                return DateOnly.Parse(text, CultureInfo.InvariantCulture);
            }

            if (nonNullableType == typeof(TimeOnly))
            {
                return TimeOnly.Parse(text, CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(text, nonNullableType, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is ArgumentException or FormatException or InvalidCastException or OverflowException)
        {
            throw new RsqlLinqException(
                $"Value '{value.RawText}' cannot be converted to mapped type '{targetType.Name}'.",
                exception);
        }
    }

    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from ? _to : base.VisitParameter(node);
        }
    }
}
