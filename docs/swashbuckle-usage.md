# Swashbuckle Usage

`RsqlParserNet.Swashbuckle` adds Swashbuckle operation filters for APIs that expose RSQL query parameters.

Use this package when an application generates Swagger/OpenAPI documents through Swashbuckle. Use `RsqlParserNet.OpenApi` instead when the application uses ASP.NET Core's built-in OpenAPI document generation.

## Endpoint-Scoped Documentation

Register the operation filter once:

```csharp
using RsqlParserNet.Swashbuckle;

builder.Services.AddSwaggerGen(options =>
{
    options.AddRsqlQueryParametersOperationFilter();
});
```

Mark only the endpoints that accept RSQL query parameters:

```csharp
app.MapGet("/products", HandleProducts)
    .WithRsqlSwaggerQueryParameters();
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
    .WithRsqlSwaggerQueryParameters(options =>
    {
        options.FilterParameterName = "q";
        options.SortParameterName = "orderBy";
        options.PageParameterName = "p";
        options.PageSizeParameterName = "take";
    });
```

## Document Every Operation

For small APIs where every list endpoint uses the same RSQL query contract, a global filter is available:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddRsqlQueryParametersToAllOperations();
});
```

Endpoint-scoped documentation is the safer default for larger APIs because it avoids advertising query parameters on endpoints that do not accept them.

## Partial Query Contracts

Endpoints can document only the query parts they support:

```csharp
app.MapGet("/products/search", HandleSearch)
    .WithRsqlSwaggerQueryParameters(options =>
    {
        options.IncludeSort = false;
        options.IncludePagination = false;
    });
```
