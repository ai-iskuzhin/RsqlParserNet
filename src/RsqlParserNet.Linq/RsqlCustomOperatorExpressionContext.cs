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
    /// <param name="addNullGuard">Whether to guard the member expression against null before calling the method.</param>
    /// <returns>A Boolean expression for the string method call.</returns>
    public Expression CallStringMethod(string methodName, bool addNullGuard = true)
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

        var call = Expression.Call(Member, method, value);
        if (!addNullGuard)
        {
            return call;
        }

        var nullGuard = Expression.NotEqual(Member, Expression.Constant(null, typeof(string)));
        return Expression.AndAlso(nullGuard, call);
    }
}
