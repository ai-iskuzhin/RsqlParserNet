# RsqlParserNet Working Agreement

This file gives AI coding agents and maintainers the project-specific rules that matter most.

## Purpose

`RsqlParserNet` is a dependency-light .NET parser for RSQL/FIQL-style REST API query expressions.

The core package parses query text into a typed AST with source spans and structured diagnostics. It must stay independent from ASP.NET Core, LINQ, Entity Framework Core, FastEndpoints, ORMs, and application-specific code.

## Project Layout

Current projects:

```text
src/RsqlParserNet
tests/RsqlParserNet.Tests
```

Future adapter packages should be added as separate projects/packages:

```text
src/RsqlParserNet.Linq
src/RsqlParserNet.EntityFrameworkCore
src/RsqlParserNet.AspNetCore
src/RsqlParserNet.FastEndpoints
```

Use `net10.0`. Do not add legacy target frameworks unless there is an explicit product decision.

## Core Package Rules

- Keep the core parser dependency-light.
- Keep tokenizer/parser implementation details internal.
- Public API should be limited to parser entry points, options, AST nodes, values, diagnostics, source locations, custom operator configuration, and traversal helpers.
- Parser output must be typed AST, not string fragments.
- Preserve original source spans and diagnostic locations.
- Preserve wildcard and date-like values as text; adapters decide semantics and type coercion.
- Do not add expression tree, EF Core, ASP.NET Core, or reflection-based mapping behavior to the core package.

## Adapter Rules

Adapters must use explicit allowlisted field mappings.

Good shape:

```csharp
query.ApplyRsql(filter, options =>
{
    options.Allow("title", x => x.Title);
    options.Allow("status", x => x.Status);
    options.Allow("createdAt", x => x.CreatedAt);
});
```

Avoid public APIs that imply:

```text
client field name -> arbitrary reflected entity property path
```

Reflection may be used internally if needed, but public behavior must remain allowlisted and predictable.

## Syntax Compatibility

Keep syntax close to common RSQL/FIQL and the Java `jirutka/rsql-parser` behavior unless an intentional .NET-specific choice is documented.

Core supports:

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

Interpretation:

```text
; = AND
, = OR
```

See `docs/syntax.md` for the full grammar and documented deviations.

## Testing Expectations

Parser changes need focused tests for:

- selectors
- quoted strings
- escaped characters
- comparison operators
- custom operators
- logical AND/OR precedence
- parentheses
- multi-value operators
- invalid syntax diagnostics
- whitespace behavior
- source spans and line/column reporting
- Java RSQL README example conformance

Adapter packages later need integration-style tests against representative queryables, providers, and request-binding scenarios.

Always run:

```bash
dotnet test RsqlParserNet.sln
```

For package-facing changes, also run:

```bash
dotnet pack src/RsqlParserNet/RsqlParserNet.csproj --configuration Release --output artifacts/packages
```

## Documentation Expectations

Keep `README.md` short and package-user focused.

Put detailed material in docs:

- syntax and grammar: `docs/syntax.md`
- release process: `docs/release.md`
- core readiness: `docs/core-v1-checklist.md`

Update `CHANGELOG.md` for notable public behavior, API, packaging, or documentation changes.

## Release Discipline

Use Semantic Versioning.

Preview versions:

```text
0.1.0-preview.1
0.2.0-preview.1
```

Release tags use the package version prefixed with `v`:

```text
v0.1.0-preview.1
```

Do not call the core package `1.0.0` until at least one adapter package validates the AST and public API shape.
