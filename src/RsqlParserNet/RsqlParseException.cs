namespace RsqlParserNet;

/// <summary>
/// Represents an exception thrown when an RSQL expression cannot be parsed.
/// </summary>
public sealed class RsqlParseException : FormatException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlParseException"/> class.
    /// </summary>
    /// <param name="diagnostics">The diagnostics that caused the exception.</param>
    public RsqlParseException(IReadOnlyList<RsqlDiagnostic> diagnostics)
        : base(CreateMessage(diagnostics))
    {
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// Gets the structured diagnostics that caused the exception.
    /// </summary>
    public IReadOnlyList<RsqlDiagnostic> Diagnostics { get; }

    private static string CreateMessage(IReadOnlyList<RsqlDiagnostic> diagnostics)
    {
        return diagnostics.Count == 0
            ? "RSQL expression could not be parsed."
            : diagnostics[0].Message;
    }
}
