namespace RsqlParserNet.Linq;

/// <summary>
/// Provides parser option helpers for LINQ adapter custom operator conventions.
/// </summary>
public static class RsqlLinqParseOptionsExtensions
{
    /// <summary>
    /// Adds the conventional LINQ adapter custom operators when they are not already configured.
    /// </summary>
    /// <param name="options">The parser options to extend.</param>
    /// <returns>Parser options with the LINQ adapter custom operators configured.</returns>
    public static RsqlParseOptions WithLinqOperators(this RsqlParseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var customOperators = options.CustomOperators.ToList();
        AddCustomOperator(customOperators, new RsqlCustomOperator(RsqlLinqOperators.Contains));
        AddCustomOperator(customOperators, new RsqlCustomOperator(RsqlLinqOperators.StartsWith));
        AddCustomOperator(customOperators, new RsqlCustomOperator(RsqlLinqOperators.EndsWith));
        AddCustomOperator(customOperators, new RsqlCustomOperator(RsqlLinqOperators.Any, RequiresMultipleValues: true));
        AddCustomOperator(customOperators, new RsqlCustomOperator(RsqlLinqOperators.All, RequiresMultipleValues: true));

        return options with { CustomOperators = customOperators };
    }

    private static void AddCustomOperator(List<RsqlCustomOperator> customOperators, RsqlCustomOperator customOperator)
    {
        if (customOperators.All(item => item.Text != customOperator.Text))
        {
            customOperators.Add(customOperator);
        }
    }
}
