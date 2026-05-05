# RsqlParserNet

A dependency-light .NET parser for RSQL/FIQL-style REST API query expressions.

`RsqlParserNet` parses query text into a typed AST with source spans and structured diagnostics. The core package does not depend on ASP.NET Core, LINQ, Entity Framework Core, or ORM APIs.

Current status: `0.1.0-preview.1`. The parser core is usable for early testing, but public API changes are still possible before `1.0.0`.

## Installation

The package is not published yet. For local development, reference the project directly:

```xml
<ProjectReference Include="src/RsqlParserNet/RsqlParserNet.csproj" />
```

After the first package release:

```bash
dotnet add package RsqlParserNet
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
```

## Project Notes

- Versioning: [CHANGELOG.md](CHANGELOG.md)
- Syntax details: [docs/syntax.md](docs/syntax.md)
- Core v1 checklist: [docs/core-v1-checklist.md](docs/core-v1-checklist.md)
- Release process: [docs/release.md](docs/release.md)
