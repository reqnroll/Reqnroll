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

The validation is configured in `.github/workflows/ci.yml` and currently:

- ✅ Runs after package building in a separate validation job
- ✅ Includes all validation checks including deterministic builds
- ✅ Skips assembly optimization validation on non-main branches (to allow Debug builds in PRs)
- ⚠️ Reports validation issues as warnings but does not fail the build (gradual improvement mode)

## Error codes

Common error codes you might encounter:

- **12**: Author element not set explicitly
- **33**: Icon file not found
- **52**: Project URL not accessible
- **61**: Readme not set
- **81**: Assembly not optimized (Debug builds)
- **101**: XML documentation not found
- **112**: Deterministic build issues
- **119**: Source file not accessible

## Future improvements

Once package validation issues are resolved, the CI can be updated to fail builds on validation errors instead of just reporting warnings.

## Manual validation

You can run package validation locally:

```powershell
# Install the tool
dotnet tool install --global Meziantou.Framework.NuGetPackageValidation.Tool

# Validate a package
meziantou.validate-nuget-package path/to/package.nupkg

# Validate a package excluding assembly optimization (for non-main branches)
meziantou.validate-nuget-package path/to/package.nupkg --excluded-rules AssembliesMustBeOptimized

# See all available rules
meziantou.validate-nuget-package --help
```
