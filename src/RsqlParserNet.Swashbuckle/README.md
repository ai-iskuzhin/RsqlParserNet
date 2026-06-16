# RsqlParserNet.Swashbuckle

[![RsqlParserNet.Swashbuckle NuGet](https://img.shields.io/nuget/v/RsqlParserNet.Swashbuckle?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle)
[![RsqlParserNet.Swashbuckle Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.Swashbuckle?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle)

Swashbuckle (SwaggerGen) operation filters that document the `filter`, `sort`, `page`, and `pageSize` query parameters in generated OpenAPI documents. Builds on `RsqlParserNet.AspNetCore`.

## Family

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. It documents the query contract bound by [`RsqlParserNet.AspNetCore`](https://www.nuget.org/packages/RsqlParserNet.AspNetCore), which in turn builds on the [`RsqlParserNet`](https://www.nuget.org/packages/RsqlParserNet) core parser. Use [`RsqlParserNet.OpenApi`](https://www.nuget.org/packages/RsqlParserNet.OpenApi) instead when your application uses ASP.NET Core's built-in OpenAPI document generation, or [`RsqlParserNet.NSwag`](https://www.nuget.org/packages/RsqlParserNet.NSwag) when it generates documents through NSwag.

## Installation

```bash
dotnet add package RsqlParserNet.Swashbuckle
```

## Quick start

Register the operation filter once on SwaggerGen, then mark the endpoints that accept RSQL query parameters:

```csharp
using RsqlParserNet.Swashbuckle;

builder.Services.AddSwaggerGen(options =>
{
    options.AddRsqlQueryParametersOperationFilter();
});

app.MapGet("/products", HandleProducts)
    .WithRsqlSwaggerQueryParameters();
```

This documents `filter`, `sort`, `page`, and `pageSize` as optional query parameters on the marked endpoint.

Custom parameter names and partial contracts are configured per endpoint. Keep names aligned with your `AddRsqlQueryRequest(...)` options:

```csharp
app.MapGet("/products/search", HandleSearch)
    .WithRsqlSwaggerQueryParameters(options =>
    {
        options.FilterParameterName = "q";
        options.SortParameterName = "orderBy";
        options.IncludeSort = false;
        options.IncludePagination = false;
    });
```

For small APIs where every list endpoint shares the same contract, document all operations at once instead of marking each endpoint:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddRsqlQueryParametersToAllOperations();
});
```

Endpoint-scoped documentation is the safer default for larger APIs because it avoids advertising query parameters on endpoints that do not accept them.

## API

| Member | Kind | Description |
| --- | --- | --- |
| `RsqlSwaggerGenOptionsExtensions.AddRsqlQueryParametersOperationFilter(this SwaggerGenOptions)` | Extension method | Registers the operation filter that documents only endpoints marked with `WithRsqlSwaggerQueryParameters()`. |
| `RsqlSwaggerGenOptionsExtensions.AddRsqlQueryParametersToAllOperations(this SwaggerGenOptions, Action<RsqlOpenApiQueryOptions>?)` | Extension method | Registers the operation filter that documents RSQL query parameters on every operation, with optional shared configuration. |
| `RsqlSwaggerEndpointConventionBuilderExtensions.WithRsqlSwaggerQueryParameters<TBuilder>(this TBuilder, Action<RsqlOpenApiQueryOptions>?)` | Extension method | Marks an endpoint so the scoped operation filter documents its query parameters, with optional per-endpoint configuration. |
| `RsqlSwaggerQueryParametersOperationFilter` | `IOperationFilter` | Documents query parameters for operations carrying `RsqlSwaggerQueryMetadata`. |
| `RsqlSwaggerAllQueryParametersOperationFilter` | `IOperationFilter` | Documents query parameters on every generated operation. |
| `RsqlSwaggerQueryMetadata` | Endpoint metadata record | Carries the per-endpoint `RsqlOpenApiQueryOptions` attached by `WithRsqlSwaggerQueryParameters()`. |

Configuration is shared with `RsqlParserNet.OpenApi` through `RsqlOpenApiQueryOptions`: `IncludeFilter`, `IncludeSort`, `IncludePagination`, the `*ParameterName` properties, and the `*Description` properties.

## Documentation

- [Swashbuckle usage](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/swashbuckle-usage.md)
- [Main repository](https://github.com/ai-iskuzhin/RsqlParserNet)
