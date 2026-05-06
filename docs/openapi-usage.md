# OpenAPI Usage

`RsqlParserNet.OpenApi` adds endpoint-scoped OpenAPI documentation helpers for APIs that expose RSQL query parameters.

It does not add runtime behavior. Filtering, sorting, paging, validation, and EF Core execution still come from the other packages.

Use `RsqlParserNet.Swashbuckle` for Swashbuckle, and `RsqlParserNet.NSwag` for NSwag or FastEndpoints.Swagger-style OpenAPI generation.

## Minimal API

Use `WithRsqlQueryParameters()` on endpoints that accept `RsqlQueryRequest`:

```csharp
using RsqlParserNet.OpenApi;

app.MapGet("/products", async (
    RsqlQueryRequest request,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    // endpoint implementation
})
.WithName("ListProducts")
.WithRsqlQueryParameters();
```

The helper documents these optional query parameters by default:

```text
filter
sort
page
pageSize
```

The package uses ASP.NET Core's OpenAPI operation transformer support, so it works with the built-in OpenAPI document generation pipeline.

## Custom Parameter Names

Keep names aligned with `AddRsqlQueryRequest(...)` options:

```csharp
app.MapGet("/products", HandleProducts)
    .WithRsqlQueryParameters(options =>
    {
        options.FilterParameterName = "q";
        options.SortParameterName = "orderBy";
        options.PageParameterName = "p";
        options.PageSizeParameterName = "take";
    });
```

## Partial Query Contracts

Endpoints can document only the query parts they support:

```csharp
app.MapGet("/products/search", HandleSearch)
    .WithRsqlQueryParameters(options =>
    {
        options.IncludeSort = false;
        options.IncludePagination = false;
    });
```

## Advanced Customization

Use `RsqlOpenApiOperationDocumenter.Apply(...)` when an application already has its own OpenAPI operation transformer and wants to compose RSQL parameter documentation manually.
