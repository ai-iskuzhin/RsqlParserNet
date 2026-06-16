# RsqlParserNet.Linq

[![RsqlParserNet.Linq NuGet](https://img.shields.io/nuget/v/RsqlParserNet.Linq?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Linq)
[![RsqlParserNet.Linq Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.Linq?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.Linq)

The LINQ adapter for `RsqlParserNet`. It translates a parsed RSQL/FIQL AST into allowlisted expression-tree predicates for `IQueryable<T>` and `IEnumerable<T>`, and ships reusable pagination and sorting models.

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. This package builds on the core [`RsqlParserNet`](https://www.nuget.org/packages/RsqlParserNet) parser, which turns query text into a typed AST, and is itself used by the ASP.NET Core (`RsqlParserNet.AspNetCore`) and Entity Framework Core (`RsqlParserNet.EntityFrameworkCore`) adapters.

## Installation

```bash
dotnet add package RsqlParserNet.Linq
```

This pulls in the core `RsqlParserNet` package as a dependency.

## Quick start

Selectors are never reflected onto entity properties automatically. RSQL selectors are only usable after they are explicitly mapped to a .NET expression.

Apply an RSQL expression directly to a queryable with inline mappings:

```csharp
using RsqlParserNet;
using RsqlParserNet.Linq;

var filtered = products.ApplyRsql("status==active;name==B*", options =>
{
    options.Allow("status", x => x.Status);
    options.Allow("name", x => x.Name);
});
```

Build the predicate separately when another layer should own query composition:

```csharp
using System.Linq.Expressions;

Expression<Func<Product, bool>> predicate =
    RsqlPredicateBuilder.BuildPredicate<Product>("status==active", options =>
    {
        options.Allow("status", x => x.Status);
    });

var filtered = products.Where(predicate);
```

For repeated endpoint/query contracts, put the mappings in a reusable `RsqlLinqProfile<T>`. Override `ConfigureParseOptions` so callers do not have to pass matching parser options separately:

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

var profile = new ProductRsqlProfile();

var filtered = products.ApplyRsql("status==active;name=contains=ik", profile);
var predicate = RsqlPredicateBuilder.BuildPredicate("name==B*", profile);
```

## Supported operators

| RSQL | Expression behavior |
| --- | --- |
| `==` | Equality |
| `!=` | Inequality |
| `>` | Greater than |
| `>=` | Greater than or equal |
| `<` | Less than |
| `<=` | Less than or equal |
| `=in=` | `Enumerable.Contains(values, member)` |
| `=out=` | Negated `Enumerable.Contains(values, member)` |
| `=contains=` | Configurable string contains helper |
| `=starts=` | Configurable string starts-with helper |
| `=ends=` | Configurable string ends-with helper |
| `=any=` | Configurable collection match: any mapped collection item is in the supplied values |
| `=all=` | Configurable collection match: every supplied value is present in the mapped collection |
| `;` | `Expression.AndAlso` |
| `,` | `Expression.OrElse` |

`=contains=`, `=starts=`, `=ends=`, `=any=`, and `=all=` are custom operators. They must be configured in both the parser (`RsqlParseOptions.CustomOperators`, or `options.WithLinqOperators()`) and the LINQ adapter (`AllowStringContainsOperator()`, `AllowCollectionAnyOperator()`, and so on) before they can be translated. The operator text constants are available on `RsqlLinqOperators`.

## Value conversion

Literal values are converted to the mapped CLR member type before expression constants are built. Supported conversions include:

- strings
- booleans
- numeric primitives
- enums (case-insensitive)
- nullable mapped types
- `Guid`
- `DateTime`
- `DateTimeOffset`
- `DateOnly`
- `TimeOnly`
- custom mapped types that expose a `TypeConverter` capable of converting from `string`, such as strongly-typed IDs and value objects

For a member mapped to a custom type, the adapter resolves the type's `TypeConverter` via `TypeDescriptor.GetConverter(...)` and, when it can convert from `string`, uses it to materialize the constant:

```csharp
[TypeConverter(typeof(ProductCodeConverter))]
public readonly record struct ProductCode(string Value);

var predicate = RsqlPredicateBuilder.BuildPredicate<CatalogEntry>("code==SKU100", options =>
{
    options.Allow("code", x => x.Code);
});
// "SKU100" is converted to ProductCode("SKU100") via its TypeConverter.
```

Invalid conversions throw `RsqlLinqException`.

`DateTimeOffset` literals are normalized to UTC by default before constants are built, which keeps EF Core/Npgsql queries compatible with PostgreSQL `timestamp with time zone` parameters:

```csharp
var filtered = acts.ApplyRsql("date>=2026-05-15T10:30:00+05:00", options =>
{
    options.Allow("date", x => x.Date);
});
// constant becomes 2026-05-15T05:30:00+00:00
```

Conversion behavior is configurable through `RsqlLinqOptions<T>`:

- `NormalizeDateTimeOffsetsToUtc` — set to `false` to preserve the parsed offset instead of converting to UTC.
- `NormalizeValue` — a `(value, targetType) => value` hook for application-specific literal handling, applied after the built-in conversion.
- `StringComparisonMode` — set to `RsqlStringComparisonMode.CaseInsensitive` to make string helper operators and wildcard equality case-insensitive for the whole profile/endpoint. The default `ProviderDefault` follows the underlying LINQ provider (for example, EF Core database collation).

```csharp
var filtered = acts.ApplyRsql("date>=2026-05-15T10:30:00+05:00", options =>
{
    options.NormalizeDateTimeOffsetsToUtc = false;
    options.NormalizeValue = (value, targetType) => value;
    options.StringComparisonMode = RsqlStringComparisonMode.CaseInsensitive;
    options.Allow("date", x => x.Date);
});
```

## Wildcards

By default, `*` is treated as a wildcard only for string `==` and `!=` comparisons. A null guard is added before any string method is called.

| Pattern | Expression behavior |
| --- | --- |
| `name==B*` | `x.Name.StartsWith("B")` |
| `name==*met` | `x.Name.EndsWith("met")` |
| `name==*ik*` | `x.Name.Contains("ik")` |
| `name==Bo*d` | `x.Name.StartsWith("Bo") && x.Name.EndsWith("d")` |
| `name!=B*` | Negates the wildcard expression |

Complex multi-segment patterns such as `*a*b*` are rejected so the adapter stays provider-friendly and does not pretend to offer full SQL `LIKE` semantics. To match a literal asterisk instead, disable wildcards per query:

```csharp
var filtered = products.ApplyRsql("name==B*", options =>
{
    options.StringWildcardMode = RsqlStringWildcardMode.Disabled;
    options.Allow("name", x => x.Name);
});
```

## Pagination & sorting

The package includes framework-neutral pagination and sorting models. They do not depend on ASP.NET Core or EF Core, and they are shared by those adapters.

| Type | Purpose |
| --- | --- |
| `RsqlPageRequest` | One-based page number and page size, with `Skip`/`Take` and `ClampPageSize(maxPageSize)` helpers. |
| `RsqlPagination` | Metadata for `page`, `pageSize`, `totalItems`, `totalPages`, `hasPreviousPage`, and `hasNextPage`. |
| `RsqlPagedResult<T>` | A framework-neutral response shape with `Items` and `Pagination`. |

Use `ApplyPage(...)` for synchronous query composition:

```csharp
var page = new RsqlPageRequest(page: 2, pageSize: 25);

var items = products
    .OrderBy(product => product.Id)
    .ApplyPage(page)
    .ToArray();

var result = RsqlPagedResult<Product>.Create(items, page, totalItems: 100);
```

The default JSON shape is:

```json
{
  "items": [],
  "pagination": {
    "page": 2,
    "pageSize": 25,
    "totalItems": 100,
    "totalPages": 4,
    "hasPreviousPage": true,
    "hasNextPage": true
  }
}
```

Sorting is allowlisted through the same profile mappings as filtering. Use `RsqlSortRequest` for API-level sort text such as `sort=name`, `sort=-createdAt`, and `sort=-createdAt,name`. A leading `-` means descending; comma-separated fields are applied in priority order:

```csharp
var sort = RsqlSortRequest.Parse("-name");
var sorted = products.ApplySort(sort, profile);

var sorts = RsqlSortRequest.ParseMany("-updatedAt,title");
var sortedMany = products.ApplySort(sorts, profile);
```

Use `RsqlParserNet.EntityFrameworkCore` (`ToRsqlPageAsync(...)`) when EF Core should count, page, and materialize asynchronously.

## Documentation

- LINQ adapter guide: [docs/linq-adapter.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/linq-adapter.md)
- Main repository and full documentation: [RsqlParserNet on GitHub](https://github.com/ai-iskuzhin/RsqlParserNet)
