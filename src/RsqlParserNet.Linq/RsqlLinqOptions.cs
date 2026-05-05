using System.Linq.Expressions;

namespace RsqlParserNet.Linq;

/// <summary>
/// Configures allowlisted field mappings for LINQ expression generation.
/// </summary>
/// <typeparam name="T">The element type being filtered.</typeparam>
public sealed class RsqlLinqOptions<T>
{
    private readonly Dictionary<string, LambdaExpression> _fields = new(StringComparer.Ordinal);
    private readonly Dictionary<string, RsqlCustomOperatorExpressionFactory> _customOperators = new(StringComparer.Ordinal);

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
    /// Gets the configured custom operator expression factories.
    /// </summary>
    internal IReadOnlyDictionary<string, RsqlCustomOperatorExpressionFactory> CustomOperators => _customOperators;

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

    /// <summary>
    /// Applies reusable profile configuration to these options.
    /// </summary>
    /// <param name="profile">The reusable LINQ adapter profile.</param>
    public void ApplyProfile(RsqlLinqProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        profile.Configure(this);
    }

    /// <summary>
    /// Allows a custom RSQL operator and maps it to a LINQ expression factory.
    /// </summary>
    /// <remarks>
    /// The same operator text must also be configured in <see cref="RsqlParseOptions.CustomOperators"/>
    /// so the core parser can recognize it before the LINQ adapter translates it.
    /// </remarks>
    /// <param name="operatorText">The custom FIQL-style operator text, such as <c>=contains=</c>.</param>
    /// <param name="factory">The expression factory for this custom operator.</param>
    public void CustomOperator(string operatorText, RsqlCustomOperatorExpressionFactory factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorText);
        ArgumentNullException.ThrowIfNull(factory);

        _customOperators[operatorText] = factory;
    }

    /// <summary>
    /// Allows a single-value string <c>Contains</c> custom operator.
    /// </summary>
    /// <remarks>
    /// The operator text must also be configured in <see cref="RsqlParseOptions.CustomOperators"/>.
    /// </remarks>
    /// <param name="operatorText">The custom FIQL-style operator text.</param>
    public void AllowStringContainsOperator(string operatorText = "=contains=")
    {
        CustomOperator(operatorText, context => context.CallStringMethod(nameof(string.Contains)));
    }
}
