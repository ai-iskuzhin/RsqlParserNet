namespace RsqlParserNet.AspNetCore;

/// <summary>
/// Contains stable error codes used by ASP.NET Core and endpoint adapters.
/// </summary>
public static class RsqlQueryErrorCodes
{
    /// <summary>
    /// Indicates that a parsed query could not be translated with the configured adapter profile.
    /// </summary>
    public const string AdapterTranslationError = "RSQL100";
}
