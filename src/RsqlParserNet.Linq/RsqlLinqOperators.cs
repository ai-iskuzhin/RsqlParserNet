namespace RsqlParserNet.Linq;

/// <summary>
/// Defines conventional custom operator text used by the LINQ adapter helpers.
/// </summary>
public static class RsqlLinqOperators
{
    /// <summary>
    /// String contains operator text, represented by <c>=contains=</c>.
    /// </summary>
    public const string Contains = "=contains=";

    /// <summary>
    /// Collection any-match operator text, represented by <c>=any=</c>.
    /// </summary>
    public const string Any = "=any=";

    /// <summary>
    /// Collection all-match operator text, represented by <c>=all=</c>.
    /// </summary>
    public const string All = "=all=";
}
