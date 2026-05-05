namespace RsqlParserNet;

/// <summary>
/// Defines stable diagnostic codes emitted by the RSQL parser.
/// </summary>
public static class RsqlDiagnosticCodes
{
    /// <summary>
    /// The expression is empty or contains only whitespace.
    /// </summary>
    public const string EmptyExpression = "RSQL000";

    /// <summary>
    /// The tokenizer encountered an invalid token or unterminated quoted string.
    /// </summary>
    public const string InvalidToken = "RSQL001";

    /// <summary>
    /// The parser encountered an unexpected token or missing syntax.
    /// </summary>
    public const string UnexpectedToken = "RSQL002";

    /// <summary>
    /// A selector does not match the configured selector syntax.
    /// </summary>
    public const string InvalidSelector = "RSQL003";
}
