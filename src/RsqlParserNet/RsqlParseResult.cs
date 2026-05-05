namespace RsqlParserNet;

/// <summary>
/// Represents the result of parsing an RSQL expression.
/// </summary>
/// <param name="Query">The parsed query when parsing succeeded.</param>
/// <param name="Diagnostics">Structured diagnostics produced during parsing.</param>
public sealed record RsqlParseResult(RsqlQuery? Query, IReadOnlyList<RsqlDiagnostic> Diagnostics)
{
    /// <summary>
    /// Gets a value indicating whether parsing completed without diagnostics.
    /// </summary>
    public bool Success => Query is not null && Diagnostics.Count == 0;
}
