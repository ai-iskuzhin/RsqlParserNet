# RsqlParserNet.OpenApi

[![RsqlParserNet.OpenApi NuGet](https://img.shields.io/nuget/v/RsqlParserNet.OpenApi?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.OpenApi)
[![RsqlParserNet.OpenApi Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.OpenApi?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.OpenApi)

Endpoint-scoped OpenAPI documentation for the `filter`, `sort`, `page`, and `pageSize` query parameters, using ASP.NET Core's built-in OpenAPI (`Microsoft.AspNetCore.OpenApi`). It documents the parameters only; filtering, sorting, paging, and validation come from the other packages.

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. It builds on the [`RsqlParserNet.AspNetCore`](https://www.nuget.org/packages/RsqlParserNet.AspNetCore) query models, which build on the [`RsqlParserNet`](https://www.nuget.org/packages/RsqlParserNet) core parser. For other OpenAPI stacks, use [`RsqlParserNet.Swashbuckle`](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle) (Swashbuckle / SwaggerGen) or [`RsqlParserNet.NSwag`](https://www.nuget.org/packages/RsqlParserNet.NSwag) (NSwag and FastEndpoints.Swagger-style generation).

## Installation

```bash
dotnet add package RsqlParserNet.OpenApi
```

## Quick start

Call `WithRsqlQueryParameters()` on an endpoint that accepts `RsqlQueryRequest`. It registers an OpenAPI operation transformer that documents the four query parameters as optional:

```csharp
using RsqlParserNet.AspNetCore;
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

By default this documents `filter`, `sort`, `page`, and `pageSize`.

Use the configuration callback to align names with `AddRsqlQueryRequest(...)` or to document only the parts an endpoint supports:

```csharp
app.MapGet("/products/search", HandleSearch)
    .WithRsqlQueryParameters(options =>
    {
        options.FilterParameterName = "q";
        options.IncludeSort = false;
        options.IncludePagination = false;
    });
```

## API

| Member | Description |
| --- | --- |
| `RsqlOpenApiEndpointConventionBuilderExtensions.WithRsqlQueryParameters<TBuilder>(this TBuilder, Action<RsqlOpenApiQueryOptions>?)` | Endpoint convention builder extension that registers an OpenAPI operation transformer documenting the RSQL query parameters. Returns the builder. |
| `RsqlOpenApiOperationDocumenter.Apply(OpenApiOperation, RsqlOpenApiQueryOptions?)` | Adds the configured query parameters to an `OpenApiOperation`. Use this to compose RSQL documentation inside an application's own operation transformer. Skips parameters already present by name in the query. |
| `RsqlOpenApiQueryOptions` | Configures which parameters are documented, their names, and their descriptions. |

`RsqlOpenApiQueryOptions` properties:

| Property | Default | Purpose |
| --- | --- | --- |
| `IncludeFilter` | `true` | Document the filter parameter. |
| `IncludeSort` | `true` | Document the sort parameter. |
| `IncludePagination` | `true` | Document the page and page size parameters. |
| `FilterParameterName` | `RsqlQueryFilter.DefaultQueryParameterName` | Filter parameter name. |
| `SortParameterName` | `RsqlSortQuery.DefaultSortParameterName` | Sort parameter name. |
| `PageParameterName` | `RsqlPageQuery.DefaultPageParameterName` | Page parameter name. |
| `PageSizeParameterName` | `RsqlPageQuery.DefaultPageSizeParameterName` | Page size parameter name. |
| `FilterDescription` | RSQL filter example text | Filter parameter description. |
| `SortDescription` | Sort field example text | Sort parameter description. |
| `PageDescription` | `"One-based page number."` | Page parameter description. |
| `PageSizeDescription` | `"Number of items requested per page."` | Page size parameter description. |

## Documentation

- OpenAPI usage: [docs/openapi-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/openapi-usage.md)
- Main repository: [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet)
