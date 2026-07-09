# FastEndpoints Usage

`RsqlParserNet.FastEndpoints` adds thin helpers for APIs built with FastEndpoints. It reuses the ASP.NET Core query binding models and translates invalid RSQL input into FastEndpoints validation failures.

The package does not replace explicit allowlisted profiles. Filtering and sorting still use `RsqlLinqProfile<T>`.

## Compatibility

`RsqlParserNet.FastEndpoints` references `FastEndpoints` `8.0.0` or newer. Applications on FastEndpoints 7.x or earlier should either upgrade FastEndpoints before installing this adapter, stay on `RsqlParserNet.FastEndpoints` `1.0.3`, or use the framework-neutral `RsqlQueryRequest.Parse(...)` / ASP.NET Core models directly.

FastEndpoints 7 applications that call `UseAuthorization()` must also register authorization services, for example with `builder.Services.AddAuthorization()`.

## Register Options

Use the same option registration as the ASP.NET Core adapter:

```csharp
using RsqlParserNet;
using RsqlParserNet.AspNetCore;

builder.Services.AddRsqlQueryRequest(
    configureFilter: options =>
    {
        options.QueryParameterName = "filter";
        options.ParseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
    },
    configureSort: options => options.SortParameterName = "sort",
    configurePage: options =>
    {
        options.DefaultPageSize = 50;
        options.MaxPageSize = 100;
    });
```

## Endpoint Example

```csharp
using FastEndpoints;
using RsqlParserNet.EntityFrameworkCore;
using RsqlParserNet.FastEndpoints;
using RsqlParserNet.Linq;

public sealed class ListProductsEndpoint : EndpointWithoutRequest<RsqlPagedResult<ProductResponse>>
{
    private readonly AppDbContext _db;

    public ListProductsEndpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/products");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var rsql = this.BindRsqlQueryRequestAndAddErrors();
        ThrowIfAnyErrors();

        if (!rsql.TryApplyTo(_db.Products, ProductRsqlProfile.Instance, out var query, out var errors))
        {
            this.AddRsqlValidationFailures(errors);
            ThrowIfAnyErrors();
        }

        query = rsql.Sort.HasRequest
            ? query
            : query.OrderBy(product => product.Id);

        var result = await query
            .Select(product => new ProductResponse(product.Id, product.Name))
            .ToRsqlPageAsync(rsql.PageRequest, cancellationToken);

        await Send.OkAsync(result, cancellationToken);
    }
}
```

`BindRsqlQueryRequestAndAddErrors()` reads `filter`, `sort`, `page`, and `pageSize` from the current `HttpContext`, applies configured query option names, and adds failures to the endpoint when parsing or binding fails. Call `ThrowIfAnyErrors()` afterward when the endpoint should return FastEndpoints' normal validation response.

Use `BindRsqlQueryRequest()` when the endpoint wants to inspect the request before adding failures, and `AddRsqlValidationFailures(request)` when validation should be added later.

Use `TryApplyTo()` before materializing the query when unknown selectors, unsupported operators, or value conversion failures should become validation failures instead of unhandled exceptions.

## Validation

Filter diagnostics become `ValidationFailure` entries on the filter query parameter. The parser diagnostic code is preserved as the FastEndpoints error code.

Sort and page failures become `ValidationFailure` entries keyed by their query parameter names.

For a custom response shape, use the shared ASP.NET Core model directly:

```csharp
var rsql = this.BindRsqlQueryRequest();

if (!rsql.IsValid)
{
    var errors = rsql.GetErrors();
    // Map errors to the API's response contract.
}
```

The helper package does not force a response body. It only bridges RSQL binding errors into FastEndpoints validation when asked.
