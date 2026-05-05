# Entity Framework Core

`RsqlParserNet.EntityFrameworkCore` adds EF Core async execution helpers on top of the LINQ adapter.

The package does not replace explicit allowlisted profiles. It only removes repeated endpoint code for counting, paging, and materializing EF Core queries.

## Paged Results

```csharp
using RsqlParserNet.EntityFrameworkCore;
using RsqlParserNet.Linq;

var page = new RsqlPageRequest(page: 1, pageSize: 50);

var result = await db.Products
    .OrderBy(product => product.Id)
    .ToRsqlPageAsync(page, cancellationToken);
```

The response model is:

```json
{
  "items": [],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalItems": 123,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

## Filtering And Paging

Use a reusable LINQ profile to keep field access explicit:

```csharp
var parsed = RsqlParser.Parse("status==active");
var page = new RsqlPageRequest(page: 1, pageSize: 50);

var result = await db.Products
    .OrderBy(product => product.Id)
    .ToRsqlPageAsync(parsed, ProductRsqlProfile.Instance, page, cancellationToken);
```

For exception-based flows, the helper can parse expression text directly:

```csharp
var result = await db.Products
    .OrderBy(product => product.Id)
    .ToRsqlPageAsync(
        "status==active",
        ProductRsqlProfile.Instance,
        new RsqlPageRequest(page: 1, pageSize: 50),
        cancellationToken: cancellationToken);
```

For API endpoints, prefer parsing with `RsqlParser.TryParse` or `RsqlQueryFilter` first so syntax diagnostics can be returned as validation errors.

## Sorting

Sorting uses the same explicit profile mappings as filtering:

```csharp
var sort = RsqlSortRequest.Parse("-createdAt");

var result = await db.Products
    .ToRsqlPageAsync(
        parsed,
        sort,
        ProductRsqlProfile.Instance,
        new RsqlPageRequest(page: 1, pageSize: 50),
        cancellationToken);
```

The supported sort syntax is:

```text
sort=name
sort=-createdAt
```

Sort fields are case-sensitive and must be allowlisted in the profile.
