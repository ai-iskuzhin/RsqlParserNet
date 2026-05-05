namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Configures ASP.NET Core query binding for <see cref="RsqlSortQuery"/>.
/// </summary>
public sealed class RsqlSortQueryOptions
{
    /// <summary>
    /// Gets or sets the query string parameter name used for sorting.
    /// </summary>
    public string SortParameterName { get; set; } = RsqlSortQuery.DefaultSortParameterName;
}
