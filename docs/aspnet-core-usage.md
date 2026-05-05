# ASP.NET Core Usage

`RsqlParserNet` and `RsqlParserNet.Linq` do not depend on ASP.NET Core. The optional `RsqlParserNet.AspNetCore` package adds a bindable query filter wrapper for ASP.NET Core request handling.

The recommended API flow is:

1. Bind the RSQL filter from a query string parameter.
2. Use a reusable `RsqlLinqProfile<T>` for the endpoint's allowlisted fields.
3. Return structured parser diagnostics for invalid filter syntax.
4. Apply the predicate before paging or materializing the query.

## Register Binding Options

Configure parser options once when the same custom operator set should be used by bound filters:

```csharp
using RsqlParserNet;
using RsqlParserNet.AspNetCore;

builder.Services.AddRsqlQueryRequest(
    configureFilter: options =>
    {
        options.QueryParameterName = "filter";
        options.ParseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
    },
    configureSort: options =>
    {
        options.SortParameterName = "sort";
    },
    configurePage: options =>
    {
        options.PageParameterName = "page";
        options.PageSizeParameterName = "pageSize";
        options.DefaultPageSize = 50;
        options.MaxPageSize = 100;
    });
```

## Minimal API Binding

```csharp
using Microsoft.EntityFrameworkCore;
using RsqlParserNet;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.EntityFrameworkCore;
using RsqlParserNet.Linq;

app.MapGet("/products", async (
    RsqlQueryRequest request,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    if (!request.IsValid)
    {
        return Results.ValidationProblem(request.ToValidationErrors());
    }

    if (!request.TryApplyTo(db.Products, ProductRsqlProfile.Instance, out var query, out var errors))
    {
        return Results.ValidationProblem(request.ToValidationErrors(errors));
    }

    query = request.Sort.HasRequest
        ? query
        : query.OrderBy(product => product.Id);

    var result = await query.ToRsqlPageAsync(request.PageRequest, cancellationToken);

    return Results.Ok(result);
});
```

`RsqlQueryRequest` uses `filter`, `sort`, `page`, and `pageSize` by default. Configure the individual option objects when an API uses different query parameter names.

Invalid filter, sort, page, or page size values do not fail binding. The bound request becomes invalid, and the endpoint decides how to return the error.

`ToValidationErrors()` returns ASP.NET Core validation problem fields keyed by query parameter name. Filter diagnostics keep the parser diagnostic code in each message.

Use `ToValidationProblemDetails()` when an API has a central ProblemDetails pipeline:

```csharp
if (!request.IsValid)
{
    return Results.Problem(request.ToValidationProblemDetails());
}
```

Use `GetErrors()` when the API needs its own error shape. It returns structured `RsqlQueryError` values with parameter name, message, source, optional diagnostic code, and parser source location when available.

`TryApplyTo()` is the safest endpoint path when query semantics depend on a LINQ profile. It catches adapter translation errors such as unknown selectors, unsupported operators, or value conversion failures and returns them through the same structured error model.

The individual binders `RsqlQueryFilter`, `RsqlSortQuery`, and `RsqlPageQuery` remain available when an endpoint only needs part of the query contract.

## Other Endpoint Frameworks

Use `RsqlQueryFilter.Parse` when a framework does not use Minimal API parameter binding:

```csharp
using RsqlParserNet;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.Linq;

var parseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
var filter = RsqlQueryFilter.Parse(httpContext.Request.Query["filter"].FirstOrDefault(), parseOptions);

if (!filter.IsValid)
{
    // Convert filter.ToValidationErrors() into the response style used by the framework.
}

if (filter.HasQuery)
{
    query = query.ApplyRsql(filter.Query!, ProductRsqlProfile.Instance);
}
```

The static `Parse` method keeps the same wrapper usable from controllers, endpoint filters, FastEndpoints handlers, or tests.

## FastEndpoints

Use `RsqlParserNet.FastEndpoints` when an API should add invalid RSQL input to FastEndpoints' validation failure flow. See [fastendpoints-usage.md](fastendpoints-usage.md).

## Profile

Keep profiles close to the endpoint contract, not necessarily the database entity. Only selectors listed here are available to clients.

```csharp
using RsqlParserNet;
using RsqlParserNet.Linq;

public sealed class ProductRsqlProfile : RsqlLinqProfile<Product>
{
    public static ProductRsqlProfile Instance { get; } = new();

    public override RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
    {
        return options.WithLinqOperators();
    }

    public override void Configure(RsqlLinqOptions<Product> options)
    {
        options.Allow("name", product => product.Name);
        options.Allow("status", product => product.Status);
        options.Allow("createdAt", product => product.CreatedAt);
        options.Allow("tags", product => product.Tags);
        options.AllowStringContainsOperator();
        options.AllowStringStartsWithOperator();
        options.AllowStringEndsWithOperator();
        options.AllowCollectionAnyOperator();
        options.AllowCollectionAllOperator();
    }
}
```

## Example Requests

```text
GET /products?filter=status==active
GET /products?filter=status=in=(active,draft);createdAt>=2026-01-01
GET /products?filter=name==Bike*
GET /products?filter=name=contains=ik
GET /products?filter=name=starts=Bi
GET /products?filter=tags=any=(outdoor,bike)
GET /products?filter=tags=all=(bike,outdoor)
GET /products?filter=status==active&page=2&pageSize=25
GET /products?filter=status==active&sort=-createdAt&page=2&pageSize=25
GET /products?filter=status==active&sort=-createdAt,name&page=2&pageSize=25
```

## Notes

- Apply RSQL filtering before paging.
- Keep maximum page sizes and server-side ordering outside the RSQL expression.
- Do not expose arbitrary property paths from client input.
- Prefer profiles over attribute discovery for public API filters.
- Return diagnostics to clients rather than raw exception messages.
