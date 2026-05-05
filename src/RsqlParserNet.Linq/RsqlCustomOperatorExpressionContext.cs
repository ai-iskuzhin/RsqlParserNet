using System.Linq.Expressions;

namespace RsqlParserNet.Linq;

/// <summary>
/// Provides expression-building inputs for a custom RSQL comparison operator.
/// </summary>
public sealed class RsqlCustomOperatorExpressionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlCustomOperatorExpressionContext"/> class.
    /// </summary>
    /// <param name="member">The allowlisted member expression being compared.</param>
    /// <param name="values">The comparison values converted to the member type.</param>
    /// <param name="comparison">The source comparison AST node.</param>
    public RsqlCustomOperatorExpressionContext(
        Expression member,
        IReadOnlyList<Expression> values,
        RsqlComparisonNode comparison)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(comparison);

        Member = member;
        Values = values;
        Comparison = comparison;
    }

    /// <summary>
    /// Gets the allowlisted member expression being compared.
    /// </summary>
    public Expression Member { get; }

    /// <summary>
    /// Gets the comparison values converted to the member type.
    /// </summary>
    public IReadOnlyList<Expression> Values { get; }

    /// <summary>
    /// Gets the source comparison AST node.
    /// </summary>
    public RsqlComparisonNode Comparison { get; }

    /// <summary>
    /// Gets the single converted value or throws when the custom operator received a different value count.
    /// </summary>
    /// <returns>The single converted value expression.</returns>
    public Expression RequireSingleValue()
    {
        if (Values.Count != 1)
        {
            throw new RsqlLinqException($"Custom operator '{Comparison.OperatorText}' requires exactly one value.");
        }

        return Values[0];
    }

    /// <summary>
    /// Builds a string instance method call such as <c>Contains</c>, <c>StartsWith</c>, or <c>EndsWith</c>.
    /// </summary>
    /// <param name="methodName">The string method name.</param>
    /// <param name="comparisonMode">The string comparison behavior.</param>
    /// <param name="addNullGuard">Whether to guard the member expression against null before calling the method.</param>
    /// <returns>A Boolean expression for the string method call.</returns>
    public Expression CallStringMethod(
        string methodName,
        RsqlStringComparisonMode comparisonMode,
        bool addNullGuard = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        if (Member.Type != typeof(string))
        {
            throw new RsqlLinqException(
                $"Custom operator '{Comparison.OperatorText}' requires a string mapped member.");
        }

        var value = RequireSingleValue();
        if (value.Type != typeof(string))
        {
            throw new RsqlLinqException(
                $"Custom operator '{Comparison.OperatorText}' requires a string comparison value.");
        }

        var method = typeof(string).GetMethod(methodName, [typeof(string)])
            ?? throw new RsqlLinqException($"String method '{methodName}' could not be found.");

        var (member, comparisonValue) = comparisonMode switch
        {
            RsqlStringComparisonMode.ProviderDefault => (Member, value),
            RsqlStringComparisonMode.CaseInsensitive => (NormalizeString(Member), NormalizeString(value)),
            _ => throw new RsqlLinqException($"String comparison mode '{comparisonMode}' is not supported.")
        };

        var call = Expression.Call(member, method, comparisonValue);
        if (!addNullGuard)
        {
            return call;
        }

        var nullGuard = Expression.NotEqual(Member, Expression.Constant(null, typeof(string)));
        return Expression.AndAlso(nullGuard, call);
    }

    /// <summary>
    /// Builds a string instance method call such as <c>Contains</c>, <c>StartsWith</c>, or <c>EndsWith</c>.
    /// </summary>
    /// <param name="methodName">The string method name.</param>
    /// <param name="addNullGuard">Whether to guard the member expression against null before calling the method.</param>
    /// <returns>A Boolean expression for the string method call.</returns>
    public Expression CallStringMethod(string methodName, bool addNullGuard = true)
    {
        return CallStringMethod(methodName, RsqlStringComparisonMode.ProviderDefault, addNullGuard);
    }

    private static MethodCallExpression NormalizeString(Expression expression)
    {
        var normalize = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)
            ?? throw new RsqlLinqException("String method 'ToUpper' could not be found.");
        return Expression.Call(expression, normalize);
    }

    /// <summary>
    /// Builds a collection expression that matches when any mapped collection item is present in the supplied values.
    /// </summary>
    /// <returns>A Boolean expression for collection any matching.</returns>
    public Expression CallCollectionAny()
    {
        var elementType = RequireCollectionElementType();
        RequireAtLeastOneValue();

        var collection = AsEnumerableExpression(elementType);
        var item = Expression.Parameter(elementType, "item");
        var values = Expression.NewArrayInit(elementType, Values);
        var contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [elementType],
            values,
            item);
        var any = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Any),
            [elementType],
            collection,
            Expression.Lambda(contains, item));

        return AddCollectionNullGuard(any);
    }

    /// <summary>
    /// Builds a collection expression that matches when every supplied value is present in the mapped collection.
    /// </summary>
    /// <returns>A Boolean expression for collection all matching.</returns>
    public Expression CallCollectionAll()
    {
        var elementType = RequireCollectionElementType();
        RequireAtLeastOneValue();

        var collection = AsEnumerableExpression(elementType);
        var value = Expression.Parameter(elementType, "value");
        var values = Expression.NewArrayInit(elementType, Values);
        var contains = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Contains),
            [elementType],
            collection,
            value);
        var all = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.All),
            [elementType],
            values,
            Expression.Lambda(contains, value));

        return AddCollectionNullGuard(all);
    }

    private void RequireAtLeastOneValue()
    {
        if (Values.Count == 0)
        {
            throw new RsqlLinqException($"Custom operator '{Comparison.OperatorText}' requires at least one value.");
        }
    }

    private Type RequireCollectionElementType()
    {
        var elementType = RsqlLinqTypeHelpers.GetEnumerableElementType(Member.Type, Comparison.OperatorText);
        if (Values.Any(value => value.Type != elementType))
        {
            throw new RsqlLinqException(
                $"Custom operator '{Comparison.OperatorText}' requires values converted to collection element type '{elementType.Name}'.");
        }

        return elementType;
    }

    private Expression AsEnumerableExpression(Type elementType)
    {
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
        return enumerableType.IsAssignableFrom(Member.Type)
            ? Member
            : Expression.Convert(Member, enumerableType);
    }

    private Expression AddCollectionNullGuard(Expression expression)
    {
        if (Member.Type.IsValueType && Nullable.GetUnderlyingType(Member.Type) is null)
        {
            return expression;
        }

        return Expression.AndAlso(
            Expression.NotEqual(Member, Expression.Constant(null, Member.Type)),
            expression);
    }
}
