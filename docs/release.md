# Release Process

This project uses Semantic Versioning for NuGet packages.

Preview versions should use a prerelease suffix:

```text
0.1.0-preview.1
0.1.0-preview.2
0.3.0-preview.1
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
   Use absolute GitHub URLs for links in `README.md` because it is embedded into every NuGet package.
4. Run:

```bash
dotnet test RsqlParserNet.sln
dotnet pack src/RsqlParserNet/RsqlParserNet.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.Linq/RsqlParserNet.Linq.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.AspNetCore/RsqlParserNet.AspNetCore.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.EntityFrameworkCore/RsqlParserNet.EntityFrameworkCore.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.FastEndpoints/RsqlParserNet.FastEndpoints.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.OpenApi/RsqlParserNet.OpenApi.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.Swashbuckle/RsqlParserNet.Swashbuckle.csproj --configuration Release --output artifacts/packages
dotnet pack src/RsqlParserNet.NSwag/RsqlParserNet.NSwag.csproj --configuration Release --output artifacts/packages
```

If releasing only one package, the other package-specific pack steps can be skipped.

When releasing adapter packages for the first time, publish in dependency order:

```text
RsqlParserNet
RsqlParserNet.Linq
RsqlParserNet.AspNetCore
RsqlParserNet.EntityFrameworkCore
RsqlParserNet.FastEndpoints
RsqlParserNet.OpenApi
RsqlParserNet.Swashbuckle
RsqlParserNet.NSwag
```

5. Inspect the generated packages:

```bash
unzip -l artifacts/packages/RsqlParserNet.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.Linq.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.AspNetCore.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.EntityFrameworkCore.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.FastEndpoints.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.OpenApi.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.Swashbuckle.<version>.nupkg
unzip -l artifacts/packages/RsqlParserNet.NSwag.<version>.nupkg
```

## Package Versions

Packages can be versioned independently while the project is in preview, but aligned versions are preferred when publishing the whole package family from one release tag. The `0.3.0-preview.1` release aligns every package.

Current package identities:

```text
RsqlParserNet
RsqlParserNet.Linq
RsqlParserNet.AspNetCore
RsqlParserNet.EntityFrameworkCore
RsqlParserNet.FastEndpoints
RsqlParserNet.OpenApi
RsqlParserNet.Swashbuckle
RsqlParserNet.NSwag
```

## Tagging

Use tags that match the package version prefixed with `v`:

```bash
git tag -a v0.3.0-preview.1 -m "RsqlParserNet packages 0.3.0-preview.1"
git push origin v0.3.0-preview.1
```

Pushing a `v*` tag runs CI, packs the projects, creates a GitHub release, attaches the packed NuGet artifacts, and publishes packages to NuGet.org when `NUGET_API_KEY` is configured.

Tags with prerelease SemVer suffixes, such as `v0.3.0-preview.1`, are created as GitHub prereleases.

## Publishing To NuGet

Tag releases publish to NuGet.org automatically when the repository secret `NUGET_API_KEY` is configured. The release workflow pushes every generated `.nupkg` and `.snupkg` with `--skip-duplicate`.

Manual publishing remains available as a fallback. After CI succeeds on the release tag, download the package artifact and publish:

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

GitHub releases are created automatically by `.github/workflows/release.yml` when a `v*` tag is pushed. Preview tags are marked as prereleases.

The workflow attaches:

- `RsqlParserNet.<version>.nupkg`
- `RsqlParserNet.<version>.snupkg`
- `RsqlParserNet.Linq.<version>.nupkg`
- `RsqlParserNet.Linq.<version>.snupkg`
- `RsqlParserNet.AspNetCore.<version>.nupkg`
- `RsqlParserNet.AspNetCore.<version>.snupkg`
- `RsqlParserNet.EntityFrameworkCore.<version>.nupkg`
- `RsqlParserNet.EntityFrameworkCore.<version>.snupkg`
- `RsqlParserNet.FastEndpoints.<version>.nupkg`
- `RsqlParserNet.FastEndpoints.<version>.snupkg`
- `RsqlParserNet.OpenApi.<version>.nupkg`
- `RsqlParserNet.OpenApi.<version>.snupkg`
- `RsqlParserNet.Swashbuckle.<version>.nupkg`
- `RsqlParserNet.Swashbuckle.<version>.snupkg`
- `RsqlParserNet.NSwag.<version>.nupkg`
- `RsqlParserNet.NSwag.<version>.snupkg`

When packages have different versions, the GitHub release will attach the versions currently defined in each package project unless a workflow explicitly overrides them.

GitHub release notes are generated from commits and pull requests for the tag. Keep `CHANGELOG.md` as the human-maintained project history and package artifact, but do not paste the full changelog into every GitHub release.

After the workflow completes, verify:

- GitHub release exists.
- Preview GitHub releases are marked as prerelease.
- `.nupkg` and `.snupkg` files are attached.
- Packages appear on NuGet.org.
- Package README and repository links render correctly on NuGet.org.

## NuGet Release

NuGet publishing is automatic on `v*` tags when `NUGET_API_KEY` is configured. Publish only from tags that have passed CI and created the GitHub release artifacts.

The repository expects a GitHub Actions secret named:

```text
NUGET_API_KEY
```

If automatic publishing needs to be retried for one package, run the `Publish NuGet` workflow manually from GitHub Actions and enter the git ref and package version:

```text
git_ref: v0.3.0-preview.1
version: 0.3.0-preview.1
package: RsqlParserNet
```

Repeat the workflow for each package that should be published from the same release tag:

```text
RsqlParserNet
RsqlParserNet.Linq
RsqlParserNet.AspNetCore
RsqlParserNet.EntityFrameworkCore
RsqlParserNet.FastEndpoints
RsqlParserNet.OpenApi
RsqlParserNet.Swashbuckle
RsqlParserNet.NSwag
```

The `git_ref` should usually be the release tag. This keeps the package published to NuGet identical to the package attached to the GitHub release.

The workflow restores, builds, tests, packs the requested package and version from the requested git ref, and pushes the `.nupkg` and `.snupkg` to NuGet.

## GitHub Packages Preview Feed

The repository also includes a manual `Publish GitHub Packages` workflow for publishing preview packages to GitHub Packages without a NuGet.org API key.

Use it when packages should be tested from the repository's GitHub Packages feed before publishing to NuGet.org:

```text
git_ref: v0.3.0-preview.1
version: 0.3.0-preview.1
package: RsqlParserNet.Linq
```

GitHub Packages publishing uses the workflow `GITHUB_TOKEN` with `packages: write` permission. It publishes only the `.nupkg`; NuGet.org remains the preferred public registry for normal package discovery and symbol packages.

Consumers need to add the GitHub Packages source explicitly:

```bash
dotnet nuget add source \
  --username <github-user> \
  --password <github-token> \
  --store-password-in-clear-text \
  --name rsqlparsernet-github \
  https://nuget.pkg.github.com/ai-iskuzhin/index.json
```

Then install from that source:

```bash
dotnet add package RsqlParserNet.Linq \
  --version 0.3.0-preview.1 \
  --source rsqlparsernet-github
```
