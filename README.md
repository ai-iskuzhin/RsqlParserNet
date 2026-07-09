# RsqlParserNet

[![CI](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/ci.yml)
[![Release](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/release.yml/badge.svg)](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/release.yml)
[![Publish NuGet](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/publish-nuget.yml)
[![Publish GitHub Packages](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/publish-github-packages.yml/badge.svg)](https://github.com/ai-iskuzhin/RsqlParserNet/actions/workflows/publish-github-packages.yml)
[![License](https://img.shields.io/github/license/ai-iskuzhin/RsqlParserNet?style=flat-square)](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&style=flat-square)](https://dotnet.microsoft.com/)
[![Coverage](https://img.shields.io/badge/coverage-not%20published-lightgrey?style=flat-square)](#development)

| Package | Latest Release | Downloads |
| :--- | :---: | :---: |
| `RsqlParserNet` | [![RsqlParserNet NuGet](https://img.shields.io/nuget/v/RsqlParserNet?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet) | [![RsqlParserNet Downloads](https://img.shields.io/nuget/dt/RsqlParserNet?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet) |
| `RsqlParserNet.Linq` | [![RsqlParserNet.Linq NuGet](https://img.shields.io/nuget/v/RsqlParserNet.Linq?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Linq) | [![RsqlParserNet.Linq Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.Linq?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Linq) |
| `RsqlParserNet.AspNetCore` | [![RsqlParserNet.AspNetCore NuGet](https://img.shields.io/nuget/v/RsqlParserNet.AspNetCore?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.AspNetCore) | [![RsqlParserNet.AspNetCore Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.AspNetCore?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.AspNetCore) |
| `RsqlParserNet.EntityFrameworkCore` | [![RsqlParserNet.EntityFrameworkCore NuGet](https://img.shields.io/nuget/v/RsqlParserNet.EntityFrameworkCore?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.EntityFrameworkCore) | [![RsqlParserNet.EntityFrameworkCore Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.EntityFrameworkCore?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.EntityFrameworkCore) |
| `RsqlParserNet.FastEndpoints` | [![RsqlParserNet.FastEndpoints NuGet](https://img.shields.io/nuget/v/RsqlParserNet.FastEndpoints?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.FastEndpoints) | [![RsqlParserNet.FastEndpoints Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.FastEndpoints?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.FastEndpoints) |
| `RsqlParserNet.OpenApi` | [![RsqlParserNet.OpenApi NuGet](https://img.shields.io/nuget/v/RsqlParserNet.OpenApi?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.OpenApi) | [![RsqlParserNet.OpenApi Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.OpenApi?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.OpenApi) |
| `RsqlParserNet.Swashbuckle` | [![RsqlParserNet.Swashbuckle NuGet](https://img.shields.io/nuget/v/RsqlParserNet.Swashbuckle?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle) | [![RsqlParserNet.Swashbuckle Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.Swashbuckle?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle) |
| `RsqlParserNet.NSwag` | [![RsqlParserNet.NSwag NuGet](https://img.shields.io/nuget/v/RsqlParserNet.NSwag?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.NSwag) | [![RsqlParserNet.NSwag Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.NSwag?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.NSwag) |

A dependency-light .NET parser for RSQL/FIQL-style REST API query expressions.

`RsqlParserNet` parses query text into a typed AST with source spans and structured diagnostics. The core package does not depend on ASP.NET Core, LINQ, Entity Framework Core, or ORM APIs.

Current stable release: `1.1.0`.

## Ready-To-Use API Query Models

The package family includes ready-to-use primitives for common REST API list endpoints, so applications do not have to rebuild filter, sort, page, validation, and response-shape plumbing from scratch.

| Need | Ready-to-use type or helper |
| --- | --- |
| Parse `filter=...` into structured syntax | `RsqlQuery`, `RsqlParser.TryParse(...)`, `RsqlQueryFilter` |
| Bind `filter`, `sort`, `page`, and `pageSize` in ASP.NET Core | `RsqlQueryRequest` |
| Return validation-style query errors | `RsqlQueryError`, `ToValidationErrors()`, `ToValidationProblemDetails()` |
| Apply allowlisted filtering and sorting | `RsqlLinqProfile<T>`, `ApplyRsql(...)`, `ApplySort(...)`, `TryApplyTo(...)` |
| Represent page input | `RsqlPageRequest` |
| Represent paged response metadata | `RsqlPagination` |
| Return a consistent paged response | `RsqlPagedResult<T>` |
| Count, page, and materialize EF Core queries | `ToRsqlPageAsync(...)` |
| Document query parameters in OpenAPI | `RsqlParserNet.OpenApi`, `RsqlParserNet.Swashbuckle`, `RsqlParserNet.NSwag` |

The default query contract is:

```text
GET /products?filter=status==active&sort=-createdAt&page=1&pageSize=25
```

The default paged response shape is:

```json
{
  "items": [],
  "pagination": {
    "page": 1,
    "pageSize": 25,
    "totalItems": 42,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

Selectors are still explicit and safe by design: applications choose which fields are available through allowlisted mappings.

## Installation

Choose the smallest package that owns the behavior you need:

| Package | Use it when |
| --- | --- |
| `RsqlParserNet` | You only need parsing, diagnostics, and the typed AST. |
| `RsqlParserNet.Linq` | You want allowlisted expression tree predicates for `IQueryable<T>` or `IEnumerable<T>`. |
| `RsqlParserNet.AspNetCore` | You want bindable query-string models for `filter`, `sort`, `page`, and `pageSize`. |
| `RsqlParserNet.EntityFrameworkCore` | You want EF Core async paging helpers over already-composed queries. |
| `RsqlParserNet.FastEndpoints` | You want FastEndpoints binding and validation failure glue. |
| `RsqlParserNet.OpenApi` | You want ASP.NET Core built-in OpenAPI query parameter documentation. |
| `RsqlParserNet.Swashbuckle` | You want Swashbuckle operation filters. |
| `RsqlParserNet.NSwag` | You want NSwag operation processors. |

Selectors are parsed as text. They are not reflected onto entity properties by the core package. Application code and adapters should explicitly allowlist the selectors they accept.

Core parser:

```bash
dotnet add package RsqlParserNet
```

LINQ adapter:

```bash
dotnet add package RsqlParserNet.Linq
```

ASP.NET Core binding:

```bash
dotnet add package RsqlParserNet.AspNetCore
```

Entity Framework Core helpers:

```bash
dotnet add package RsqlParserNet.EntityFrameworkCore
```

FastEndpoints helpers:

```bash
dotnet add package RsqlParserNet.FastEndpoints
```

`RsqlParserNet.FastEndpoints` requires `FastEndpoints` `8.0.0` or newer. Applications on older FastEndpoints versions can stay on `RsqlParserNet.FastEndpoints` `1.0.3`, or use the ASP.NET Core query models directly through `RsqlQueryRequest.Parse(...)`.

OpenAPI helpers:

```bash
dotnet add package RsqlParserNet.OpenApi
```

Swashbuckle helpers:

```bash
dotnet add package RsqlParserNet.Swashbuckle
```

NSwag helpers:

```bash
dotnet add package RsqlParserNet.NSwag
```

For local development, reference the project directly:

```xml
<ProjectReference Include="src/RsqlParserNet/RsqlParserNet.csproj" />
<ProjectReference Include="src/RsqlParserNet.Linq/RsqlParserNet.Linq.csproj" />
<ProjectReference Include="src/RsqlParserNet.AspNetCore/RsqlParserNet.AspNetCore.csproj" />
<ProjectReference Include="src/RsqlParserNet.EntityFrameworkCore/RsqlParserNet.EntityFrameworkCore.csproj" />
<ProjectReference Include="src/RsqlParserNet.FastEndpoints/RsqlParserNet.FastEndpoints.csproj" />
<ProjectReference Include="src/RsqlParserNet.OpenApi/RsqlParserNet.OpenApi.csproj" />
<ProjectReference Include="src/RsqlParserNet.Swashbuckle/RsqlParserNet.Swashbuckle.csproj" />
<ProjectReference Include="src/RsqlParserNet.NSwag/RsqlParserNet.NSwag.csproj" />
```

## Quick Start

```csharp
using RsqlParserNet;

var query = RsqlParser.Parse("""status=="active";title=="Bike*"""");

foreach (var comparison in query.Root.Comparisons())
{
    Console.WriteLine($"{comparison.Selector} {comparison.Operator} {comparison.Values[0].Text}");
}
```

For non-exception flows, use `TryParse`:

```csharp
var result = RsqlParser.TryParse("status==");

if (!result.Success)
{
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message}");
    }
}
```

## Supported Syntax

Common RSQL/FIQL forms are supported:

```text
status==active
status=="active"
title=="SUP*"
status=in=(active,draft)
createdAt>=2026-01-01
status==active;title=="SUP*"
status==active,title=="Bike*"
(status==active;title=="SUP*"),status==draft
```

Logical operators:

```text
; = AND
, = OR
```

AND has higher precedence than OR. Parentheses can override precedence.

See [docs/syntax.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/syntax.md) for the full grammar, selector rules, value behavior, custom operators, and wildcard/date semantics.

## Options

Parser behavior can be configured:

```csharp
var options = RsqlParseOptions.Default with
{
    AllowWordLogicalOperators = false,
    AllowDottedSelectors = true,
    CustomOperators =
    [
        new RsqlCustomOperator("=contains="),
        new RsqlCustomOperator("=all=", RequiresMultipleValues: true)
    ]
};

var result = RsqlParser.TryParse("title=contains=Bike", options);
```

Custom operators must use FIQL-style syntax such as `=contains=`, must not duplicate built-in operators, and must not be configured more than once.

## Diagnostics

Parse errors are returned as structured diagnostics:

```csharp
var result = RsqlParser.TryParse("status==");
var diagnostic = result.Diagnostics[0];

Console.WriteLine(diagnostic.Code);
Console.WriteLine(diagnostic.Message);
Console.WriteLine(diagnostic.Span.Start);
Console.WriteLine(diagnostic.Start.Line);
Console.WriteLine(diagnostic.Start.Column);
```

Current diagnostic codes:

| Code | Meaning |
| --- | --- |
| `RSQL000` | Empty expression |
| `RSQL001` | Invalid token or unterminated quoted string |
| `RSQL002` | Unexpected token or missing syntax |
| `RSQL003` | Invalid selector |

`RsqlParser.Parse` throws `ArgumentException` for empty input and `RsqlParseException` for invalid syntax. `RsqlParseException.Diagnostics` contains the same structured diagnostics returned by `TryParse`.

For API responses, diagnostics can be projected without depending on any ASP.NET Core adapter:

```csharp
var result = RsqlParser.TryParse(filter);

if (!result.Success)
{
    var errors = result.Diagnostics.Select(diagnostic => new
    {
        diagnostic.Code,
        diagnostic.Message,
        diagnostic.Start.Line,
        diagnostic.Start.Column
    });

    return Results.BadRequest(errors);
}
```

## Core AST Model

The core parser returns a typed AST and does not assign query semantics to application fields:

| Type | Key properties | Meaning |
| --- | --- | --- |
| `RsqlQuery` | `Expression`, `Root` | Parsed query text and root AST node. |
| `RsqlComparisonNode` | `Selector`, `Operator`, `OperatorText`, `Values` | A selector/operator/value comparison such as `status==active`. |
| `RsqlLogicalNode` | `Operator`, `Children` | An AND/OR group. `;` maps to AND and `,` maps to OR. |
| `RsqlValue` | `Text`, `RawText`, `Kind`, `Span` | Parsed value text, original source text, classified value kind, and source span. |
| `RsqlDiagnostic` | `Code`, `Message`, `Span`, `Start`, `End` | Structured parse error with source location. |

Use `RsqlNodeExtensions.Comparisons()` when you only need to inspect every comparison node:

```csharp
var query = RsqlParser.Parse("status==active;title==Bike");

foreach (var comparison in query.Root.Comparisons())
{
    Console.WriteLine($"{comparison.Selector}: {comparison.Values[0].Text}");
}
```

## Manual AST Filtering

Use the core package directly when an endpoint supports a small, controlled subset and does not need `RsqlParserNet.Linq`.

This example supports only:

- AND groups
- `==`
- exactly one value
- explicitly allowlisted selectors

```csharp
using RsqlParserNet;

static IQueryable<Product> ApplyProductFilter(IQueryable<Product> query, RsqlNode node)
{
    return node switch
    {
        RsqlComparisonNode comparison => ApplyProductComparison(query, comparison),
        RsqlLogicalNode { Operator: RsqlLogicalOperator.And } logical =>
            logical.Children.Aggregate(query, ApplyProductFilter),
        RsqlLogicalNode { Operator: RsqlLogicalOperator.Or } =>
            throw new NotSupportedException("OR is not supported by this endpoint."),
        _ => throw new NotSupportedException("Unsupported RSQL node.")
    };
}

static IQueryable<Product> ApplyProductComparison(
    IQueryable<Product> query,
    RsqlComparisonNode comparison)
{
    if (comparison.Operator is not RsqlComparisonOperator.Equal)
    {
        throw new NotSupportedException("Only == is supported by this endpoint.");
    }

    if (comparison.Values.Count is not 1)
    {
        throw new NotSupportedException("Only one comparison value is supported.");
    }

    var value = comparison.Values[0].Text
        ?? throw new NotSupportedException("Null values are not supported by this endpoint.");

    return comparison.Selector switch
    {
        "status" => query.Where(product => product.Status == value),
        "title" => query.Where(product => product.Title == value),
        _ => throw new ArgumentException($"Unsupported filter field '{comparison.Selector}'.")
    };
}
```

Endpoint code can parse first, return syntax diagnostics, and then apply the allowlisted subset:

```csharp
var result = RsqlParser.TryParse(filter);

if (!result.Success)
{
    return Results.BadRequest(result.Diagnostics.Select(diagnostic => new
    {
        diagnostic.Code,
        diagnostic.Message,
        diagnostic.Start.Line,
        diagnostic.Start.Column
    }));
}

var products = ApplyProductFilter(db.Products, result.Query!.Root);
```

## LINQ Adapter

The repository includes an early `RsqlParserNet.Linq` adapter project. It translates parsed AST nodes into expression tree predicates using explicit selector mappings:

```csharp
using RsqlParserNet;
using RsqlParserNet.Linq;

var query = RsqlParser.Parse("status=in=(active,draft);count>=10");

var filtered = products.ApplyRsql(query, options =>
{
    options.Allow("status", x => x.Status);
    options.Allow("count", x => x.Count);
});
```

You can also build the predicate separately when another layer should decide how to compose or apply it:

```csharp
var predicate = RsqlPredicateBuilder.BuildPredicate<Product>("status==active", options =>
{
    options.Allow("status", x => x.Status);
});

var filtered = products.Where(predicate);
```

For repeated endpoint/query contracts, put mappings in a reusable profile:

```csharp
public sealed class ProductRsqlProfile : RsqlLinqProfile<Product>
{
    public override RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
    {
        return options.WithLinqOperators();
    }

    public override void Configure(RsqlLinqOptions<Product> options)
    {
        options.Allow("name", x => x.Name);
        options.Allow("status", x => x.Status);
        options.Allow("count", x => x.Count);
        options.Allow("tags", x => x.Tags);
        options.AllowStringContainsOperator();
        options.AllowStringStartsWithOperator();
        options.AllowStringEndsWithOperator();
        options.AllowCollectionAnyOperator();
        options.AllowCollectionAllOperator();
    }
}

var filtered = products.ApplyRsql("status==active;name=contains=ik", new ProductRsqlProfile());
```

The adapter currently supports:

| Operator | LINQ behavior |
| --- | --- |
| `==` / `!=` | Equality and inequality |
| `>` / `>=` / `<` / `<=` | Comparable mapped types such as numbers and dates |
| `=in=` / `=out=` | Membership checks over the supplied value list |
| `=contains=` / `=starts=` / `=ends=` | Custom string helpers for allowlisted string fields |
| `=any=` / `=all=` | Custom collection helpers for allowlisted collection fields |
| `;` / `,` | `AndAlso` and `OrElse` expression composition |

Values are converted using the mapped member type, including common scalar types, enums, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`. `DateTimeOffset` literals are normalized to UTC by default before expression constants are built, which keeps EF Core/Npgsql queries compatible with PostgreSQL `timestamp with time zone` parameters.

String helper operators and wildcard equality use the LINQ provider's string comparison behavior by default. APIs can opt into endpoint-wide case-insensitive string matching through `RsqlLinqOptions<T>.StringComparisonMode`.

APIs that need application-specific literal handling can set `RsqlLinqOptions<T>.NormalizeValue`; APIs that need to preserve parsed date-time offsets can set `NormalizeDateTimeOffsetsToUtc` to `false`.

Custom operators can be translated explicitly after they are configured in parser options:

```csharp
var parseOptions = RsqlParseOptions.Default with
{
    CustomOperators = [new RsqlCustomOperator(RsqlLinqOperators.Contains)]
};

var filtered = products.ApplyRsql("name=contains=ik", options =>
{
    options.Allow("name", x => x.Name);
    options.AllowStringContainsOperator();
}, parseOptions);
```

Collection fields can use explicit custom operators:

```text
tags=any=(bike,outdoor)
tags=all=(bike,outdoor)
```

String equality supports common `*` wildcards by default:

```text
name==B*    -> StartsWith("B")
name==*met  -> EndsWith("met")
name==*ik*  -> Contains("ik")
name==Bo*d  -> StartsWith("Bo") && EndsWith("d")
```

See [docs/linq-adapter.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/linq-adapter.md) for wildcard behavior, value conversion, and adapter limitations.
See [docs/aspnet-core-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/aspnet-core-usage.md) for ASP.NET Core request handling examples.

## Pagination Models

`RsqlParserNet.Linq` includes reusable pagination models that are used by the ASP.NET Core and Entity Framework Core adapters:

| Type | Purpose |
| --- | --- |
| `RsqlPageRequest` | Page number and page size request, with page-size clamping helpers. |
| `RsqlPagination` | Metadata for `page`, `pageSize`, `totalItems`, `totalPages`, `hasPreviousPage`, and `hasNextPage`. |
| `RsqlPagedResult<T>` | A framework-neutral response shape with `Items` and `Pagination`. |

The default JSON shape is:

```json
{
  "items": [],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 42,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

Use `ApplyPage(...)` for synchronous query composition and `ToRsqlPageAsync(...)` from `RsqlParserNet.EntityFrameworkCore` when EF Core should count, page, and materialize asynchronously.

## ASP.NET Core Binding

`RsqlParserNet.AspNetCore` adds bindable query wrappers for request query strings. Minimal APIs can accept `RsqlQueryRequest` directly to reuse parsed filter, sort, and page state:

```csharp
builder.Services.AddRsqlQueryRequest(
    configureFilter: options =>
    {
        options.ParseOptions = ProductRsqlProfile.Instance.ConfigureParseOptions(RsqlParseOptions.Default);
    },
    configurePage: options => options.MaxPageSize = 100);

app.MapGet("/products", async (
    RsqlQueryRequest request,
    AppDbContext db) =>
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

    var result = await query.ToRsqlPageAsync(request.PageRequest);

    return Results.Ok(result);
});
```

## Adapter Direction

Expression trees are a good fit for LINQ and Entity Framework Core integration, but they belong in separate adapter packages rather than the parser core.

The intended adapter shape is explicit and allowlisted:

```csharp
query.ApplyRsql(filter, options =>
{
    options.Allow("title", x => x.Title);
    options.Allow("status", x => x.Status);
    options.Allow("createdAt", x => x.CreatedAt);
});
```

The core parser should not expose arbitrary reflected entity/property access. Future adapters should translate the AST into expression trees only through configured field mappings.

Framework adapters should stay separate when they need framework-specific dependencies. `RsqlParserNet.AspNetCore` owns ASP.NET Core binding helpers.

`RsqlParserNet.EntityFrameworkCore` owns EF Core async execution helpers, including paged result helpers that return `items` and `pagination` metadata. Sorting uses the same allowlisted profile mappings as filtering with `sort=field`, `sort=-field`, and multi-field sort text such as `sort=-createdAt,name`.

`RsqlParserNet.FastEndpoints` owns FastEndpoints-specific validation glue. It reuses the ASP.NET Core query models and adds `ValidationFailure` entries to the endpoint, so FastEndpoints APIs can keep their normal validation response flow.

`RsqlParserNet.OpenApi` owns endpoint-scoped OpenAPI query parameter documentation helpers for `filter`, `sort`, `page`, and `pageSize`.

`RsqlParserNet.Swashbuckle` owns Swashbuckle operation filters for documenting the same query parameters through SwaggerGen.

`RsqlParserNet.NSwag` owns NSwag operation processors for documenting the same query parameters through NSwag and FastEndpoints.Swagger-style OpenAPI generation.

## Development

```bash
dotnet build RsqlParserNet.sln
dotnet test RsqlParserNet.sln
dotnet test RsqlParserNet.sln --collect:"XPlat Code Coverage"
dotnet pack src/RsqlParserNet/RsqlParserNet.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.Linq/RsqlParserNet.Linq.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.AspNetCore/RsqlParserNet.AspNetCore.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.EntityFrameworkCore/RsqlParserNet.EntityFrameworkCore.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.FastEndpoints/RsqlParserNet.FastEndpoints.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.OpenApi/RsqlParserNet.OpenApi.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.Swashbuckle/RsqlParserNet.Swashbuckle.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.NSwag/RsqlParserNet.NSwag.csproj --configuration Release --output artifacts/packages
```

Coverage is collected locally with Coverlet. A public percentage badge will be added after coverage publishing is wired into CI.

## Release Automation

Pushing a `v*` tag builds, tests, packs, and creates a GitHub Release with every package artifact. Tags also publish all generated `.nupkg` and `.snupkg` files to NuGet.org when the repository secret `NUGET_API_KEY` is configured.

The manual `Publish NuGet` workflow remains available for retrying one package from a release tag.

## Project Notes

- Versioning: [CHANGELOG.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/CHANGELOG.md)
- Syntax details: [docs/syntax.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/syntax.md)
- LINQ adapter details: [docs/linq-adapter.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/linq-adapter.md)
- ASP.NET Core usage: [docs/aspnet-core-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/aspnet-core-usage.md)
- EF Core helpers: [docs/entity-framework-core.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/entity-framework-core.md)
- FastEndpoints usage: [docs/fastendpoints-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/fastendpoints-usage.md)
- OpenAPI usage: [docs/openapi-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/openapi-usage.md)
- Swashbuckle usage: [docs/swashbuckle-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/swashbuckle-usage.md)
- NSwag usage: [docs/nswag-usage.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/nswag-usage.md)
- 1.0.0 readiness: [docs/v1-readiness.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/v1-readiness.md)
- Release process: [docs/release.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/release.md)
