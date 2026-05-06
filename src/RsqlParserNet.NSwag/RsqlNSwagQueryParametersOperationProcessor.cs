using Microsoft.AspNetCore.Mvc.Abstractions;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace RsqlParserNet.NSwag;

/// <summary>
/// Documents RSQL query parameters for endpoints marked with <see cref="RsqlNSwagQueryMetadata"/>.
/// </summary>
public sealed class RsqlNSwagQueryParametersOperationProcessor : IOperationProcessor
{
    /// <summary>
    /// Applies RSQL query parameter documentation to marked NSwag operations.
    /// </summary>
    /// <param name="context">The NSwag operation processor context.</param>
    /// <returns><see langword="true"/> to keep the operation in the generated document.</returns>
    public bool Process(OperationProcessorContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context is not AspNetCoreOperationProcessorContext aspNetCoreContext)
        {
            return true;
        }

        var metadata = GetMetadata(aspNetCoreContext.ApiDescription.ActionDescriptor).LastOrDefault();
        if (metadata is null)
        {
            return true;
        }

        RsqlNSwagOperationDocumenter.Apply(context.OperationDescription.Operation, metadata.Options);
        return true;
    }

    private static IEnumerable<RsqlNSwagQueryMetadata> GetMetadata(ActionDescriptor actionDescriptor)
    {
        return actionDescriptor.EndpointMetadata.OfType<RsqlNSwagQueryMetadata>();
    }
}
