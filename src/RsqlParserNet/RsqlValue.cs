namespace RsqlParserNet;

/// <summary>
/// Defines the primitive kind of a parsed RSQL value.
/// </summary>
public enum RsqlValueKind
{
    /// <summary>
    /// A string value.
    /// </summary>
    String,

    /// <summary>
    /// A numeric value.
    /// </summary>
    Number,

    /// <summary>
    /// A Boolean value.
    /// </summary>
    Boolean,

    /// <summary>
    /// A null literal.
    /// </summary>
    Null
}

/// <summary>
/// Represents a parsed RSQL comparison value.
/// </summary>
/// <param name="Kind">The value kind.</param>
/// <param name="Text">The normalized value text.</param>
/// <param name="RawText">The exact value text from the source expression.</param>
/// <param name="Span">The source span covered by the value.</param>
public sealed record RsqlValue(RsqlValueKind Kind, string? Text, string RawText, RsqlTextSpan Span);
