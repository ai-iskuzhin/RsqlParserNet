namespace RsqlParserNet;

/// <summary>
/// Represents a node in an RSQL abstract syntax tree.
/// </summary>
/// <param name="Span">The source span covered by the node.</param>
public abstract record RsqlNode(RsqlTextSpan Span);

/// <summary>
/// Defines supported RSQL logical operators.
/// </summary>
public enum RsqlLogicalOperator
{
    /// <summary>
    /// Logical conjunction, represented by <c>;</c>.
    /// </summary>
    And,

    /// <summary>
    /// Logical disjunction, represented by <c>,</c>.
    /// </summary>
    Or
}

/// <summary>
/// Represents a comparison expression such as <c>status=="active"</c>.
/// </summary>
/// <param name="Selector">The selector being compared.</param>
/// <param name="Operator">The comparison operator.</param>
/// <param name="OperatorText">The exact operator text from the source expression.</param>
/// <param name="Values">The comparison values.</param>
/// <param name="Span">The source span covered by the node.</param>
public sealed record RsqlComparisonNode(
    string Selector,
    RsqlComparisonOperator Operator,
    string OperatorText,
    IReadOnlyList<RsqlValue> Values,
    RsqlTextSpan Span) : RsqlNode(Span)
{
    /// <summary>
    /// Gets a value indicating whether this comparison uses a configured custom operator.
    /// </summary>
    public bool IsCustomOperator => Operator == RsqlComparisonOperator.Custom;

    /// <summary>
    /// Gets a value indicating whether this comparison has more than one value.
    /// </summary>
    public bool HasMultipleValues => Values.Count > 1;
}

/// <summary>
/// Represents a logical expression joining two or more RSQL nodes.
/// </summary>
/// <param name="Operator">The logical operator.</param>
/// <param name="Children">The child operands joined by the logical operator.</param>
/// <param name="Span">The source span covered by the node.</param>
public sealed record RsqlLogicalNode(
    RsqlLogicalOperator Operator,
    IReadOnlyList<RsqlNode> Children,
    RsqlTextSpan Span) : RsqlNode(Span);
