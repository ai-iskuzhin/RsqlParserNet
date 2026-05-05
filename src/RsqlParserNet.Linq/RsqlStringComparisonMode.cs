namespace RsqlParserNet.Linq;

/// <summary>
/// Defines how string helper and wildcard expressions compare string values.
/// </summary>
public enum RsqlStringComparisonMode
{
    /// <summary>
    /// Uses the LINQ provider's default string comparison behavior.
    /// </summary>
    ProviderDefault,

    /// <summary>
    /// Normalizes both compared strings before calling string methods.
    /// </summary>
    CaseInsensitive
}
