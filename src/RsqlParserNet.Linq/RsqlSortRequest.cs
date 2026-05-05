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

        var normalizedText = text.Trim();
        var isDescending = normalizedText[0] == '-';
        var field = isDescending ? normalizedText[1..] : normalizedText;
        if (string.IsNullOrWhiteSpace(field))
        {
            throw new ArgumentException("Sort field must not be empty.", nameof(text));
        }

        if (!IsValidField(field))
        {
            throw new ArgumentException("Sort field must use selector syntax.", nameof(text));
        }

        return new RsqlSortRequest(
            field,
            isDescending ? RsqlSortDirection.Descending : RsqlSortDirection.Ascending);
    }

    private static bool IsValidField(string field)
    {
        var segmentStart = 0;
        for (var index = 0; index <= field.Length; index++)
        {
            if (index == field.Length || field[index] == '.')
            {
                if (!IsValidSegment(field.AsSpan(segmentStart, index - segmentStart)))
                {
                    return false;
                }

                segmentStart = index + 1;
            }
        }

        return true;
    }

    private static bool IsValidSegment(ReadOnlySpan<char> segment)
    {
        if (segment.Length == 0 || !IsSelectorStart(segment[0]))
        {
            return false;
        }

        for (var index = 1; index < segment.Length; index++)
        {
            if (!IsSelectorPart(segment[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSelectorStart(char value)
    {
        return char.IsAsciiLetter(value) || value == '_';
    }

    private static bool IsSelectorPart(char value)
    {
        return char.IsAsciiLetterOrDigit(value) || value is '_' or '-';
    }
}
