# Core v1 Checklist

The core package can be considered ready for `1.0.0` when these items are true.

## Parser Contract

- Supported grammar is documented in `README.md`.
- Common Java RSQL parser examples are covered by tests.
- Symbolic logical operators `;` and `,` are stable.
- Optional word logical operators `and` and `or` are configurable.
- Built-in comparison operators are stable.
- Custom FIQL-style operators are configurable.

## AST Contract

- `RsqlQuery` exposes the original expression and root node.
- `RsqlComparisonNode` exposes selector, normalized operator, raw operator text, values, and span.
- `RsqlLogicalNode` exposes operator, child nodes, and span.
- `RsqlValue` exposes kind, normalized text, raw text, and span.
- Traversal helpers are available for adapter authors.

## Diagnostics

- Diagnostic codes are stable constants.
- Diagnostics include message, span, start location, and end location.
- Empty input, invalid tokens, unexpected tokens, and invalid selectors are covered by tests.

## Adapter Boundary

- Core does not depend on ASP.NET Core, LINQ, EF Core, or ORM APIs.
- Core preserves wildcard and date-like values as text.
- Type coercion and wildcard interpretation are left to adapters.
- Adapters must use explicit allowlisted field mappings.

## Release

- README and changelog are current.
- CI passes.
- `dotnet pack` produces `.nupkg` and `.snupkg`.
- Release tag matches package version.
