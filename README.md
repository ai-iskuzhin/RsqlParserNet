# RsqlParserNet

A dependency-light .NET parser for RSQL/FIQL-style REST API query expressions.

`RsqlParserNet` parses query text into a typed AST with source spans and structured diagnostics. The core package does not depend on ASP.NET Core, LINQ, Entity Framework Core, or ORM APIs.

Current status: `0.1.0-preview.3`. The parser core is published for early testing, but public API changes are still possible before `1.0.0`.

## Installation

```bash
dotnet add package RsqlParserNet --prerelease
```

For local development, reference the project directly:

```xml
<ProjectReference Include="src/RsqlParserNet/RsqlParserNet.csproj" />
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

See [docs/syntax.md](docs/syntax.md) for the full grammar, selector rules, value behavior, custom operators, and wildcard/date semantics.

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

var filtered = products.ApplyRsql("status==active;name=contains=ik", new ProductRsqlProfile());
```

The adapter currently supports:

| Operator | LINQ behavior |
| --- | --- |
| `==` / `!=` | Equality and inequality |
| `>` / `>=` / `<` / `<=` | Comparable mapped types such as numbers and dates |
| `=in=` / `=out=` | Membership checks over the supplied value list |
| `=any=` / `=all=` | Custom collection helpers for allowlisted collection fields |
| `;` / `,` | `AndAlso` and `OrElse` expression composition |

Values are converted using the mapped member type, including common scalar types, enums, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`.

Custom operators can be translated explicitly after they are configured in parser options:

```csharp
var parseOptions = RsqlParseOptions.Default with
{
    CustomOperators = [new RsqlCustomOperator("=contains=")]
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

See [docs/linq-adapter.md](docs/linq-adapter.md) for wildcard behavior, value conversion, and adapter limitations.

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

## Development

```bash
dotnet build RsqlParserNet.sln
dotnet test RsqlParserNet.sln
dotnet pack src/RsqlParserNet/RsqlParserNet.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.Linq/RsqlParserNet.Linq.csproj --configuration Release --output artifacts/packages
```

## Project Notes

- Versioning: [CHANGELOG.md](CHANGELOG.md)
- Syntax details: [docs/syntax.md](docs/syntax.md)
- LINQ adapter details: [docs/linq-adapter.md](docs/linq-adapter.md)
- LINQ adapter roadmap: [docs/linq-roadmap.md](docs/linq-roadmap.md)
- Core v1 checklist: [docs/core-v1-checklist.md](docs/core-v1-checklist.md)
- Release process: [docs/release.md](docs/release.md)
