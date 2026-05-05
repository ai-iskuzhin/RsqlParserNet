# LINQ Adapter Roadmap

This checklist tracks the path from useful preview to a stable LINQ adapter.

## Done

- Explicit selector allowlisting with `options.Allow("field", x => x.Member)`.
- Reusable `RsqlLinqProfile<T>` profiles.
- Profile-level parser option configuration.
- Predicate generation through `RsqlPredicateBuilder`.
- Queryable filtering through `ApplyRsql`.
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
- String contains helper for `=contains=`.
- Collection helpers:
  - `=any=`
  - `=all=`
- Conversion for strings, booleans, numeric primitives, enums, nullable values, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`.
- EF Core SQLite translation tests for built-in scalar operators, wildcards, string custom operators, and collection operators.
- CI, release, and manual NuGet publish workflow support for the LINQ adapter package.

## Before Stable

- Decide whether `=any=` and `=all=` should remain helper conventions or move into a documented companion syntax profile.
- Add samples for ASP.NET Core request handling without adding ASP.NET dependencies to the LINQ package.
- Review public API names before publishing `RsqlParserNet.Linq`.

## Later

- Optional attribute-based discovery for DTOs, if profiles still feel too repetitive.
- More custom operator helpers, such as starts-with, ends-with, and case-insensitive string matching.
- Provider-specific packages if EF Core or other LINQ providers need specialized translations.
