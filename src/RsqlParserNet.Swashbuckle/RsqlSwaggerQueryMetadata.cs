using RsqlParserNet.OpenApi;

namespace RsqlParserNet.Swashbuckle;

/// <summary>
/// Marks an endpoint as exposing RSQL query parameters in Swashbuckle-generated OpenAPI documents.
/// </summary>
/// <param name="Options">The query parameter documentation options.</param>
public sealed record RsqlSwaggerQueryMetadata(RsqlOpenApiQueryOptions Options);
