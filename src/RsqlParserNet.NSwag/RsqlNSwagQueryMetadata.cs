using RsqlParserNet.OpenApi;

namespace RsqlParserNet.NSwag;

/// <summary>
/// Stores RSQL query parameter documentation options for NSwag operation processors.
/// </summary>
/// <param name="Options">The query parameter documentation options.</param>
public sealed record RsqlNSwagQueryMetadata(RsqlOpenApiQueryOptions Options);
