namespace RsqlParserNet;

/// <summary>
/// Represents the severity of a parse diagnostic.
/// </summary>
public enum RsqlDiagnosticSeverity
{
    /// <summary>
    /// Indicates a parse error.
    /// </summary>
    Error
}

/// <summary>
/// Represents a structured RSQL parse diagnostic.
/// </summary>
/// <param name="Code">A stable diagnostic code.</param>
/// <param name="Message">A human-readable diagnostic message.</param>
/// <param name="Span">The source span associated with the diagnostic.</param>
/// <param name="Start">The source location where the diagnostic starts.</param>
/// <param name="End">The source location where the diagnostic ends.</param>
/// <param name="Severity">The diagnostic severity.</param>
public sealed record RsqlDiagnostic(
    string Code,
    string Message,
    RsqlTextSpan Span,
    RsqlSourceLocation Start,
    RsqlSourceLocation End,
    RsqlDiagnosticSeverity Severity = RsqlDiagnosticSeverity.Error);
