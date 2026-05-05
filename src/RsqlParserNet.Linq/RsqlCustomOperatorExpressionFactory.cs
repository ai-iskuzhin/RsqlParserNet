using System.Linq.Expressions;

namespace RsqlParserNet.Linq;

/// <summary>
/// Builds a LINQ expression for a configured custom RSQL comparison operator.
/// </summary>
/// <param name="context">The custom operator expression context.</param>
/// <returns>A Boolean expression that represents the custom operator.</returns>
public delegate Expression RsqlCustomOperatorExpressionFactory(RsqlCustomOperatorExpressionContext context);
