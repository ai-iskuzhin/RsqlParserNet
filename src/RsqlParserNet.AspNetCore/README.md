# RsqlParserNet.AspNetCore

[![RsqlParserNet.AspNetCore NuGet](https://img.shields.io/nuget/v/RsqlParserNet.AspNetCore?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.AspNetCore)
[![RsqlParserNet.AspNetCore Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.AspNetCore?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.AspNetCore)

Bindable ASP.NET Core query-string models for `filter`, `sort`, `page`, and `pageSize`, plus validation-error projection. Minimal APIs accept `RsqlQueryRequest` directly to reuse parsed filter, sort, and page state.

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. It builds on [`RsqlParserNet`](https://www.nuget.org/packages/RsqlParserNet) (parsing, diagnostics, the typed AST) and [`RsqlParserNet.Linq`](https://www.nuget.org/packages/RsqlParserNet.Linq) (allowlisted filter/sort translation and pagination models). [`RsqlParserNet.FastEndpoints`](https://www.nuget.org/packages/RsqlParserNet.FastEndpoints) builds on this package to add FastEndpoints validation glue.

## Installation

```bash
dotnet add package RsqlParserNet.AspNetCore
```

## Quick start

Register binding options once, then bind `RsqlQueryRequest` in a minimal API endpoint. Async paging via `ToRsqlPageAsync` comes from [`RsqlParserNet.EntityFrameworkCore`](https://www.nuget.org/packages/RsqlParserNet.EntityFrameworkCore).

```csharp
using Microsoft.EntityFrameworkCore;
using RsqlParserNet;
using RsqlParserNet.AspNetCore;
using RsqlParserNet.EntityFrameworkCore;
using RsqlParserNet.Linq;

builder.Services.AddRsqlQueryRequest(
    configureFilter: options =>
    {
        options.ParseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
    },
    configurePage: options => options.MaxPageSize = 100);

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

The default query contract is:

```text
GET /products?filter=status==active&sort=-createdAt&page=1&pageSize=25
```

Invalid filter, sort, page, or page size values do not fail binding. The bound request becomes invalid (`IsValid` is `false`), and the endpoint decides how to return the error.

`TryApplyTo` is the safest endpoint path when query semantics depend on a LINQ profile. It catches adapter translation errors such as unknown selectors, unsupported operators, or value conversion failures and returns them through the same structured error model.

## Query models

| Type | Purpose |
| --- | --- |
| `RsqlQueryRequest` | Combined filter, sort, and page state bound from the request. Exposes `IsValid`, `Filter`, `Sort`, `Page`, `PageRequest`, `ApplyTo`, `TryApplyTo`, and the error/validation helpers. |
| `RsqlQueryFilter` | The bound RSQL filter (`filter` by default). Exposes `IsValid`, `HasQuery`, `Query`, `Diagnostics`, and a static `Parse` for non-minimal-API frameworks. |
| `RsqlSortQuery` | The bound sort request (`sort` by default). Exposes `HasRequest`, `Requests`, and `IsValid`. |
| `RsqlPageQuery` | The bound page state (`page` and `pageSize` by default), with default and maximum page-size clamping. Exposes `IsValid` and `Request`. |

Bind a single component directly when an endpoint only needs part of the contract. Use the static `RsqlQueryFilter.Parse(...)` when a framework does not use minimal API parameter binding:

```csharp
var parseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
var filter = RsqlQueryFilter.Parse(httpContext.Request.Query["filter"].FirstOrDefault(), parseOptions);

if (filter.IsValid && filter.HasQuery)
{
    query = query.ApplyRsql(filter.Query!, ProductRsqlProfile.Instance);
}
```

Register binding options with `AddRsqlQueryRequest` (filter, sort, and page together) or with `AddRsqlQueryFilter`, `AddRsqlSortQuery`, and `AddRsqlPageQuery` individually.

## Validation errors

`GetErrors()` returns structured `RsqlQueryError` values for custom API error shapes. Each error carries:

| Field | Meaning |
| --- | --- |
| `ParameterName` | The query string parameter that produced the error. |
| `Message` | The human-readable error message. |
| `Source` | The originating component: `RsqlQueryErrorSource.Filter`, `.Sort`, or `.Page`. |
| `Code` | An optional stable diagnostic code. |
| `Span`, `Start`, `End` | Optional parser source span and locations for filter diagnostics. |

To return responses through the ASP.NET Core validation pipeline:

```csharp
// Dictionary<string, string[]> keyed by query parameter name.
return Results.ValidationProblem(request.ToValidationErrors());

// Or the same errors as HttpValidationProblemDetails (filter/translation errors also
// surfaced under the "rsqlErrors" extension).
return Results.Problem(request.ToValidationProblemDetails());
```

Both `ToValidationErrors()` and `ToValidationProblemDetails()` have overloads that accept an `IReadOnlyList<RsqlQueryError>`, so adapter translation errors from `TryApplyTo` flow through the same projection. Filter diagnostics keep the parser diagnostic code (for example `RSQL002`) in each message.

Error codes:

| Code | Meaning |
| --- | --- |
| `RSQL000`–`RSQL003` | Filter parse diagnostics from `RsqlParserNet` (empty expression, invalid token, unexpected token, invalid selector). |
| `RsqlQueryErrorCodes.AdapterTranslationError` (`RSQL100`) | A parsed query could not be translated with the configured LINQ profile. |

## Documentation

- Full ASP.NET Core usage guide: [docs/aspnet-core-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/aspnet-core-usage.md)
- Main repository: [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet)
