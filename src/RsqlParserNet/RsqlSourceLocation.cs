namespace RsqlParserNet;

/// <summary>
/// Represents a zero-based source location in an RSQL expression.
/// </summary>
/// <param name="Offset">The zero-based character offset.</param>
/// <param name="Line">The zero-based line number.</param>
/// <param name="Column">The zero-based column number.</param>
public readonly record struct RsqlSourceLocation(int Offset, int Line, int Column);
