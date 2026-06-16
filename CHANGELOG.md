# Changelog

All notable changes to `RsqlParserNet` will be documented in this file.

The project uses Semantic Versioning. Starting with `1.0.0`, public API changes follow normal SemVer compatibility rules.

## 1.0.3

### Added

- Added LINQ adapter support for converting filter values to custom mapped types that expose a `TypeConverter` (e.g. strongly-typed IDs and value objects). Based on an idea from [#3](https://github.com/ai-iskuzhin/RsqlParserNet/pull/3) by [@TitovPavel](https://github.com/TitovPavel).

## 1.0.2

### Added

- Added LINQ adapter value normalization options, including default UTC normalization for `DateTimeOffset` constants.

### Fixed

- Normalized LINQ adapter `DateTimeOffset` constants to UTC by default so EF Core/Npgsql can compare offset date-time filter values against PostgreSQL `timestamp with time zone` columns.

## 1.0.1

### Added

- Documented the reusable pagination models and default paged response shape.
- Documented the ready-to-use API query models for filter, sort, pagination, validation errors, EF Core paging, and OpenAPI documentation.

## 1.0.0

First stable release.

### Added

- Added NuGet README guidance for choosing packages, reading the core AST model, projecting diagnostics to API errors, and manually applying a safe allowlisted AST subset without the LINQ adapter.

### Changed

- Stabilized the aligned package family for the first `1.0.0` release.

## 0.3.0-preview.1

### Added

- Added the `RsqlParserNet.NSwag` adapter project with endpoint-scoped and global operation processors for documenting RSQL query parameters.

### Changed

- Documented the FastEndpoints 7 compatibility requirement and updated FastEndpoints examples to use the current `Send.OkAsync` response helper.
- Removed completed roadmap/checklist docs that were duplicated by the usage docs and `docs/v1-readiness.md`.

## 0.2.0-preview.1

### Added

- Added the `RsqlParserNet.Linq` adapter project for allowlisted expression tree generation.
- Added LINQ adapter overloads for parsed `RsqlQuery` instances and raw RSQL expression text.
- Added `RsqlPredicateBuilder` for building reusable `Expression<Func<T, bool>>` predicates without applying `Where`.
- Added `RsqlLinqProfile<T>` for reusable allowlisted LINQ adapter configuration.
- Added profile-level parser option configuration for reusable custom operator contracts.
- Added LINQ adapter support for equality, inequality, range comparisons, `=in=`, `=out=`, AND, and OR.
- Added explicit custom operator expression factories for the LINQ adapter, including string contains, starts-with, and ends-with helpers.
- Added collection custom operator helpers for `=any=` and `=all=` in the LINQ adapter.
- Added LINQ adapter operator constants and a parser option helper for conventional custom operators.
- Added EF Core SQLite translation tests for LINQ adapter expressions.
- Added configurable string wildcard support for LINQ equality and inequality comparisons.
- Added value conversion coverage for strings, booleans, numbers, enums, nullable values, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`.
- Added LINQ adapter tests for selector allowlisting, logical expressions, built-in operators, null handling, and conversion failures.
- Added LINQ adapter documentation for supported operators, wildcards, and value conversion.
- Added ASP.NET Core request handling examples without adding ASP.NET dependencies to the packages.
- Added the `RsqlParserNet.AspNetCore` adapter project with a bindable `RsqlQueryFilter` request model.
- Documented the FastEndpoints reuse path through `RsqlQueryFilter.Parse`.
- Added framework-neutral pagination request/result models and queryable page helpers.
- Added framework-neutral sort request parsing and allowlisted queryable sort helpers.
- Added ASP.NET Core `RsqlPageQuery` binding for `page` and `pageSize` query parameters.
- Added ASP.NET Core `RsqlSortQuery` binding for `sort=field` and `sort=-field`.
- Added ASP.NET Core `RsqlQueryRequest` binding for combined filter, sort, and page query state.
- Added `RsqlQueryRequest.PageRequest` for accessing a validated non-null page request in endpoint code.
- Added structured `RsqlQueryError` values and validation problem details mapping for query binding errors.
- Added `RsqlQueryRequest.TryApplyTo` for mapping LINQ/profile translation failures into query errors.
- Added the `RsqlParserNet.EntityFrameworkCore` adapter project with async paged result helpers.
- Added the `RsqlParserNet.FastEndpoints` adapter project with query binding and validation failure helpers.
- Added the `RsqlParserNet.OpenApi` adapter project with endpoint-scoped query parameter documentation helpers.
- Added the `RsqlParserNet.Swashbuckle` adapter project with endpoint-scoped and global operation filters.
- Added sort request whitespace normalization and selector-style field validation.
- Added multi-field sorting with `sort=-field,otherField` across LINQ, ASP.NET Core binding, and EF Core helpers.
- Added configurable string comparison mode for case-insensitive string helper and wildcard matching.
- Added PostgreSQL SQL-generation tests for common LINQ adapter expressions.

### Changed

- Changed ASP.NET Core filter validation errors to use the query parameter name as the validation key while keeping diagnostic codes in messages.
- Changed packable projects to treat missing public XML documentation as a build error.

## 0.1.0-preview.3

Release workflow preview refresh.

### Fixed

- Included the manual NuGet publish workflow in the release tag.

## 0.1.0-preview.2

Metadata-only preview refresh.

### Fixed

- Corrected NuGet project and repository links to `https://github.com/ai-iskuzhin/RsqlParserNet`.

## 0.1.0-preview.1

Initial parser-focused preview.

### Added

- Dependency-light core parser package targeting `net10.0`.
- Tokenizer for RSQL/FIQL-style comparison and logical syntax.
- Typed AST nodes for comparisons and logical groups.
- Comparison helper properties for custom operators and multi-value comparisons.
- Comparison operators:
  - `==`
  - `!=`
  - `>`
  - `>=`
  - `<`
  - `<=`
  - `=gt=`
  - `=ge=`
  - `=lt=`
  - `=le=`
  - `=in=`
  - `=out=`
- Logical operators:
  - `;` for AND
  - `,` for OR
  - optional word operators `and` and `or`
- Configured custom FIQL-style comparison operators.
- Parenthesized value list enforcement for `=in=`, `=out=`, and multi-value custom operators.
- Validation for custom operator configuration.
- Parentheses for grouping.
- Multi-value comparison arguments.
- Quoted and unquoted value parsing.
- Primitive value classification for strings, numbers, booleans, and nulls.
- Raw value text preservation via `RsqlValue.RawText`.
- Parser-level preservation of wildcard and date-like values as string text.
- AST traversal helpers via `DescendantsAndSelf()` and `Comparisons()`.
- Conformance tests for common Java RSQL parser README examples.
- README grammar and core responsibility documentation.
- Split detailed syntax documentation into `docs/syntax.md` and simplified the package README.
- Release process and core v1 checklist documentation.
- Tag-based GitHub Release workflow for package artifacts.
- Manual NuGet publish workflow using `NUGET_API_KEY`.
- Structured diagnostics with stable diagnostic code constants.
- Source spans and line/column locations for diagnostics.
- Parser options with `RsqlParseOptions.AllowWordLogicalOperators`.
- Selector validation with `RsqlParseOptions.AllowDottedSelectors`.
- Unit tests for valid syntax, invalid syntax, diagnostics, spans, grouping, and options.
