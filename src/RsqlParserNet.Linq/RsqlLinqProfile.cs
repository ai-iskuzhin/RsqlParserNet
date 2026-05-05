namespace RsqlParserNet.Linq;

/// <summary>
/// Defines reusable LINQ adapter configuration for a filtered element type.
/// </summary>
/// <typeparam name="T">The element type being filtered.</typeparam>
public abstract class RsqlLinqProfile<T>
{
    /// <summary>
    /// Configures parser options needed by this profile.
    /// </summary>
    /// <remarks>
    /// Override this method when the profile needs custom operators or stricter parser behavior.
    /// </remarks>
    /// <param name="options">The base parser options.</param>
    /// <returns>The parser options to use for profile-based string parsing.</returns>
    public virtual RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options;
    }

    /// <summary>
    /// Configures allowlisted selectors, custom operators, and adapter options.
    /// </summary>
    /// <param name="options">The LINQ adapter options to configure.</param>
    public abstract void Configure(RsqlLinqOptions<T> options);
}
