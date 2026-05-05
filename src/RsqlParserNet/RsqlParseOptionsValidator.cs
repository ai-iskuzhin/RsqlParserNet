namespace RsqlParserNet;

/// <summary>
/// Validates parser options before parsing starts.
/// </summary>
internal static class RsqlParseOptionsValidator
{
    private static readonly HashSet<string> BuiltInOperatorTexts = new(StringComparer.OrdinalIgnoreCase)
    {
        "==",
        "!=",
        ">",
        ">=",
        "<",
        "<=",
        "=gt=",
        "=ge=",
        "=lt=",
        "=le=",
        "=in=",
        "=out="
    };

    /// <summary>
    /// Validates parser options and throws when options are invalid.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    public static void Validate(RsqlParseOptions options)
    {
        var seenCustomOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var customOperator in options.CustomOperators)
        {
            ValidateCustomOperator(customOperator, seenCustomOperators);
        }
    }

    private static void ValidateCustomOperator(RsqlCustomOperator customOperator, HashSet<string> seenCustomOperators)
    {
        if (string.IsNullOrWhiteSpace(customOperator.Text))
        {
            throw new ArgumentException("Custom operator text must not be empty.", nameof(RsqlParseOptions.CustomOperators));
        }

        if (!IsFiqlStyleOperator(customOperator.Text))
        {
            throw new ArgumentException(
                $"Custom operator '{customOperator.Text}' must use FIQL-style syntax such as '=contains='.",
                nameof(RsqlParseOptions.CustomOperators));
        }

        if (BuiltInOperatorTexts.Contains(customOperator.Text))
        {
            throw new ArgumentException(
                $"Custom operator '{customOperator.Text}' conflicts with a built-in operator.",
                nameof(RsqlParseOptions.CustomOperators));
        }

        if (!seenCustomOperators.Add(customOperator.Text))
        {
            throw new ArgumentException(
                $"Custom operator '{customOperator.Text}' is configured more than once.",
                nameof(RsqlParseOptions.CustomOperators));
        }
    }

    private static bool IsFiqlStyleOperator(string value)
    {
        if (value.Length < 3 || value[0] != '=' || value[^1] != '=')
        {
            return false;
        }

        for (var index = 1; index < value.Length - 1; index++)
        {
            if (!char.IsAsciiLetter(value[index]) && value[index] != '-')
            {
                return false;
            }
        }

        return true;
    }
}
