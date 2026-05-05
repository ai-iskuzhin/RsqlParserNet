# 1.0.0 Readiness

This document tracks what must be true before the first stable `1.0.0` release.

## Core Parser

The core parser is close to stable. Before `1.0.0`, verify:

- Public AST types are intentionally named and documented.
- Diagnostic codes are final enough to support API clients.
- Syntax docs match the parser behavior exactly.
- No adapter-specific behavior has leaked into the core package.

## Adapter Packages

The adapter surface is still preview. Before `1.0.0`, verify:

- `RsqlParserNet.Linq` profile, allowlist, sorting, paging, string comparison, custom operator, and collection operator APIs feel stable after real API usage.
- `RsqlParserNet.AspNetCore` query binding and validation error shapes are stable.
- `RsqlParserNet.EntityFrameworkCore` async paging helpers are stable and do not hide too much query composition from applications.
- `RsqlParserNet.FastEndpoints` validation helpers fit normal FastEndpoints endpoint flow without forcing a specific response shape.

## Release Hygiene

Before stable:

- Publish all intended adapter packages as previews.
- Align package versions for the stable release.
- Freeze package IDs and repository metadata.
- Confirm CI builds, tests, formats, and packs every package.
- Review README, package README content, and docs for stale preview language.
- Gather at least one more integration feedback pass from a real API.

## Can Wait

These are useful but should not block `1.0.0` unless real users ask for them:

- Attribute-based field discovery.
- Provider-specific EF Core translation packages.
- Live database integration tests in this repository.
- OpenAPI query documentation helpers.
- Additional endpoint framework adapters.
