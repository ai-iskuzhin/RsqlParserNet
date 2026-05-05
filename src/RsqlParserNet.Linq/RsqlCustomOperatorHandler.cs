namespace RsqlParserNet.Linq;

/// <summary>
/// Stores custom operator translation behavior for the LINQ adapter.
/// </summary>
/// <param name="ValueTypeSelector">Selects the target value type from the mapped member type.</param>
/// <param name="Factory">Builds the custom operator expression.</param>
internal sealed record RsqlCustomOperatorHandler(
    Func<Type, Type> ValueTypeSelector,
    RsqlCustomOperatorExpressionFactory Factory);
