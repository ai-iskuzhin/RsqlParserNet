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

## Profiles

Use `RsqlLinqProfile<T>` when the same allowlisted field set is reused by multiple endpoints or services:

```csharp
public sealed class ProductRsqlProfile : RsqlLinqProfile<Product>
{
    public override RsqlParseOptions ConfigureParseOptions(RsqlParseOptions options)
    {
        return options.CustomOperators.Any(x => x.Text == "=contains=")
            ? options
            : options with
            {
                CustomOperators = [.. options.CustomOperators, new RsqlCustomOperator("=contains=")]
            };
    }

    public override void Configure(RsqlLinqOptions<Product> options)
    {
        options.Allow("name", x => x.Name);
        options.Allow("status", x => x.Status);
        options.Allow("count", x => x.Count);
        options.AllowStringContainsOperator();
    }
}
```

Profiles can be passed directly to `ApplyRsql` or `RsqlPredicateBuilder`:

```csharp
var profile = new ProductRsqlProfile();

var filtered = products.ApplyRsql("status==active;name=contains=ik", profile);

var predicate = RsqlPredicateBuilder.BuildPredicate("name==B*", profile);
```

Profiles are still explicit allowlists. They are the recommended reuse mechanism before adding attribute-based discovery. If a profile uses custom operators, override `ConfigureParseOptions` so callers do not need to pass matching parser options separately.

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

Custom operators are parsed by the core package, and the LINQ adapter translates them only when each operator is mapped intentionally.

## Custom Operators

Custom operators require two configuration steps:

1. Configure the core parser so the operator text is recognized.
2. Configure the LINQ adapter so the parsed operator can become an expression.

```csharp
var parseOptions = RsqlParseOptions.Default with
{
    CustomOperators = [new RsqlCustomOperator("=contains=")]
};

var filtered = products.ApplyRsql(
    "name=contains=ik",
    options =>
    {
        options.Allow("name", x => x.Name);
        options.AllowStringContainsOperator();
    },
    parseOptions);
```

For custom expression logic, use `CustomOperator`:

```csharp
var parseOptions = RsqlParseOptions.Default with
{
    CustomOperators = [new RsqlCustomOperator("=starts=")]
};

var predicate = RsqlPredicateBuilder.BuildPredicate<Product>(
    "name=starts=Bo",
    options =>
    {
        options.Allow("name", x => x.Name);
        options.CustomOperator("=starts=", context =>
            context.CallStringMethod(nameof(string.StartsWith)));
    },
    parseOptions);
```

Custom operator factories receive the allowlisted member expression, the comparison AST node, and values converted to the mapped member type. The returned expression must be Boolean.

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
