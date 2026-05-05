namespace RsqlParserNet;

/// <summary>
/// Represents a parsed RSQL query expression.
/// </summary>
/// <param name="Expression">The original expression text.</param>
/// <param name="Root">The root syntax node.</param>
public sealed record RsqlQuery(string Expression, RsqlNode Root);
