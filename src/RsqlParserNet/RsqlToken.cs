namespace RsqlParserNet;

/// <summary>
/// Defines the lexical token kinds recognized by the RSQL tokenizer.
/// </summary>
internal enum RsqlTokenKind
{
    /// <summary>
    /// Marks the end of the input expression.
    /// </summary>
    EndOfInput,

    /// <summary>
    /// A selector or unquoted value token.
    /// </summary>
    Identifier,

    /// <summary>
    /// A quoted string value token, represented by text enclosed in <c>"</c> or <c>'</c>.
    /// </summary>
    String,

    /// <summary>
    /// The equality comparison operator token, represented by <c>==</c>.
    /// </summary>
    Equal,

    /// <summary>
    /// The inequality comparison operator token, represented by <c>!=</c>.
    /// </summary>
    NotEqual,

    /// <summary>
    /// The greater-than comparison operator token, represented by <c>&gt;</c> or <c>=gt=</c>.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// The greater-than-or-equal comparison operator token, represented by <c>&gt;=</c> or <c>=ge=</c>.
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// The less-than comparison operator token, represented by <c>&lt;</c> or <c>=lt=</c>.
    /// </summary>
    LessThan,

    /// <summary>
    /// The less-than-or-equal comparison operator token, represented by <c>&lt;=</c> or <c>=le=</c>.
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// The inclusion comparison operator token, represented by <c>=in=</c>.
    /// </summary>
    In,

    /// <summary>
    /// The exclusion comparison operator token, represented by <c>=out=</c>.
    /// </summary>
    NotIn,

    /// <summary>
    /// A configured custom comparison operator token.
    /// </summary>
    CustomOperator,

    /// <summary>
    /// An opening parenthesis token, represented by <c>(</c>.
    /// </summary>
    OpenParen,

    /// <summary>
    /// A closing parenthesis token, represented by <c>)</c>.
    /// </summary>
    CloseParen,

    /// <summary>
    /// A comma token, represented by <c>,</c> and used for OR expressions and multi-value arguments.
    /// </summary>
    Comma,

    /// <summary>
    /// A semicolon token, represented by <c>;</c> and used for AND expressions.
    /// </summary>
    Semicolon,

    /// <summary>
    /// A word-form logical AND token, represented by standalone <c>and</c>.
    /// </summary>
    And,

    /// <summary>
    /// A word-form logical OR token, represented by standalone <c>or</c>.
    /// </summary>
    Or,

    /// <summary>
    /// A token that could not be recognized as valid RSQL syntax.
    /// </summary>
    Invalid
}

/// <summary>
/// Represents one lexical token read from an RSQL expression.
/// </summary>
/// <param name="Kind">The token kind.</param>
/// <param name="Text">The normalized token text.</param>
/// <param name="RawText">The exact token text from the source expression.</param>
/// <param name="Span">The source span covered by the token.</param>
internal readonly record struct RsqlToken(RsqlTokenKind Kind, string Text, string RawText, RsqlTextSpan Span);
