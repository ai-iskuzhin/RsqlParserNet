# RsqlParserNet.EntityFrameworkCore

[![RsqlParserNet.EntityFrameworkCore NuGet](https://img.shields.io/nuget/v/RsqlParserNet.EntityFrameworkCore?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.EntityFrameworkCore)
[![RsqlParserNet.EntityFrameworkCore Downloads](https://img.shields.io/nuget/dt/RsqlParserNet.EntityFrameworkCore?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet.EntityFrameworkCore)

EF Core async paging helpers that count, page, and materialize an already-composed `IQueryable<T>` into a framework-neutral `RsqlPagedResult<T>` with `items` and `pagination` metadata.

Part of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. It builds on [`RsqlParserNet.Linq`](https://www.nuget.org/packages/RsqlParserNet.Linq) (allowlisted filtering, sorting, and the pagination models) and complements [`RsqlParserNet.AspNetCore`](https://www.nuget.org/packages/RsqlParserNet.AspNetCore), which owns request-string binding. This package only removes repeated endpoint code for asynchronous counting, paging, and materialization; it does not replace explicit allowlisted profiles.

## Installation

```bash
dotnet add package RsqlParserNet.EntityFrameworkCore
```

Requires `Microsoft.EntityFrameworkCore` `10.0.0` (targets `net10.0`).

## Quick start

Compose filtering and sorting through your allowlisted [`RsqlParserNet.Linq`](https://www.nuget.org/packages/RsqlParserNet.Linq) profile, then page and materialize asynchronously:

```csharp
using RsqlParserNet;
using RsqlParserNet.Linq;
using RsqlParserNet.EntityFrameworkCore;

var query = db.Products
    .ApplyRsql("status==active", ProductRsqlProfile.Instance)
    .ApplySort(RsqlSortRequest.Parse("-createdAt"), ProductRsqlProfile.Instance);

var result = await query.ToRsqlPageAsync(
    new RsqlPageRequest(page: 1, pageSize: 25),
    cancellationToken);
```

`result` is an `RsqlPagedResult<Product>`:

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

Overloads can also apply the query and sort for you in one call:

```csharp
var parsed = RsqlParser.Parse("status==active");

var result = await db.Products.ToRsqlPageAsync(
    parsed,
    RsqlSortRequest.Parse("-createdAt"),
    ProductRsqlProfile.Instance,
    new RsqlPageRequest(page: 1, pageSize: 25),
    cancellationToken);
```

`DateTimeOffset` literals are normalized to UTC by the LINQ adapter before constants are built, which keeps EF Core/Npgsql comparisons compatible with PostgreSQL `timestamp with time zone` parameters.

## Helpers

All helpers are extension methods on `IQueryable<T>` in `RsqlEntityFrameworkQueryableExtensions` and return `Task<RsqlPagedResult<T>>`.

| Overload | What it does |
| --- | --- |
| `ToRsqlPageAsync(page, ct)` | Counts, pages, and materializes the source queryable as-is. |
| `ToRsqlPageAsync(query, profile, page, ct)` | Applies a parsed `RsqlQuery` through the profile, then pages. |
| `ToRsqlPageAsync(sort, profile, page, ct)` | Applies a single `RsqlSortRequest` through the profile, then pages. |
| `ToRsqlPageAsync(sorts, profile, page, ct)` | Applies an ordered `IEnumerable<RsqlSortRequest>` through the profile, then pages. |
| `ToRsqlPageAsync(query, sort, profile, page, ct)` | Applies a parsed `RsqlQuery` and a single sort, then pages. |
| `ToRsqlPageAsync(query, sorts, profile, page, ct)` | Applies a parsed `RsqlQuery` and ordered sorts, then pages. |
| `ToRsqlPageAsync(expression, profile, page, parseOptions?, ct)` | Parses RSQL expression text, applies it, then pages. |
| `ToRsqlPageAsync(expression, sort, profile, page, parseOptions?, ct)` | Parses RSQL expression text, applies it and a single sort, then pages. |
| `ToRsqlPageAsync(expression, sorts, profile, page, parseOptions?, ct)` | Parses RSQL expression text, applies it and ordered sorts, then pages. |

The string-expression overloads throw on invalid syntax. For API endpoints, prefer parsing with `RsqlParser.TryParse` or `RsqlQueryFilter` first so syntax diagnostics can be returned as validation errors.

## Documentation

- EF Core helpers: [docs/entity-framework-core.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/entity-framework-core.md)
- Main repository: [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet)
