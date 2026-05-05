# ASP.NET Core Usage

`RsqlParserNet` and `RsqlParserNet.Linq` do not depend on ASP.NET Core, but they can be used directly from controllers, minimal APIs, endpoint filters, or FastEndpoints handlers.

The recommended API flow is:

1. Accept the RSQL filter as a query string parameter.
2. Use a reusable `RsqlLinqProfile<T>` for the endpoint's allowlisted fields.
3. Return structured parser diagnostics for invalid filter syntax.
4. Apply the predicate before paging or materializing the query.

## Minimal API

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RsqlParserNet;
using RsqlParserNet.Linq;

app.MapGet("/products", async (
    [FromQuery(Name = "filter")] string? filter,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    IQueryable<Product> query = db.Products;

    if (!string.IsNullOrWhiteSpace(filter))
    {
        var parseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
        var parsed = RsqlParser.TryParse(filter, parseOptions);

        if (!parsed.Success)
        {
            return Results.ValidationProblem(
                parsed.Diagnostics.ToDictionary(
                    diagnostic => diagnostic.Code,
                    diagnostic => new[] { diagnostic.Message }));
        }

        query = query.ApplyRsql(parsed.Query!, ProductRsqlProfile.Instance);
    }

    var products = await query
        .OrderBy(product => product.Id)
        .Take(100)
        .ToListAsync(cancellationToken);

    return Results.Ok(products);
});
```

## Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RsqlParserNet;
using RsqlParserNet.Linq;

[ApiController]
[Route("products")]
public sealed class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> Get(
        [FromQuery(Name = "filter")] string? filter,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = _db.Products;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var parseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
            var parsed = RsqlParser.TryParse(filter, parseOptions);

            if (!parsed.Success)
            {
                foreach (var diagnostic in parsed.Diagnostics)
                {
                    ModelState.AddModelError(diagnostic.Code, diagnostic.Message);
                }

                return ValidationProblem(ModelState);
            }

            query = query.ApplyRsql(parsed.Query!, ProductRsqlProfile.Instance);
        }

        return await query
            .OrderBy(product => product.Id)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
}
```

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
        var customOperators = options.CustomOperators.ToList();
        AddCustomOperator(customOperators, new RsqlCustomOperator("=contains="));
        AddCustomOperator(customOperators, new RsqlCustomOperator("=any=", RequiresMultipleValues: true));
        AddCustomOperator(customOperators, new RsqlCustomOperator("=all=", RequiresMultipleValues: true));

        return options with { CustomOperators = customOperators };
    }

    public override void Configure(RsqlLinqOptions<Product> options)
    {
        options.Allow("name", product => product.Name);
        options.Allow("status", product => product.Status);
        options.Allow("createdAt", product => product.CreatedAt);
        options.Allow("tags", product => product.Tags);
        options.AllowStringContainsOperator();
        options.AllowCollectionAnyOperator();
        options.AllowCollectionAllOperator();
    }

    private static void AddCustomOperator(
        List<RsqlCustomOperator> customOperators,
        RsqlCustomOperator customOperator)
    {
        if (customOperators.All(item => item.Text != customOperator.Text))
        {
            customOperators.Add(customOperator);
        }
    }
}
```

## Example Requests

```text
GET /products?filter=status==active
GET /products?filter=status=in=(active,draft);createdAt>=2026-01-01
GET /products?filter=name==Bike*
GET /products?filter=name=contains=ik
GET /products?filter=tags=any=(outdoor,bike)
GET /products?filter=tags=all=(bike,outdoor)
```

## Notes

- Apply RSQL filtering before paging.
- Keep maximum page sizes and server-side ordering outside the RSQL expression.
- Do not expose arbitrary property paths from client input.
- Prefer profiles over attribute discovery for public API filters.
- Return diagnostics to clients rather than raw exception messages.
