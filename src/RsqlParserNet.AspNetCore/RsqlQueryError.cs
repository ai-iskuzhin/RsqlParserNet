namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Identifies which query component produced a binding error.
/// </summary>
public enum RsqlQueryErrorSource
{
    /// <summary>
    /// The error came from the RSQL filter expression.
    /// </summary>
    Filter,

    /// <summary>
    /// The error came from the sort query parameter.
    /// </summary>
    Sort,

    /// <summary>
    /// The error came from the page or page size query parameter.
    /// </summary>
    Page
}

/// <summary>
/// Represents a structured query binding error that can be mapped to API error responses.
/// </summary>
/// <param name="ParameterName">The query string parameter name associated with the error.</param>
/// <param name="Message">The human-readable error message.</param>
/// <param name="Source">The query component that produced the error.</param>
/// <param name="Code">An optional stable diagnostic code.</param>
/// <param name="Span">The optional source span for parser diagnostics.</param>
/// <param name="Start">The optional start location for parser diagnostics.</param>
/// <param name="End">The optional end location for parser diagnostics.</param>
public sealed record RsqlQueryError(
    string ParameterName,
    string Message,
    RsqlQueryErrorSource Source,
    string? Code = null,
    RsqlTextSpan? Span = null,
    RsqlSourceLocation? Start = null,
    RsqlSourceLocation? End = null);
