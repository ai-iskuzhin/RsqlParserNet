namespace RsqlParserNet.Linq;

/// <summary>
/// Represents a one-based page request for queryable result sets.
/// </summary>
public sealed record RsqlPageRequest
{
    /// <summary>
    /// The first valid page number.
    /// </summary>
    public const int FirstPage = 1;

    /// <summary>
    /// The default number of items requested per page.
    /// </summary>
    public const int DefaultPageSize = 50;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlPageRequest"/> class.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The number of items requested per page.</param>
    public RsqlPageRequest(int page = FirstPage, int pageSize = DefaultPageSize)
    {
        if (page < FirstPage)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than or equal to 1.");
        }

        var skip = ((long)page - 1) * pageSize;
        if (skip > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "The requested page is too large to translate to LINQ Skip.");
        }

        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Gets the one-based page number.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Gets the number of items requested per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the zero-based item offset used by LINQ <c>Skip</c>.
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// Gets the item count used by LINQ <c>Take</c>.
    /// </summary>
    public int Take => PageSize;

    /// <summary>
    /// Returns a copy with the page size clamped to a maximum value.
    /// </summary>
    /// <param name="maxPageSize">The maximum allowed page size.</param>
    /// <returns>The current request when it is already within the maximum; otherwise a clamped copy.</returns>
    public RsqlPageRequest ClampPageSize(int maxPageSize)
    {
        if (maxPageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPageSize), maxPageSize, "Maximum page size must be greater than or equal to 1.");
        }

        return PageSize <= maxPageSize ? this : new RsqlPageRequest(Page, maxPageSize);
    }
}
