namespace RsqlParserNet.Linq;

/// <summary>
/// Provides type inspection helpers for LINQ expression generation.
/// </summary>
internal static class RsqlLinqTypeHelpers
{
    /// <summary>
    /// Gets the element type for an <see cref="IEnumerable{T}"/> type.
    /// </summary>
    /// <param name="type">The collection type.</param>
    /// <param name="operatorText">The operator text used for error reporting.</param>
    /// <returns>The collection element type.</returns>
    public static Type GetEnumerableElementType(Type type, string operatorText)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type == typeof(string))
        {
            throw new RsqlLinqException($"Custom operator '{operatorText}' requires a collection mapped member.");
        }

        if (type.IsArray)
        {
            return type.GetElementType()
                ?? throw new RsqlLinqException($"Custom operator '{operatorText}' requires a typed collection mapped member.");
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        var enumerableType = type
            .GetInterfaces()
            .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableType?.GetGenericArguments()[0]
            ?? throw new RsqlLinqException($"Custom operator '{operatorText}' requires a collection mapped member.");
    }
}
