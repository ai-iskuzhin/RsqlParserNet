using System.Globalization;
using System.Linq.Expressions;

namespace RsqlParserNet.Linq;

/// <summary>
/// Builds LINQ expression tree predicates from parsed RSQL queries.
/// </summary>
public static class RsqlPredicateBuilder
{
    /// <summary>
    /// Parses an RSQL expression and builds a predicate expression.
    /// </summary>
    /// <typeparam name="T">The element type being filtered.</typeparam>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="configure">Configures allowlisted selector mappings.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <returns>A predicate expression for the supplied RSQL expression.</returns>
    public static Expression<Func<T, bool>> BuildPredicate<T>(
        string expression,
        Action<RsqlLinqOptions<T>> configure,
        RsqlParseOptions? parseOptions = null)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(configure);

        var query = parseOptions is null
            ? RsqlParser.Parse(expression)
            : RsqlParser.Parse(expression, parseOptions);

        return BuildPredicate(query, configure);
    }

    /// <summary>
    /// Parses an RSQL expression and builds a predicate expression using reusable profile configuration.
    /// </summary>
    /// <typeparam name="T">The element type being filtered.</typeparam>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <param name="parseOptions">Optional parser options.</param>
    /// <returns>A predicate expression for the supplied RSQL expression.</returns>
    public static Expression<Func<T, bool>> BuildPredicate<T>(
        string expression,
        RsqlLinqProfile<T> profile,
        RsqlParseOptions? parseOptions = null)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var effectiveParseOptions = ConfigureProfileParseOptions(profile, parseOptions);
        return BuildPredicate<T>(expression, options => options.ApplyProfile(profile), effectiveParseOptions);
    }

    /// <summary>
    /// Builds a predicate expression from a parsed RSQL query.
    /// </summary>
    /// <typeparam name="T">The element type being filtered.</typeparam>
    /// <param name="query">The parsed RSQL query.</param>
    /// <param name="configure">Configures allowlisted selector mappings.</param>
    /// <returns>A predicate expression for the supplied parsed query.</returns>
    public static Expression<Func<T, bool>> BuildPredicate<T>(
        RsqlQuery query,
        Action<RsqlLinqOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new RsqlLinqOptions<T>();
        configure(options);

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = BuildExpression(query.Root, options, parameter);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// Builds a predicate expression from a parsed RSQL query using reusable profile configuration.
    /// </summary>
    /// <typeparam name="T">The element type being filtered.</typeparam>
    /// <param name="query">The parsed RSQL query.</param>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    /// <returns>A predicate expression for the supplied parsed query.</returns>
    public static Expression<Func<T, bool>> BuildPredicate<T>(
        RsqlQuery query,
        RsqlLinqProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return BuildPredicate<T>(query, options => options.ApplyProfile(profile));
    }

    private static RsqlParseOptions ConfigureProfileParseOptions<T>(
        RsqlLinqProfile<T> profile,
        RsqlParseOptions? parseOptions)
    {
        var baseOptions = parseOptions ?? RsqlParseOptions.Default;
        return profile.ConfigureParseOptions(baseOptions)
            ?? throw new RsqlLinqException("Profile returned null parser options.");
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
            RsqlComparisonOperator.Equal => BuildEqualityComparison(comparison, left, options, negate: false),
            RsqlComparisonOperator.NotEqual => BuildEqualityComparison(comparison, left, options, negate: true),
            RsqlComparisonOperator.GreaterThan => BuildSingleValueComparison(comparison, left, Expression.GreaterThan),
            RsqlComparisonOperator.GreaterThanOrEqual => BuildSingleValueComparison(comparison, left, Expression.GreaterThanOrEqual),
            RsqlComparisonOperator.LessThan => BuildSingleValueComparison(comparison, left, Expression.LessThan),
            RsqlComparisonOperator.LessThanOrEqual => BuildSingleValueComparison(comparison, left, Expression.LessThanOrEqual),
            RsqlComparisonOperator.In => BuildContainsComparison(comparison, left, negate: false),
            RsqlComparisonOperator.NotIn => BuildContainsComparison(comparison, left, negate: true),
            RsqlComparisonOperator.Custom => BuildCustomComparison(comparison, left, options),
            _ => throw new RsqlLinqException($"Operator '{comparison.OperatorText}' is not supported by the LINQ adapter.")
        };
    }

    private static Expression BuildCustomComparison<T>(
        RsqlComparisonNode comparison,
        Expression left,
        RsqlLinqOptions<T> options)
    {
        if (!options.CustomOperators.TryGetValue(comparison.OperatorText, out var handler))
        {
            throw new RsqlLinqException(
                $"Custom operator '{comparison.OperatorText}' is not allowlisted by the LINQ adapter.");
        }

        var valueType = handler.ValueTypeSelector(left.Type);
        var values = comparison.Values.Select(value => ConvertValueExpression(value, valueType)).ToArray();
        var context = new RsqlCustomOperatorExpressionContext(left, values, comparison);
        var expression = handler.Factory(context);

        if (expression.Type != typeof(bool))
        {
            throw new RsqlLinqException(
                $"Custom operator '{comparison.OperatorText}' must return a Boolean expression.");
        }

        return expression;
    }

    private static Expression BuildEqualityComparison<T>(
        RsqlComparisonNode comparison,
        Expression left,
        RsqlLinqOptions<T> options,
        bool negate)
    {
        if (comparison.Values.Count != 1)
        {
            throw new RsqlLinqException($"Operator '{comparison.OperatorText}' requires exactly one value.");
        }

        var value = comparison.Values[0];
        var isWildcardComparison =
            options.StringWildcardMode == RsqlStringWildcardMode.Enabled &&
            left.Type == typeof(string) &&
            value.Kind == RsqlValueKind.String &&
            value.Text?.Contains('*', StringComparison.Ordinal) == true;

        var expression = isWildcardComparison
            ? BuildWildcardComparison(comparison, left, value.Text!, options.StringComparisonMode)
            : BuildSingleValueComparison(comparison, left, negate ? Expression.NotEqual : Expression.Equal);

        return negate && isWildcardComparison ? Expression.Not(expression) : expression;
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

    private static Expression BuildWildcardComparison(
        RsqlComparisonNode comparison,
        Expression left,
        string pattern,
        RsqlStringComparisonMode comparisonMode)
    {
        var segments = pattern.Split('*');
        var meaningfulSegments = segments.Where(segment => segment.Length > 0).ToArray();

        if (meaningfulSegments.Length == 0)
        {
            return Expression.NotEqual(left, Expression.Constant(null, typeof(string)));
        }

        if (segments.Length > 3)
        {
            throw new RsqlLinqException(
                $"Wildcard pattern '{comparison.Values[0].RawText}' is too complex for the LINQ adapter.");
        }

        var startsWithWildcard = pattern.StartsWith('*');
        var endsWithWildcard = pattern.EndsWith('*');
        var nullGuard = Expression.NotEqual(left, Expression.Constant(null, typeof(string)));
        Expression match = (startsWithWildcard, endsWithWildcard, meaningfulSegments.Length) switch
        {
            (true, true, 1) => CallStringMethod(left, nameof(string.Contains), meaningfulSegments[0], comparisonMode),
            (true, false, 1) => CallStringMethod(left, nameof(string.EndsWith), meaningfulSegments[0], comparisonMode),
            (false, true, 1) => CallStringMethod(left, nameof(string.StartsWith), meaningfulSegments[0], comparisonMode),
            (false, false, 2) => Expression.AndAlso(
                CallStringMethod(left, nameof(string.StartsWith), meaningfulSegments[0], comparisonMode),
                CallStringMethod(left, nameof(string.EndsWith), meaningfulSegments[1], comparisonMode)),
            _ => throw new RsqlLinqException(
                $"Wildcard pattern '{comparison.Values[0].RawText}' is not supported by the LINQ adapter.")
        };

        return Expression.AndAlso(nullGuard, match);
    }

    private static MethodCallExpression CallStringMethod(
        Expression instance,
        string methodName,
        string value,
        RsqlStringComparisonMode comparisonMode)
    {
        var comparisonValue = Expression.Constant(value, typeof(string));
        var (member, constant) = comparisonMode switch
        {
            RsqlStringComparisonMode.ProviderDefault => (instance, (Expression)comparisonValue),
            RsqlStringComparisonMode.CaseInsensitive => ((Expression)NormalizeString(instance), NormalizeString(comparisonValue)),
            _ => throw new RsqlLinqException($"String comparison mode '{comparisonMode}' is not supported.")
        };

        return Expression.Call(
            member,
            typeof(string).GetMethod(methodName, [typeof(string)])!,
            constant);
    }

    private static MethodCallExpression NormalizeString(Expression expression)
    {
        return Expression.Call(
            expression,
            typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!);
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
            var converter = TypeDescriptor.GetConverter(nonNullableType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(null, CultureInfo.InvariantCulture, text);
            }
            
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
