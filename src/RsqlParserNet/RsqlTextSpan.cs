namespace RsqlParserNet;

/// <summary>
/// Represents a zero-based source span in an RSQL expression.
/// </summary>
/// <param name="Start">The zero-based start offset.</param>
/// <param name="Length">The span length.</param>
public readonly record struct RsqlTextSpan(int Start, int Length)
{
    /// <summary>
    /// Gets the exclusive end offset.
    /// </summary>
    public int End => Start + Length;

    /// <summary>
    /// Creates a source span from inclusive start and exclusive end offsets.
    /// </summary>
    /// <param name="start">The inclusive start offset.</param>
    /// <param name="end">The exclusive end offset.</param>
    /// <returns>The created source span.</returns>
    internal static RsqlTextSpan FromBounds(int start, int end) => new(start, end - start);
}
