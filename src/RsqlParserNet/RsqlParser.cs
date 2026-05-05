namespace RsqlParserNet;

/// <summary>
/// Parses RSQL/FIQL-style query expressions.
/// </summary>
public static class RsqlParser
{
    /// <summary>
    /// Parses an RSQL expression into a query model.
    /// </summary>
    /// <param name="expression">The RSQL expression text.</param>
    /// <returns>The parsed query model.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is empty.</exception>
    /// <exception cref="RsqlParseException">Thrown when the expression contains invalid syntax.</exception>
    public static RsqlQuery Parse(string expression)
    {
        return Parse(expression, RsqlParseOptions.Default);
    }

    /// <summary>
    /// Parses an RSQL expression into a query model.
    /// </summary>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="options">The parser options.</param>
    /// <returns>The parsed query model.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is empty.</exception>
    /// <exception cref="RsqlParseException">Thrown when the expression contains invalid syntax.</exception>
    public static RsqlQuery Parse(string expression, RsqlParseOptions options)
    {
        var result = TryParse(expression, options);
        if (result.Success)
        {
            return result.Query!;
        }

        if (result.Diagnostics.Count > 0 && result.Diagnostics[0].Code == RsqlDiagnosticCodes.EmptyExpression)
        {
            throw new ArgumentException(result.Diagnostics[0].Message, nameof(expression));
        }

        throw new RsqlParseException(result.Diagnostics);
    }

    /// <summary>
    /// Parses an RSQL expression into a query model without throwing for parse errors.
    /// </summary>
    /// <param name="expression">The RSQL expression text.</param>
    /// <returns>The parse result.</returns>
    public static RsqlParseResult TryParse(string expression)
    {
        return TryParse(expression, RsqlParseOptions.Default);
    }

    /// <summary>
    /// Parses an RSQL expression into a query model without throwing for parse errors.
    /// </summary>
    /// <param name="expression">The RSQL expression text.</param>
    /// <param name="options">The parser options.</param>
    /// <returns>The parse result.</returns>
    public static RsqlParseResult TryParse(string expression, RsqlParseOptions options)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(options);
        RsqlParseOptionsValidator.Validate(options);
        return new RsqlSyntaxParser(expression, options).Parse();
    }
}
