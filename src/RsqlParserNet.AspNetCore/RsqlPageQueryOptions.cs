using RsqlParserNet.Linq;

namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Configures ASP.NET Core query binding for <see cref="RsqlPageQuery"/>.
/// </summary>
public sealed class RsqlPageQueryOptions
{
    /// <summary>
    /// Gets or sets the query string parameter name used for the one-based page number.
    /// </summary>
    public string PageParameterName { get; set; } = RsqlPageQuery.DefaultPageParameterName;

    /// <summary>
    /// Gets or sets the query string parameter name used for the page size.
    /// </summary>
    public string PageSizeParameterName { get; set; } = RsqlPageQuery.DefaultPageSizeParameterName;

    /// <summary>
    /// Gets or sets the page number used when the request does not include a page value.
    /// </summary>
    public int DefaultPage { get; set; } = RsqlPageRequest.FirstPage;

    /// <summary>
    /// Gets or sets the page size used when the request does not include a page size value.
    /// </summary>
    public int DefaultPageSize { get; set; } = RsqlPageRequest.DefaultPageSize;

    /// <summary>
    /// Gets or sets the maximum page size allowed by the binding layer.
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}
