namespace RsqlParserNet;

/// <summary>
/// Defines configurable behavior for RSQL parsing.
/// </summary>
public sealed record RsqlParseOptions
{
    /// <summary>
    /// Gets the default parser options.
    /// </summary>
    public static RsqlParseOptions Default { get; } = new();

    /// <summary>
    /// Gets a value indicating whether standalone word logical operators are accepted.
    /// </summary>
    /// <remarks>
    /// When enabled, expressions such as <c>status==active and title==Bike</c> are accepted
    /// in addition to symbolic RSQL separators such as <c>;</c> and <c>,</c>.
    /// </remarks>
    public bool AllowWordLogicalOperators { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether dotted selector paths are accepted.
    /// </summary>
    /// <remarks>
    /// When enabled, selectors such as <c>customer.name</c> are accepted. Adapters
    /// should still require explicit allowlisted mappings before applying selectors.
    /// </remarks>
    public bool AllowDottedSelectors { get; init; } = true;

    /// <summary>
    /// Gets configured custom FIQL-style comparison operators.
    /// </summary>
    /// <remarks>
    /// Custom operator text must use FIQL-style syntax, such as <c>=contains=</c>.
    /// </remarks>
    public IReadOnlyCollection<RsqlCustomOperator> CustomOperators { get; init; } = [];
}
