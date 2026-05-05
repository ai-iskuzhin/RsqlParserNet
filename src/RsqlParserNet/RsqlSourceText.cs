namespace RsqlParserNet;

/// <summary>
/// Maps source offsets to line and column locations.
/// </summary>
internal sealed class RsqlSourceText
{
    private readonly string _text;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlSourceText"/> class.
    /// </summary>
    /// <param name="text">The source text to map.</param>
    public RsqlSourceText(string text)
    {
        _text = text;
    }

    /// <summary>
    /// Creates a diagnostic with source span and line/column locations.
    /// </summary>
    /// <param name="code">The diagnostic code.</param>
    /// <param name="message">The diagnostic message.</param>
    /// <param name="span">The source span associated with the diagnostic.</param>
    /// <returns>The created diagnostic.</returns>
    public RsqlDiagnostic CreateDiagnostic(string code, string message, RsqlTextSpan span)
    {
        return new RsqlDiagnostic(code, message, span, GetLocation(span.Start), GetLocation(span.End));
    }

    /// <summary>
    /// Gets the line and column location for a source offset.
    /// </summary>
    /// <param name="offset">The zero-based source offset.</param>
    /// <returns>The source location.</returns>
    public RsqlSourceLocation GetLocation(int offset)
    {
        var clampedOffset = Math.Clamp(offset, 0, _text.Length);
        var line = 0;
        var column = 0;

        for (var index = 0; index < clampedOffset; index++)
        {
            if (_text[index] == '\n')
            {
                line++;
                column = 0;
                continue;
            }

            column++;
        }

        return new RsqlSourceLocation(clampedOffset, line, column);
    }
}
