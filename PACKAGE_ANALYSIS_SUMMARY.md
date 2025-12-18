# Reqnroll 3.3.0 NuGet Package Analysis Report

## Executive Summary

This report analyzes the issues with Reqnroll 3.3.0 NuGet packages compared to version 3.2.1. The analysis was conducted on 2025-12-18 in response to GitHub issues #970 and #972.

**Key Findings:**
- **Reqnroll.SpecFlowCompatibility.3.3.0**: Missing generator plugin DLL files in `build/net462` and `build/netstandard2.0` folders
- **Reqnroll.CustomPlugin.3.3.0**: Missing lib folder with required assemblies, changed dependency structure  
- **Root Cause**: Changes introduced in PR #914 "Switch to project-based packing"

---

## Issue #970: Reqnroll.SpecFlowCompatibility Package

### Problem Description
Users upgrading to Reqnroll 3.3.0 encounter a build error:
```
[Reqnroll] System.IO.FileNotFoundException: Could not load file or assembly 
'file:///C:\Users\...\reqnroll.specflowcompatibility\3.3.0\build\net462\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll' 
or one of its dependencies. The system cannot find the file specified.
```

### Package Analysis

#### Version 3.2.1 (Working)
Structure:
```
build/
  net462/
    Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
    [other dependencies]
  netstandard2.0/
    Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
    [other dependencies]
  Reqnroll.SpecFlowCompatibility.props
  Reqnroll.SpecFlowCompatibility.targets
lib/
  net462/
    Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll
  netstandard2.0/
    Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll
```

#### Version 3.3.0 (Broken)
Structure:
```
build/
  Reqnroll.SpecFlowCompatibility.props
  Reqnroll.SpecFlowCompatibility.targets
lib/
  net462/
    Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll
  netstandard2.0/
    Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll
```

**Missing:** The `build/net462/` and `build/netstandard2.0/` subdirectories containing the generator plugin DLL.

### What the Targets File Expects

From `Reqnroll.SpecFlowCompatibility.targets` line 7:
```xml
<_ReqnrollSpecFlowCompatibilityGeneratorPluginPath>
  $(MSBuildThisFileDirectory)\$(_ReqnrollSpecFlowCompatibilityGeneratorPlugin)\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
</_ReqnrollSpecFlowCompatibilityGeneratorPluginPath>
```

This expects the generator plugin DLL to be in:
- `build/net462/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll` (for MSBuild on .NET Framework)
- `build/netstandard2.0/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll` (for MSBuild on .NET Core)

### Impact
- **Severity**: Critical
- **Affects**: All users of Reqnroll.SpecFlowCompatibility package
- **Workaround**: Downgrade to version 3.2.1

---

## Issue #972: Reqnroll.CustomPlugin Package

### Problem Description
Users trying to create custom generator plugins cannot find the required types:
- `IGeneratorPlugin` interface not found
- `Reqnroll.Generator.CodeDom` namespace missing
- `Reqnroll.Generator.UnitTestProvider` namespace missing

### Package Analysis

#### Version 3.2.1 NuSpec (Working)
```xml
<dependencies>
  <dependency id="Reqnroll" version="[3.2.1]" />
  <dependency id="System.CodeDom" version="8.0.0" />
</dependencies>
```

Expected to have `lib` folder with:
- Reqnroll.Generator.dll
- Reqnroll.dll  
- System.CodeDom.dll

#### Version 3.3.0 NuSpec (Broken)
```xml
<dependencies>
  <group targetFramework=".NETStandard2.0">
    <dependency id="Reqnroll" version="[3.3.0, 4.0.0)" exclude="Build,Analyzers" />
  </group>
</dependencies>
```

Package contents:
```
build/
  netstandard2.0/
    Reqnroll.CustomPlugin.dll (4KB - stub only)
    Reqnroll.CustomPlugin.deps.json
```

**Missing:**
1. No `lib` folder with compile-time assemblies
2. Missing `System.CodeDom` dependency
3. The DLL in build/ is just a 4KB stub, not including Reqnroll.Generator types

### What Changed

The CustomPlugin package is now structured as a **generator plugin** (with files in `build/` folder) rather than a **library package** (with files in `lib/` folder). This means:

**3.2.1 Approach:**
- Users reference the package
- NuGet restores `lib/` assemblies for compilation
- Users can use `IGeneratorPlugin`, `CodeDom` types at compile time

**3.3.0 Approach:**
- Package only provides build-time MSBuild plugin in `build/`
- No `lib/` assemblies for compilation
- Users cannot reference generator types at compile time
- The dependency on `System.CodeDom` was removed

### Impact
- **Severity**: Critical
- **Affects**: All developers creating custom Reqnroll generator plugins
- **Workaround**: Downgrade to version 3.2.1

---

## Root Cause Analysis

### PR #914: "Switch to project-based packing"

The issue was introduced in PR #914 which:
1. Rebuilt the NuGet packing process to use project-based packing
2. Removed .nuspec files in favor of MSBuild project properties
3. Added MSBuild prop and target files for plugin packaging

### What Went Wrong

#### For Reqnroll.SpecFlowCompatibility:

The `Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.csproj` references:
```xml
<ProjectReference
  Include="..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\..."
  PrivateAssets="all"
  PackAsReqnrollGeneratorPlugin="true" />
```

The `PackAsReqnrollGeneratorPlugin="true"` attribute should trigger including the generator plugin DLLs in the `build/` folder, but this is not working correctly.

#### For Reqnroll.CustomPlugin:

The `Reqnroll.CustomPlugin.csproj` sets:
```xml
<IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>
```

This causes it to be packaged as a generator plugin (build/ folder) rather than a library (lib/ folder), which breaks the use case where developers need to reference generator types at compile time.

---

## Affected Packages

Based on GitHub issues and package analysis, the following packages are confirmed to have issues in 3.3.0:

| Package Name | Version 3.3.0 Status | Issue | Severity |
|--------------|----------------------|-------|----------|
| Reqnroll.SpecFlowCompatibility | ❌ Broken | Missing build/net462 and build/netstandard2.0 generator DLLs | Critical |
| Reqnroll.CustomPlugin | ❌ Broken | Missing lib/ folder, wrong package structure | Critical |

### Other Packages (Status Unknown)

The following packages had 3.3.0 versions released and should be checked:

| Package Name | Requires Verification |
|--------------|----------------------|
| Reqnroll | ✓ |
| Reqnroll.Assist.Dynamic | ✓ |
| Reqnroll.Autofac | ✓ |
| Reqnroll.ExternalData | ✓ |
| Reqnroll.MSTest | ✓ |
| Reqnroll.Microsoft.Extensions.DependencyInjection | ✓ |
| Reqnroll.NUnit | ✓ |
| Reqnroll.TUnit | ✓ |
| Reqnroll.Templates.DotNet | ✓ |
| Reqnroll.Tools.MsBuild.Generation | ✓ |
| Reqnroll.Verify | ✓ |
| Reqnroll.Windsor | ✓ |
| Reqnroll.xUnit | ✓ |
| Reqnroll.xunit.v3 | ✓ |

---

## Recommendations

### Immediate Actions (Required)

1. **Issue a hotfix release (3.3.1 or 3.3.0-hotfix1)** that fixes the packaging issues
2. **Unlist Reqnroll.SpecFlowCompatibility 3.3.0** from NuGet.org to prevent new installations
3. **Unlist Reqnroll.CustomPlugin 3.3.0** from NuGet.org to prevent new installations

### Required Fixes

#### Reqnroll.SpecFlowCompatibility:

The generator plugin DLLs need to be included in the package at:
- `build/net462/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll`
- `build/netstandard2.0/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll`

The `PackAsReqnrollGeneratorPlugin="true"` mechanism needs to be debugged to understand why it's not including these files.

#### Reqnroll.CustomPlugin:

This package needs a different approach because it serves two purposes:
1. **Compile-time library**: Developers need to reference `IGeneratorPlugin` and other types
2. **Documentation/template**: Show how plugins are structured

Options:
- **Option A**: Revert to lib/ package structure with Reqnroll.Generator as dependency
- **Option B**: Split into two packages:
  - `Reqnroll.CustomPlugin` (lib/ with compile-time types)
  - `Reqnroll.CustomPlugin.Templates` (example project structure)
- **Option C**: Include both lib/ and build/ folders in the package

**Recommended: Option A** - Restore the 3.2.1 package structure with lib/ folder and add back the System.CodeDom dependency.

### Testing Improvements

1. **Add package content validation tests** that verify:
   - Expected folders exist in packages
   - Required DLL files are present
   - nuspec dependencies are correct
2. **Add integration tests** that:
   - Install and use Reqnroll.SpecFlowCompatibility in a test project
   - Create a custom plugin using Reqnroll.CustomPlugin
3. **Package comparison checks in CI**:
   - Compare new package structure with previous version
   - Flag any missing files or folders
   - Validate dependencies haven't changed unexpectedly

### Process Improvements

1. **Package release checklist** should include:
   - Manual inspection of generated .nupkg files
   - Extraction and verification of package contents
   - Comparison with previous version packages
2. **Automated package validation** using tools like:
   - NuGet Package Explorer
   - Custom validation scripts
   - Meziantou.Framework.NuGetPackageValidation.Tool (already in use per PACKAGE_VALIDATION.md)

---

## Verification Steps (For Package Maintainers)

To verify a package is correctly built:

1. **Extract the .nupkg file** (it's a ZIP archive):
   ```bash
   unzip Reqnroll.PackageName.3.3.x.nupkg -d extracted/
   ```

2. **Verify folder structure** matches expectations:
   - For runtime plugins: Check `lib/` folder
   - For generator plugins: Check `build/` folder
   - For hybrid packages: Check both

3. **Check .nuspec file** for correct dependencies:
   ```bash
   cat extracted/Reqnroll.PackageName.nuspec
   ```

4. **Compare with previous version**:
   ```bash
   diff -r extracted-old/ extracted-new/
   ```

5. **Test in a real project**:
   - Create a minimal test project
   - Install the package
   - Build and verify it works

---

## Timeline

- **2025-12-17**: Reqnroll 3.3.0 released
- **2025-12-18**: Issue #970 reported (SpecFlowCompatibility)
- **2025-12-18**: Issue #972 reported (CustomPlugin)
- **2025-12-18**: This analysis conducted

---

## Related Resources

- GitHub Issue #970: https://github.com/reqnroll/Reqnroll/issues/970
- GitHub Issue #972: https://github.com/reqnroll/Reqnroll/issues/972
- GitHub PR #914: https://github.com/reqnroll/Reqnroll/pull/914
- PACKAGE_VALIDATION.md: Package validation documentation in repo

---

*Report generated: 2025-12-18*
*Analysis conducted by: GitHub Copilot Agent*
