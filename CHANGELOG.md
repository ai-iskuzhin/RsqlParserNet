# Changelog

All notable changes to `RsqlParserNet` will be documented in this file.

The project uses Semantic Versioning. Versions below `1.0.0` are preview releases and may include public API changes while the core parser contract is finalized.

## Unreleased

### Added

- Added the `RsqlParserNet.Linq` adapter project for allowlisted expression tree generation.
- Added LINQ adapter overloads for parsed `RsqlQuery` instances and raw RSQL expression text.
- Added `RsqlPredicateBuilder` for building reusable `Expression<Func<T, bool>>` predicates without applying `Where`.
- Added `RsqlLinqProfile<T>` for reusable allowlisted LINQ adapter configuration.
- Added profile-level parser option configuration for reusable custom operator contracts.
- Added LINQ adapter support for equality, inequality, range comparisons, `=in=`, `=out=`, AND, and OR.
- Added explicit custom operator expression factories for the LINQ adapter, including a string contains helper.
- Added collection custom operator helpers for `=any=` and `=all=` in the LINQ adapter.
- Added LINQ adapter operator constants and a parser option helper for conventional custom operators.
- Added EF Core SQLite translation tests for LINQ adapter expressions.
- Added configurable string wildcard support for LINQ equality and inequality comparisons.
- Added value conversion coverage for strings, booleans, numbers, enums, nullable values, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, and `TimeOnly`.
- Added LINQ adapter tests for selector allowlisting, logical expressions, built-in operators, null handling, and conversion failures.
- Added LINQ adapter documentation for supported operators, wildcards, and value conversion.
- Added ASP.NET Core request handling examples without adding ASP.NET dependencies to the packages.

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
