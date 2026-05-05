namespace RsqlParserNet.Linq;

/// <summary>
/// Provides pagination helpers for queryable result sets.
/// </summary>
public static class RsqlQueryablePaginationExtensions
{
    /// <summary>
    /// Applies a page request to a queryable source.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="page">The page request.</param>
    /// <returns>The paged queryable.</returns>
    public static IQueryable<T> ApplyPage<T>(this IQueryable<T> source, RsqlPageRequest page)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(page);

        return source.Skip(page.Skip).Take(page.Take);
    }
}
