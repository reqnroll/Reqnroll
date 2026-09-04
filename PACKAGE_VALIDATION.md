# NuGet Package Validation

This repository uses [Meziantou.Framework.NuGetPackageValidation.Tool](https://github.com/meziantou/Meziantou.Framework/tree/main/src/Meziantou.Framework.NuGetPackageValidation.Tool) to validate NuGet packages during the CI build process.

## What is validated

The tool validates various aspects of NuGet packages including:

- **Assembly optimization**: Ensures assemblies are optimized for release
- **Author information**: Validates that package author is properly set
- **Description and metadata**: Checks for required package metadata
- **Icons and documentation**: Ensures package has proper icon and XML documentation
- **Repository information**: Validates repository URLs and branch information
- **Symbols**: Checks symbol package validity
- **Readme files**: Ensures packages have readme documentation

## Current configuration

The validation is configured in `.github/workflows/ci.yml` and:

- ✅ Runs after package building in a separate validation job
- ✅ Includes all validation checks including deterministic builds
- ✅ Skips assembly optimization validation on non-main branches (to allow Debug builds in PRs)
- ✅ **Fails the build** when a package has validation issues not covered by its exclusions
- ✅ Uses per-package rule exclusions so each package can suppress only the checks it genuinely cannot pass yet

## Per-package rule overrides

Each known package is registered in an ordered hashtable (`$packageRuleOverrides`) inside the `Validate NuGet packages` step in `.github/workflows/ci.yml`. The key is the **NuGet package ID** (read from the `.nuspec` inside the `.nupkg`) and the value is an array of rule names to skip for that package.

### Adding a new package

When a new package is added to the repository, you **must** also register it in the hashtable, otherwise the build will fail with:

```
::error title=Unknown Package:: Package '<id>' is not registered in $packageRuleOverrides.
```

Add an entry like this to the hashtable in the `Validate NuGet packages` step:

```powershell
# No exclusions needed (preferred — the package passes all checks)
'My.New.Package' = @()

# With exclusions for known issues that are not yet resolved
'My.New.Package' = @('ReadmeMustBeSet', 'XmlDocumentationMustBePresent')
```

Run the validation locally (see below) to discover which rules need to be excluded, then add as few exclusions as possible.

## Error codes

Common error codes you might encounter, and their corresponding rule names:

| Code | Rule name                    | Description                                      |
|------|------------------------------|--------------------------------------------------|
| 12   | `AuthorMustBeSet`            | Author element not set explicitly                |
| 33   | `IconMustBeSet`              | Icon file not found                              |
| 52   | `ProjectUrlMustBeSet`        | Project URL not accessible                       |
| 61   | `ReadmeMustBeSet`            | Readme not set                                   |
| 81   | `AssembliesMustBeOptimized`  | Assembly not optimized (Debug builds)            |
| 101  | `XmlDocumentationMustBePresent` | XML documentation not found                   |
| 111  | `Symbols`                    | Symbol file not found                            |
| 112  | `Symbols`                    | Deterministic build issues in symbol file        |
| 119  | `Symbols`                    | Source file not accessible in symbol package     |

Run `meziantou.validate-nuget-package --help` for the full list of available rule names.

## Manual validation

You can run package validation locally:

```powershell
# Install the tool
dotnet tool install --global Meziantou.Framework.NuGetPackageValidation.Tool

# Validate a package
meziantou.validate-nuget-package path/to/package.nupkg

# Validate a package with specific rule exclusions
meziantou.validate-nuget-package path/to/package.nupkg --excluded-rules ReadmeMustBeSet,XmlDocumentationMustBePresent

# Validate a package excluding assembly optimization (for non-main branches)
meziantou.validate-nuget-package path/to/package.nupkg --excluded-rules AssembliesMustBeOptimized

# See all available rules
meziantou.validate-nuget-package --help
```
