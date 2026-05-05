namespace RsqlParserNet.Linq;

/// <summary>
/// Represents a page of query results with pagination metadata.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
/// <param name="Items">The items in the current page.</param>
/// <param name="Pagination">The pagination metadata.</param>
public sealed record RsqlPagedResult<T>(IReadOnlyList<T> Items, RsqlPagination Pagination)
{
    /// <summary>
    /// Creates a paged result from items, a page request, and the total item count.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="page">The original page request.</param>
    /// <param name="totalItems">The total number of matching items before paging.</param>
    /// <returns>A paged result.</returns>
    public static RsqlPagedResult<T> Create(IReadOnlyList<T> items, RsqlPageRequest page, long totalItems)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(page);

        if (totalItems < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalItems), totalItems, "Total item count must be greater than or equal to 0.");
        }

        return new RsqlPagedResult<T>(items, new RsqlPagination(page.Page, page.PageSize, totalItems));
    }
}
