using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using RsqlParserNet.OpenApi;

namespace RsqlParserNet.NSwag;

/// <summary>
/// Documents RSQL query parameters for every NSwag operation.
/// </summary>
public sealed class RsqlNSwagAllQueryParametersOperationProcessor : IOperationProcessor
{
    private readonly RsqlOpenApiQueryOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlNSwagAllQueryParametersOperationProcessor"/> class.
    /// </summary>
    public RsqlNSwagAllQueryParametersOperationProcessor()
        : this(new RsqlOpenApiQueryOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RsqlNSwagAllQueryParametersOperationProcessor"/> class.
    /// </summary>
    /// <param name="options">The query parameter documentation options.</param>
    public RsqlNSwagAllQueryParametersOperationProcessor(RsqlOpenApiQueryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <summary>
    /// Applies RSQL query parameter documentation to an NSwag operation.
    /// </summary>
    /// <param name="context">The NSwag operation processor context.</param>
    /// <returns><see langword="true"/> to keep the operation in the generated document.</returns>
    public bool Process(OperationProcessorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        RsqlNSwagOperationDocumenter.Apply(context.OperationDescription.Operation, _options);
        return true;
    }
}
