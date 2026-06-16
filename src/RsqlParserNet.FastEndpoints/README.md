# RsqlParserNet.FastEndpoints

[![RsqlParserNet.FastEndpoints NuGet](https://img.shields.io/nuget/v/RsqlParserNet.FastEndpoints?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.FastEndpoints)
[![RsqlParserNet.FastEndpoints Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.FastEndpoints?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.FastEndpoints)

FastEndpoints binding and validation glue for RSQL/FIQL-style REST API query expressions. It reuses the ASP.NET Core query models and translates invalid RSQL input into FastEndpoints `ValidationFailure` entries, so APIs keep their normal validation response flow.

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. This package builds on [`RsqlParserNet.AspNetCore`](https://www.nuget.org/packages/RsqlParserNet.AspNetCore), reusing its bindable `RsqlQueryRequest` model and option registration. Filtering and sorting still use explicit, allowlisted `RsqlLinqProfile<T>` mappings.

## Requirements

`RsqlParserNet.FastEndpoints` currently requires `FastEndpoints` `7.0.1` or newer. Applications on FastEndpoints 5.x or 6.x should either upgrade FastEndpoints before installing this adapter or use the framework-neutral ASP.NET Core query models directly through `RsqlQueryRequest.Parse(...)`.

FastEndpoints 7 applications that call `UseAuthorization()` must also register authorization services, for example with `builder.Services.AddAuthorization()`.

## Installation

```bash
dotnet add package RsqlParserNet.FastEndpoints
```

## Quick start

Register the shared query options (same registration as the ASP.NET Core adapter):

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

Bind and validate inside the endpoint. `BindRsqlQueryRequestAndAddErrors()` reads `filter`, `sort`, `page`, and `pageSize` from the current `HttpContext`, applies the configured option names, and adds failures when parsing or binding fails. `ThrowIfAnyErrors()` then returns FastEndpoints' normal validation response:

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

Filter diagnostics become `ValidationFailure` entries on the filter query parameter, with the parser diagnostic code preserved as the FastEndpoints error code. Sort and page failures are keyed by their query parameter names. Call `TryApplyTo()` before materializing the query so unknown selectors, unsupported operators, or value conversion failures become validation failures instead of unhandled exceptions.

For a custom response shape, inspect the bound request directly:

```csharp
var rsql = this.BindRsqlQueryRequest();

if (!rsql.IsValid)
{
    var errors = rsql.GetErrors();
    // Map errors to the API's response contract.
}
```

## API

All members are extension methods on `RsqlFastEndpointExtensions`.

| Method | Description |
| --- | --- |
| `IEndpoint.BindRsqlQueryRequest()` | Binds `filter`, `sort`, `page`, and `pageSize` from the endpoint's `HttpContext` using the configured option names and returns the `RsqlQueryRequest`. Does not add failures. |
| `IEndpoint.BindRsqlQueryRequestAndAddErrors()` | Binds the request and adds any RSQL binding/parse failures to the endpoint. Returns the `RsqlQueryRequest`. Call `ThrowIfAnyErrors()` afterward. |
| `IEndpoint.AddRsqlValidationFailures(RsqlQueryRequest request)` | Adds the request's RSQL errors to the endpoint's `ValidationFailures`. |
| `IEndpoint.AddRsqlValidationFailures(IReadOnlyList<RsqlQueryError> errors)` | Adds structured RSQL errors (for example from `TryApplyTo`) to the endpoint's `ValidationFailures`. |
| `RsqlQueryRequest.ToFastEndpointValidationFailures()` | Converts the request's RSQL binding errors into a list of `ValidationFailure`. |
| `IReadOnlyList<RsqlQueryError>.ToFastEndpointValidationFailures()` | Converts structured RSQL binding or translation errors into a list of `ValidationFailure`. |

## Documentation

- FastEndpoints usage: [docs/fastendpoints-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/fastendpoints-usage.md)
- Main repository: [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet)
