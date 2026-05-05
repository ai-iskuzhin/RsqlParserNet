# LINQ Adapter Roadmap

This checklist tracks the path from useful preview to a stable LINQ adapter.

## Done

- Explicit selector allowlisting with `options.Allow("field", x => x.Member)`.
- Reusable `RsqlLinqProfile<T>` profiles.
- Profile-level parser option configuration.
- Predicate generation through `RsqlPredicateBuilder`.
- Queryable filtering through `ApplyRsql`.
- Queryable pagination through `ApplyPage`.
- Framework-neutral paged result models:
  - `RsqlPageRequest`
  - `RsqlPagedResult<T>`
  - `RsqlPagination`
- Built-in scalar operators:
  - `==`
  - `!=`
  - `>`
  - `>=`
  - `<`
  - `<=`
  - `=in=`
  - `=out=`
- Logical operators:
  - `;`
  - `,`
- String wildcard equality for simple patterns.
- Explicit custom operator factories.
- String helpers:
  - `=contains=`
  - `=starts=`
  - `=ends=`
- Collection helpers:
  - `=any=`
  - `=all=`
- Conventional LINQ operator constants and `WithLinqOperators()` parser option helper.
- Conversion for strings, booleans, numeric primitives, enums, nullable values, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`.
- EF Core SQLite translation tests for built-in scalar operators, wildcards, string custom operators, and collection operators.
- CI, release, and manual NuGet publish workflow support for the LINQ adapter package.
- ASP.NET Core request handling samples without adding ASP.NET dependencies to the LINQ package.
- EF Core async paged result helpers in a separate adapter package.

## Before Stable

- Publish `RsqlParserNet.Linq` as `0.1.0-preview.1` and gather feedback before API freeze.

## Later

- Optional attribute-based discovery for DTOs, if profiles still feel too repetitive.
- More custom operator helpers, such as case-insensitive string matching.
- Provider-specific packages for other LINQ providers if they need specialized translations.
