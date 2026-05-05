namespace RsqlParserNet.Linq;

/// <summary>
/// Defines reusable LINQ adapter configuration for a filtered element type.
/// </summary>
/// <typeparam name="T">The element type being filtered.</typeparam>
public abstract class RsqlLinqProfile<T>
{
    /// <summary>
    /// Configures allowlisted selectors, custom operators, and adapter options.
    /// </summary>
    /// <param name="options">The LINQ adapter options to configure.</param>
    public abstract void Configure(RsqlLinqOptions<T> options);
}
