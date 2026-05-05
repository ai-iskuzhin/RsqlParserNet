using RsqlParserNet.AspNetCore;

namespace RsqlParserNet.OpenApi;

/// <summary>
/// Configures OpenAPI query parameter documentation for RSQL endpoints.
/// </summary>
public sealed class RsqlOpenApiQueryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the RSQL filter parameter should be documented.
    /// </summary>
    public bool IncludeFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the sort parameter should be documented.
    /// </summary>
    public bool IncludeSort { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the page and page size parameters should be documented.
    /// </summary>
    public bool IncludePagination { get; set; } = true;

    /// <summary>
    /// Gets or sets the RSQL filter query parameter name.
    /// </summary>
    public string FilterParameterName { get; set; } = RsqlQueryFilter.DefaultQueryParameterName;

    /// <summary>
    /// Gets or sets the sort query parameter name.
    /// </summary>
    public string SortParameterName { get; set; } = RsqlSortQuery.DefaultSortParameterName;

    /// <summary>
    /// Gets or sets the page query parameter name.
    /// </summary>
    public string PageParameterName { get; set; } = RsqlPageQuery.DefaultPageParameterName;

    /// <summary>
    /// Gets or sets the page size query parameter name.
    /// </summary>
    public string PageSizeParameterName { get; set; } = RsqlPageQuery.DefaultPageSizeParameterName;

    /// <summary>
    /// Gets or sets the filter parameter description.
    /// </summary>
    public string FilterDescription { get; set; } =
        "RSQL filter expression. Examples: status==active;name=contains=Ski, status=in=(active,draft).";

    /// <summary>
    /// Gets or sets the sort parameter description.
    /// </summary>
    public string SortDescription { get; set; } =
        "Comma-separated allowlisted sort fields. Prefix a field with '-' for descending order. Example: -updatedAt,name.";

    /// <summary>
    /// Gets or sets the page parameter description.
    /// </summary>
    public string PageDescription { get; set; } = "One-based page number.";

    /// <summary>
    /// Gets or sets the page size parameter description.
    /// </summary>
    public string PageSizeDescription { get; set; } = "Number of items requested per page.";
}
