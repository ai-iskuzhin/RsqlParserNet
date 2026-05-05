# LINQ Adapter

`RsqlParserNet.Linq` translates parsed RSQL AST nodes into expression tree predicates for `IQueryable<T>`.

The adapter is intentionally allowlisted. RSQL selectors are only usable after they are mapped to a .NET expression:

```csharp
var filtered = products.ApplyRsql("status==active;name==B*", options =>
{
    options.Allow("status", x => x.Status);
    options.Allow("name", x => x.Name);
});
```

Use `RsqlPredicateBuilder` when another layer should own query composition:

```csharp
using System.Linq.Expressions;

Expression<Func<Product, bool>> predicate =
    RsqlPredicateBuilder.BuildPredicate<Product>("status==active", options =>
    {
        options.Allow("status", x => x.Status);
    });

var filtered = products.Where(predicate);
```

## Supported Operators

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
| `;` | `Expression.AndAlso` |
| `,` | `Expression.OrElse` |

Custom operators are parsed by the core package, but the LINQ adapter rejects them until each custom operator can be mapped intentionally.

## Wildcards

By default, the LINQ adapter treats `*` as a wildcard only for string `==` and `!=` comparisons:

| Pattern | Expression behavior |
| --- | --- |
| `name==B*` | `x.Name.StartsWith("B")` |
| `name==*met` | `x.Name.EndsWith("met")` |
| `name==*ik*` | `x.Name.Contains("ik")` |
| `name==Bo*d` | `x.Name.StartsWith("Bo") && x.Name.EndsWith("d")` |
| `name!=B*` | Negates the wildcard expression |

The adapter adds a null guard before calling string methods. Complex multi-segment wildcard patterns such as `*a*b*` are rejected for now because this package should remain provider-friendly and should not pretend to offer full SQL `LIKE` semantics.

Literal asterisk matching can be enabled per query:

```csharp
var filtered = products.ApplyRsql("name==B*", options =>
{
    options.StringWildcardMode = RsqlStringWildcardMode.Disabled;
    options.Allow("name", x => x.Name);
});
```

String comparison behavior follows the underlying LINQ provider. For example, EF Core database collation can affect case sensitivity.

## Value Conversion

Values are converted using the mapped member type. Current conversion support includes:

- strings
- booleans
- numeric primitives
- enums
- nullable mapped types
- `Guid`
- `DateTime`
- `DateTimeOffset`
- `DateOnly`
- `TimeOnly`

Invalid conversions throw `RsqlLinqException`.
