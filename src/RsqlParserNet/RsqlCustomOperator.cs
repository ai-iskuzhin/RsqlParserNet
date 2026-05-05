namespace RsqlParserNet;

/// <summary>
/// Represents a configured custom FIQL-style comparison operator.
/// </summary>
/// <param name="Text">The operator text, such as <c>=contains=</c>.</param>
/// <param name="RequiresMultipleValues">Whether the operator requires a parenthesized multi-value argument list.</param>
public sealed record RsqlCustomOperator(string Text, bool RequiresMultipleValues = false);
