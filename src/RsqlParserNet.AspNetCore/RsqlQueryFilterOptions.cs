namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Configures ASP.NET Core query binding for <see cref="RsqlQueryFilter"/>.
/// </summary>
public sealed class RsqlQueryFilterOptions
{
    /// <summary>
    /// Gets or sets the default query string parameter name used when no explicit binding name is provided.
    /// </summary>
    public string QueryParameterName { get; set; } = RsqlQueryFilter.DefaultQueryParameterName;

    /// <summary>
    /// Gets or sets the parser options used while binding the query filter.
    /// </summary>
    public RsqlParseOptions ParseOptions { get; set; } = RsqlParseOptions.Default;
}
