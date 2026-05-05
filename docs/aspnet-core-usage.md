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

builder.Services.AddRsqlQueryFilter(options =>
{
    options.QueryParameterName = "filter";
    options.ParseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
});
```

## Minimal API Binding

```csharp
using Microsoft.EntityFrameworkCore;
using RsqlParserNet;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.Linq;

app.MapGet("/products", async (
    RsqlQueryFilter filter,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    IQueryable<Product> query = db.Products;

    if (!filter.IsValid)
    {
        return Results.ValidationProblem(filter.ToValidationErrors());
    }

    if (filter.HasQuery)
    {
        query = query.ApplyRsql(filter.Query!, ProductRsqlProfile.Instance);
    }

    var products = await query
        .OrderBy(product => product.Id)
        .Take(100)
        .ToListAsync(cancellationToken);

    return Results.Ok(products);
});
```

`RsqlQueryFilter` reads `filter` by default. Set `RsqlQueryFilterOptions.QueryParameterName` when an API uses another query parameter name.

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

A future `RsqlParserNet.FastEndpoints` package can add FastEndpoints-specific helpers if there is enough repeated setup to justify the dependency. Until then, handlers can use the framework-neutral `RsqlQueryFilter.Parse` API:

```csharp
var parseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
var filter = RsqlQueryFilter.Parse(HttpContext.Request.Query["filter"].FirstOrDefault(), parseOptions);

if (!filter.IsValid)
{
    // Add filter.ToValidationErrors() to the endpoint's validation/error response.
}

if (filter.HasQuery)
{
    query = query.ApplyRsql(filter.Query!, ProductRsqlProfile.Instance);
}
```

Keep a FastEndpoints package separate from `RsqlParserNet.AspNetCore` if it adds framework-specific request DTO binding, validation failures, endpoint filters, or response helpers.

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
```

## Notes

- Apply RSQL filtering before paging.
- Keep maximum page sizes and server-side ordering outside the RSQL expression.
- Do not expose arbitrary property paths from client input.
- Prefer profiles over attribute discovery for public API filters.
- Return diagnostics to clients rather than raw exception messages.
