# RsqlParserNet

[![RsqlParserNet NuGet](https://img.shields.io/nuget/v/RsqlParserNet?logo=nuget&style=flat-square)](https://www.nuget.org/packages/RsqlParserNet)
[![RsqlParserNet Downloads](https://img.shields.io/nuget/dt/RsqlParserNet?style=flat-square)](https://www.nuget.org/packages/RsqlParserNet)

A dependency-light .NET parser for RSQL/FIQL-style REST API query expressions. It parses query text into a typed AST with source spans and structured diagnostics, and does not depend on ASP.NET Core, LINQ, or Entity Framework Core.

## Part of the RsqlParserNet family

`RsqlParserNet` is the core/foundation package of the [RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet) family. It owns tokenizing, parsing, the AST model, and diagnostics. It parses selectors as text and never reflects them onto entity properties; allowlisting is the responsibility of the application and adapters.

Adapters build on this package:

- [`RsqlParserNet.Linq`](https://www.nuget.org/packages/RsqlParserNet.Linq) — allowlisted expression tree predicates for `IQueryable<T>` / `IEnumerable<T>`, plus pagination models.
- [`RsqlParserNet.AspNetCore`](https://www.nuget.org/packages/RsqlParserNet.AspNetCore) — bindable query-string models for `filter`, `sort`, `page`, and `pageSize`.
- [`RsqlParserNet.EntityFrameworkCore`](https://www.nuget.org/packages/RsqlParserNet.EntityFrameworkCore) — EF Core async paging helpers over composed queries.
- [`RsqlParserNet.FastEndpoints`](https://www.nuget.org/packages/RsqlParserNet.FastEndpoints) — FastEndpoints binding and validation failure glue.
- [`RsqlParserNet.OpenApi`](https://www.nuget.org/packages/RsqlParserNet.OpenApi) — ASP.NET Core built-in OpenAPI query parameter documentation.
- [`RsqlParserNet.Swashbuckle`](https://www.nuget.org/packages/RsqlParserNet.Swashbuckle) — Swashbuckle operation filters.
- [`RsqlParserNet.NSwag`](https://www.nuget.org/packages/RsqlParserNet.NSwag) — NSwag operation processors.

## Installation

```bash
dotnet add package RsqlParserNet
```

## Quick start

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
else
{
    var query = result.Query!;
    // use query.Root
}
```

Parser behavior can be configured with `RsqlParseOptions`, including word logical operators, dotted selectors, and FIQL-style custom operators:

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

## Supported syntax

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

Comparison operators: `==`, `!=`, `>` / `=gt=`, `>=` / `=ge=`, `<` / `=lt=`, `<=` / `=le=`, `=in=`, `=out=`, plus any configured custom operator.

Logical operators:

```text
; = AND
, = OR
```

AND has higher precedence than OR. Parentheses can override precedence. Wildcards (`*`) and date/date-time literals are preserved as string text; the core parser does not assign them semantics.

See [docs/syntax.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/syntax.md) for the full grammar, selector rules, value behavior, custom operators, and wildcard/date semantics.

## Diagnostics

Parse errors are returned as structured diagnostics with source locations:

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

`RsqlParser.TryParse` never throws for parse errors: it returns an `RsqlParseResult` whose `Success` is `false` and whose `Diagnostics` describe the problem.

`RsqlParser.Parse` throws `ArgumentException` for empty input and `RsqlParseException` for invalid syntax. `RsqlParseException.Diagnostics` contains the same structured diagnostics returned by `TryParse`.

## Core AST model

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

## Documentation

- Full syntax and grammar: [docs/syntax.md](https://github.com/ai-iskuzhin/RsqlParserNet/blob/main/docs/syntax.md)
- Project repository: [github.com/ai-iskuzhin/RsqlParserNet](https://github.com/ai-iskuzhin/RsqlParserNet)
