namespace RsqlParserNet;

/// <summary>
/// Validates selector syntax according to parser options.
/// </summary>
internal static class RsqlSelectorValidator
{
    /// <summary>
    /// Determines whether a selector is valid.
    /// </summary>
    /// <param name="selector">The selector text.</param>
    /// <param name="options">The parser options.</param>
    /// <returns><see langword="true"/> when the selector is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(string selector, RsqlParseOptions options)
    {
        if (selector.Length == 0)
        {
            return false;
        }

        var segmentStart = 0;
        for (var index = 0; index <= selector.Length; index++)
        {
            if (index == selector.Length || selector[index] == '.')
            {
                if (!IsValidSegment(selector.AsSpan(segmentStart, index - segmentStart)))
                {
                    return false;
                }

                if (index < selector.Length && !options.AllowDottedSelectors)
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
