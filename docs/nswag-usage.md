# NSwag Usage

`RsqlParserNet.NSwag` adds NSwag operation processors for APIs that expose RSQL query parameters.

Use this package when an application generates Swagger/OpenAPI documents through NSwag, including APIs that use `FastEndpoints.Swagger`. Use `RsqlParserNet.OpenApi` for ASP.NET Core's built-in OpenAPI document generation, and `RsqlParserNet.Swashbuckle` for Swashbuckle.

## Endpoint-Scoped Documentation

Register the operation processor once:

```csharp
using RsqlParserNet.NSwag;

builder.Services.AddOpenApiDocument(options =>
{
    options.AddRsqlQueryParametersOperationProcessor();
});
```

Mark only the endpoints that accept RSQL query parameters:

```csharp
app.MapGet("/products", HandleProducts)
    .WithRsqlNSwagQueryParameters();
```

The helper documents these optional query parameters by default:

```text
filter
sort
page
pageSize
```

## Custom Parameter Names

Keep names aligned with `AddRsqlQueryRequest(...)` options:

```csharp
app.MapGet("/products", HandleProducts)
    .WithRsqlNSwagQueryParameters(options =>
    {
        options.FilterParameterName = "q";
        options.SortParameterName = "orderBy";
        options.PageParameterName = "p";
        options.PageSizeParameterName = "take";
    });
```

## Document Every Operation

For small APIs where every list endpoint uses the same RSQL query contract, a global processor is available:

```csharp
builder.Services.AddOpenApiDocument(options =>
{
    options.AddRsqlQueryParametersToAllOperations();
});
```

Endpoint-scoped documentation is the safer default for larger APIs because it avoids advertising query parameters on endpoints that do not accept them.

## Partial Query Contracts

Endpoints can document only the query parts they support:

```csharp
app.MapGet("/products/search", HandleSearch)
    .WithRsqlNSwagQueryParameters(options =>
    {
        options.IncludeSort = false;
        options.IncludePagination = false;
    });
```

## Advanced Customization

Use `RsqlNSwagOperationDocumenter.Apply(...)` when an application already has its own NSwag operation processor and wants to compose RSQL parameter documentation manually.
