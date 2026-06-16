# RsqlParserNet.NSwag

[![RsqlParserNet.NSwag NuGet](https://img.shields.io/nuget/v/RsqlParserNet.NSwag?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.NSwag)
[![RsqlParserNet.NSwag Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.NSwag?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.NSwag)

NSwag operation processors that document the `filter`, `sort`, `page`, and `pageSize` query parameters in Swagger/OpenAPI documents generated through NSwag, including APIs that use `FastEndpoints.Swagger`.

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. It builds on [`RsqlParserNet.AspNetCore`](https://www.nuget.org/packages/RsqlParserNet.AspNetCore) (and the core [`RsqlParserNet`](https://www.nuget.org/packages/RsqlParserNet) parser) and references `NSwag.Generation.AspNetCore`. For other OpenAPI stacks, use [`RsqlParserNet.OpenApi`](https://www.nuget.org/packages/RsqlParserNet.OpenApi) for ASP.NET Core's built-in OpenAPI document generation, or [`RsqlParserNet.Swashbuckle`](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle) for Swashbuckle.

## Installation

```bash
dotnet add package RsqlParserNet.NSwag
```

## Quick start

Register the endpoint-scoped operation processor once on the NSwag document generator settings:

```csharp
using RsqlParserNet.NSwag;

builder.Services.AddOpenApiDocument(options =>
{
    options.AddRsqlQueryParametersOperationProcessor();
});
```

Then mark only the endpoints that accept RSQL query parameters:

```csharp
app.MapGet("/products", HandleProducts)
    .WithRsqlNSwagQueryParameters();
```

By default this documents the optional `filter`, `sort`, `page`, and `pageSize` query parameters.

Customize parameter names (keep them aligned with `AddRsqlQueryRequest(...)`) and document partial query contracts:

```csharp
app.MapGet("/products/search", HandleSearch)
    .WithRsqlNSwagQueryParameters(options =>
    {
        options.FilterParameterName = "q";
        options.IncludeSort = false;
        options.IncludePagination = false;
    });
```

For small APIs where every list endpoint shares the same query contract, document every operation with a single global processor instead:

```csharp
builder.Services.AddOpenApiDocument(options =>
{
    options.AddRsqlQueryParametersToAllOperations();
});
```

Endpoint-scoped documentation is the safer default for larger APIs because it avoids advertising query parameters on endpoints that do not accept them.

## API

| Member | Kind | Description |
| --- | --- | --- |
| `RsqlNSwagDocumentGeneratorSettingsExtensions.AddRsqlQueryParametersOperationProcessor(this TSettings)` | Settings extension | Registers an endpoint-scoped operation processor. Only endpoints marked with `WithRsqlNSwagQueryParameters(...)` are documented. |
| `RsqlNSwagDocumentGeneratorSettingsExtensions.AddRsqlQueryParametersToAllOperations(this TSettings, Action<RsqlOpenApiQueryOptions>?)` | Settings extension | Registers a processor that documents RSQL query parameters on every operation. |
| `RsqlNSwagEndpointConventionBuilderExtensions.WithRsqlNSwagQueryParameters(this TBuilder, Action<RsqlOpenApiQueryOptions>?)` | Endpoint extension | Marks an endpoint as accepting RSQL query parameters and configures which parameters and names to document. |
| `RsqlNSwagOperationDocumenter.Apply(OpenApiOperation, RsqlOpenApiQueryOptions?)` | Static helper | Adds configured RSQL query parameters to an NSwag operation. Use it to compose RSQL documentation inside your own operation processor. |
| `RsqlNSwagQueryParametersOperationProcessor` | Operation processor | The `IOperationProcessor` registered by `AddRsqlQueryParametersOperationProcessor`. Documents endpoints carrying RSQL metadata. |
| `RsqlNSwagAllQueryParametersOperationProcessor` | Operation processor | The `IOperationProcessor` registered by `AddRsqlQueryParametersToAllOperations`. Documents every operation. |
| `RsqlNSwagQueryMetadata` | Endpoint metadata | Carries the `RsqlOpenApiQueryOptions` attached by `WithRsqlNSwagQueryParameters`. |

Parameter names, descriptions, and inclusion flags (`FilterParameterName`, `SortParameterName`, `PageParameterName`, `PageSizeParameterName`, `IncludeFilter`, `IncludeSort`, `IncludePagination`, and related descriptions) come from `RsqlOpenApiQueryOptions` in `RsqlParserNet.OpenApi`.

## Documentation

- NSwag usage: [docs/nswag-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/nswag-usage.md)
- Main repository: [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet)
