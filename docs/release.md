# Release Process

This project uses Semantic Versioning for NuGet packages.

Preview versions should use a prerelease suffix:

```text
0.1.0-preview.1
0.1.0-preview.2
0.2.0-preview.1
```

Stable versions should use plain SemVer:

```text
1.0.0
1.1.0
2.0.0
```

## Before Release

1. Update package version metadata in each package project being released.
2. Update `CHANGELOG.md`.
3. Update `README.md` if the public API or supported syntax changed.
4. Run:

```bash
dotnet test RsqlParserNet.sln
dotnet pack src/RsqlParserNet/RsqlParserNet.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.Linq/RsqlParserNet.Linq.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.AspNetCore/RsqlParserNet.AspNetCore.csproj --configuration Release --output artifacts/packages
```

If releasing only one package, the other package-specific pack steps can be skipped.

5. Inspect the generated packages:

```bash
unzip -l artifacts/packages/RsqlParserNet.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.Linq.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.AspNetCore.<version>.nupkg
```

## Package Versions

Packages can be versioned independently while the project is in preview.

Current package identities:

```text
RsqlParserNet
RsqlParserNet.Linq
RsqlParserNet.AspNetCore
```

## Tagging

Use tags that match the package version prefixed with `v`:

```bash
git tag -a v0.1.0-preview.3 -m "RsqlParserNet 0.1.0-preview.3"
git push origin v0.1.0-preview.3
```

Pushing a `v*` tag runs CI, packs the projects, creates a GitHub release, and attaches the packed NuGet artifacts.

## Publishing To NuGet

Publishing is manual for now. After CI succeeds on the release tag, download the package artifact and publish:

```bash
dotnet nuget push RsqlParserNet.<version>.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key <NUGET_API_KEY>
```

Publish the `.snupkg` symbol package if it is not pushed automatically with the main package:

```bash
dotnet nuget push RsqlParserNet.<version>.snupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key <NUGET_API_KEY>
```

## GitHub Release

GitHub releases are created automatically by `.github/workflows/release.yml` when a `v*` tag is pushed.

The workflow attaches:

- `RsqlParserNet.<version>.nupkg`
- `RsqlParserNet.<version>.snupkg`
- `RsqlParserNet.Linq.<version>.nupkg`
- `RsqlParserNet.Linq.<version>.snupkg`
- `RsqlParserNet.AspNetCore.<version>.nupkg`
- `RsqlParserNet.AspNetCore.<version>.snupkg`

When packages have different versions, the GitHub release will attach the versions currently defined in each package project unless a workflow explicitly overrides them.

The initial release notes are populated from `CHANGELOG.md`. Edit the generated GitHub release notes after creation if the changelog contains unreleased or historical sections that should not appear in full.

## NuGet Release

NuGet publishing is intentionally manual for now. Publish only after the tag build and GitHub release succeed.

The repository expects a GitHub Actions secret named:

```text
NUGET_API_KEY
```

To publish, run the `Publish NuGet` workflow manually from GitHub Actions and enter the git ref and package version:

```text
git_ref: v0.1.0-preview.3
version: 0.1.0-preview.3
package: RsqlParserNet
```

For the LINQ adapter:

```text
git_ref: main
version: 0.1.0-preview.1
package: RsqlParserNet.Linq
```

For the ASP.NET Core adapter:

```text
git_ref: main
version: 0.1.0-preview.1
package: RsqlParserNet.AspNetCore
```

The `git_ref` should usually be the release tag. This keeps the package published to NuGet identical to the package attached to the GitHub release.

The workflow restores, builds, tests, packs the requested package and version from the requested git ref, and pushes the `.nupkg` and `.snupkg` to NuGet.
