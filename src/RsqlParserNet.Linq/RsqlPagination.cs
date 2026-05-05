namespace RsqlParserNet.Linq;

/// <summary>
/// Describes the pagination state for a paged result.
/// </summary>
public sealed record RsqlPagination
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlPagination"/> record.
    /// </summary>
    /// <param name="page">The one-based current page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="totalItems">The total number of matching items before paging.</param>
    public RsqlPagination(int page, int pageSize, long totalItems)
    {
        if (page < RsqlPageRequest.FirstPage)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than or equal to 1.");
        }

        if (totalItems < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalItems), totalItems, "Total item count must be greater than or equal to 0.");
        }

        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    /// <summary>
    /// Gets the one-based current page number.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Gets the requested page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of matching items before paging.
    /// </summary>
    public long TotalItems { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public long TotalPages => TotalItems == 0 ? 0 : (TotalItems / PageSize) + (TotalItems % PageSize == 0 ? 0 : 1);

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => Page > RsqlPageRequest.FirstPage && TotalItems > 0;

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
