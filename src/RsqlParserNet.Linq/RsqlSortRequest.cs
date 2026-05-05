namespace RsqlParserNet.Linq;

/// <summary>
/// Represents a single allowlisted sort request.
/// </summary>
public sealed record RsqlSortRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlSortRequest"/> class.
    /// </summary>
    /// <param name="field">The requested sort field.</param>
    /// <param name="direction">The requested sort direction.</param>
    public RsqlSortRequest(string field, RsqlSortDirection direction = RsqlSortDirection.Ascending)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        Field = field;
        Direction = direction;
    }

    /// <summary>
    /// Gets the requested sort field.
    /// </summary>
    public string Field { get; }

    /// <summary>
    /// Gets the requested sort direction.
    /// </summary>
    public RsqlSortDirection Direction { get; }

    /// <summary>
    /// Gets a value indicating whether the sort direction is descending.
    /// </summary>
    public bool IsDescending => Direction == RsqlSortDirection.Descending;

    /// <summary>
    /// Parses sort text such as <c>name</c> or <c>-createdAt</c>.
    /// </summary>
    /// <param name="text">The raw sort text.</param>
    /// <returns>The parsed sort request.</returns>
    public static RsqlSortRequest Parse(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var isDescending = text[0] == '-';
        var field = isDescending ? text[1..] : text;
        if (string.IsNullOrWhiteSpace(field))
        {
            throw new ArgumentException("Sort field must not be empty.", nameof(text));
        }

        return new RsqlSortRequest(
            field,
            isDescending ? RsqlSortDirection.Descending : RsqlSortDirection.Ascending);
    }
}
