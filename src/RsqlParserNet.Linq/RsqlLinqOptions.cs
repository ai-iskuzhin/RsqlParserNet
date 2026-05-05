using System.Linq.Expressions;

namespace RsqlParserNet.Linq;

/// <summary>
/// Configures allowlisted field mappings for LINQ expression generation.
/// </summary>
/// <typeparam name="T">The element type being filtered.</typeparam>
public sealed class RsqlLinqOptions<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets how string equality comparisons interpret <c>*</c> characters.
    /// </summary>
    /// <remarks>
    /// The default is <see cref="RsqlStringWildcardMode.Enabled"/> to match common RSQL API expectations.
    /// </remarks>
    public RsqlStringWildcardMode StringWildcardMode { get; set; } = RsqlStringWildcardMode.Enabled;

    /// <summary>
    /// Gets the configured field mappings.
    /// </summary>
    internal IReadOnlyDictionary<string, LambdaExpression> Fields => _fields;

    /// <summary>
    /// Allows an RSQL selector and maps it to a .NET member expression.
    /// </summary>
    /// <remarks>
    /// Selectors are matched using ordinal case-sensitive comparison.
    /// </remarks>
    /// <typeparam name="TValue">The mapped member type.</typeparam>
    /// <param name="selector">The RSQL selector name.</param>
    /// <param name="expression">The member mapping expression.</param>
    public void Allow<TValue>(string selector, Expression<Func<T, TValue>> expression)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selector);
        ArgumentNullException.ThrowIfNull(expression);

        _fields[selector] = expression;
    }
}
