namespace RsqlParserNet;

/// <summary>
/// Defines supported RSQL comparison operators.
/// </summary>
public enum RsqlComparisonOperator
{
    /// <summary>
    /// Equality operator, represented by <c>==</c>.
    /// </summary>
    Equal,

    /// <summary>
    /// Inequality operator, represented by <c>!=</c>.
    /// </summary>
    NotEqual,

    /// <summary>
    /// Greater-than operator, represented by <c>&gt;</c> or <c>=gt=</c>.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Greater-than-or-equal operator, represented by <c>&gt;=</c> or <c>=ge=</c>.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Less-than operator, represented by <c>&lt;</c> or <c>=lt=</c>.
    /// </summary>
    LessThan,

    /// <summary>
    /// Less-than-or-equal operator, represented by <c>&lt;=</c> or <c>=le=</c>.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Inclusion operator, represented by <c>=in=</c>.
    /// </summary>
    In,

    /// <summary>
    /// Exclusion operator, represented by <c>=out=</c>.
    /// </summary>
    NotIn,

    /// <summary>
    /// A configured custom FIQL-style comparison operator.
    /// </summary>
    Custom
}
