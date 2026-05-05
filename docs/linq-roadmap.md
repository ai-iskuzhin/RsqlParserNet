# LINQ Adapter Roadmap

This checklist tracks the path from useful preview to a stable LINQ adapter.

## Done

- Explicit selector allowlisting with `options.Allow("field", x => x.Member)`.
- Reusable `RsqlLinqProfile<T>` profiles.
- Profile-level parser option configuration.
- Predicate generation through `RsqlPredicateBuilder`.
- Queryable filtering through `ApplyRsql`.
- Queryable pagination through `ApplyPage`.
- Queryable sorting through `ApplySort`.
- Framework-neutral paged result models:
  - `RsqlPageRequest`
  - `RsqlPagedResult<T>`
  - `RsqlPagination`
- Framework-neutral sort model:
  - `RsqlSortRequest`
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
- Configurable string comparison mode for endpoint-wide case-insensitive search.
- Conventional LINQ operator constants and `WithLinqOperators()` parser option helper.
- Conversion for strings, booleans, numeric primitives, enums, nullable values, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`.
- EF Core SQLite translation tests for built-in scalar operators, wildcards, string custom operators, and collection operators.
- PostgreSQL SQL-generation tests for common LINQ adapter expressions without requiring a live database.
- CI, release, and manual NuGet publish workflow support for the LINQ adapter package.
- ASP.NET Core request handling samples without adding ASP.NET dependencies to the LINQ package.
- EF Core async paged result helpers in a separate adapter package.
- ASP.NET Core sort query binding with `sort=field` and `sort=-field`.
- Multi-field sorting with comma-separated sort text such as `sort=-createdAt,name`.

## Before Stable

- Publish `RsqlParserNet.Linq`, `RsqlParserNet.AspNetCore`, and `RsqlParserNet.EntityFrameworkCore` as preview packages.
- Gather API feedback on profiles, custom operators, sort parsing, pagination response shape, and framework integration before API freeze.
- Clarify preview package version alignment across core and adapter packages.

## Later

- Optional attribute-based discovery for DTOs, if profiles still feel too repetitive.
- Provider-specific packages for other LINQ providers if they need specialized translations.
- Real database integration coverage for production providers after the adapter API shape settles.
- FastEndpoints-specific binding and validation helpers, if framework-neutral parsing becomes repetitive.
- OpenAPI/query documentation helpers after the query surface stabilizes.
