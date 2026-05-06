using NSwag.Generation;
using RsqlParserNet.OpenApi;

namespace RsqlParserNet.NSwag;

/// <summary>
/// Provides NSwag settings helpers for RSQL query parameter documentation.
/// </summary>
public static class RsqlNSwagDocumentGeneratorSettingsExtensions
{
    /// <summary>
    /// Registers an endpoint-scoped NSwag operation processor for RSQL query parameters.
    /// </summary>
    /// <typeparam name="TSettings">The NSwag document generator settings type.</typeparam>
    /// <param name="settings">The NSwag document generator settings.</param>
    public static void AddRsqlQueryParametersOperationProcessor<TSettings>(this TSettings settings)
        where TSettings : OpenApiDocumentGeneratorSettings
    {
        ArgumentNullException.ThrowIfNull(settings);

        settings.OperationProcessors.Add(new RsqlNSwagQueryParametersOperationProcessor());
    }

    /// <summary>
    /// Registers an NSwag operation processor that documents RSQL query parameters on every operation.
    /// </summary>
    /// <typeparam name="TSettings">The NSwag document generator settings type.</typeparam>
    /// <param name="settings">The NSwag document generator settings.</param>
    /// <param name="configure">Optional query parameter documentation configuration.</param>
    public static void AddRsqlQueryParametersToAllOperations<TSettings>(
        this TSettings settings,
        Action<RsqlOpenApiQueryOptions>? configure = null)
        where TSettings : OpenApiDocumentGeneratorSettings
    {
        ArgumentNullException.ThrowIfNull(settings);

        var options = new RsqlOpenApiQueryOptions();
        configure?.Invoke(options);

        settings.OperationProcessors.Add(new RsqlNSwagAllQueryParametersOperationProcessor(options));
    }
}
