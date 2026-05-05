namespace RsqlParserNet.Linq;

/// <summary>
/// Defines how the LINQ adapter interprets asterisks in string equality comparisons.
/// </summary>
public enum RsqlStringWildcardMode
{
    /// <summary>
    /// Treats <c>*</c> as a literal character.
    /// </summary>
    Disabled,

    /// <summary>
    /// Treats <c>*</c> as a wildcard in string equality and inequality comparisons.
    /// </summary>
    Enabled
}
