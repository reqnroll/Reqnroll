# Dependency Changes Analysis - Reqnroll 3.2.1 → 3.3.0/3.3.1

## Methodology

This analysis compares dependencies across versions:
- **3.2.1**: Previous release (information from GitHub issues)
- **3.3.0**: Current release with issues
- **3.3.1**: Local build (analyzed from extracted packages)

**Limitations:** Cannot download packages from NuGet.org, so 3.2.1 information is based on:
- GitHub issue comments (#970, #972)
- Project file history in repository
- Assumptions based on packaging changes in PR #914

---

## Known Dependency Changes

### 1. Reqnroll.CustomPlugin

| Version | Dependencies | Notes |
|---------|--------------|-------|
| **3.2.1** | • Reqnroll [3.2.1]<br>• System.CodeDom 8.0.0 | From issue #972 |
| **3.3.0** | • Reqnroll [3.3.0, 4.0.0) | Missing System.CodeDom |
| **3.3.1** | • Reqnroll [3.3.1, 4.0.0) | Still missing System.CodeDom |

**Changes:**
- ❌ **LOST:** System.CodeDom 8.0.0 dependency
- ✅ **CHANGED:** Version constraint from exact `[3.2.1]` to range `[3.3.1, 4.0.0)`
- ✅ **ADDED:** `exclude="Build,Analyzers"` attribute

**Impact:** CRITICAL - Users cannot compile custom plugins (missing generator types)

---

### 2. Reqnroll.SpecFlowCompatibility

| Version | Dependencies | Notes |
|---------|--------------|-------|
| **3.2.1** | • Reqnroll [3.2.1] | Assumed from pattern |
| **3.3.0** | • Reqnroll [3.3.0, 4.0.0) | Package structure issue |
| **3.3.1** | • Reqnroll [3.3.1, 4.0.0) (per TFM) | Same as 3.3.0 |

**Changes:**
- ✅ **CHANGED:** Version constraint from exact to range
- ✅ **ADDED:** `exclude="Build,Analyzers"` attribute
- ✅ **ADDED:** Separate dependency groups for .NETFramework4.6.2 and .NETStandard2.0

**Impact:** Changes appear intentional, but package structure broken (missing build files)

---

## All Packages - Current Dependencies (3.3.1)

### Core Package

#### Reqnroll
**Dependencies:**
- Cucumber.CucumberExpressions 17.1.0
- Cucumber.HtmlFormatter 22.2.0
- Cucumber.Messages 30.1.0
- Gherkin 35.0.0
- Microsoft.Bcl.AsyncInterfaces 9.0.6
- Microsoft.Extensions.DependencyModel 8.0.2
- System.Text.Json 8.0.5
- System.Threading.Channels 9.0.6

**Known Changes from 3.2.1:**
- Cannot verify without access to 3.2.1 package
- CHANGELOG mentions: Updated Cucumber.HtmlFormatter to v22, Cucumber.Messages to v30 in 3.3.0

---

### Test Framework Adapters

#### Reqnroll.xUnit (v2)
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- Xunit.SkippableFact 1.4.13 ← **Pre-existing**
- xunit.core 2.8.1

**Changes from 3.2.1:**
- ✅ Xunit.SkippableFact: Already present (verified in git history)
- Version numbers updated to 3.3.1

#### Reqnroll.xunit.v3
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- xunit.v3.assert 2.0.0
- xunit.v3.extensibility.core 2.0.0

**Changes from 3.2.1:**
- N/A - This package is for xUnit v3 (newer)

#### Reqnroll.MSTest
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- MSTest.TestFramework 2.2.8

**Changes from 3.2.1:**
- Cannot verify without access to 3.2.1

#### Reqnroll.NUnit
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- NUnit 3.13.1

**Changes from 3.2.1:**
- Cannot verify without access to 3.2.1

#### Reqnroll.TUnit
**Dependencies:**
- Reqnroll.Tools.MsBuild.Generation 3.3.1
- Reqnroll 3.3.1
- TUnit.Core 1.3.25
- MSBuild.AdditionalTasks 0.1.36

**Changes from 3.2.1:**
- CHANGELOG notes: Updated to support TUnit v1.3.25 in 3.3.0

---

### Runtime Plugin Packages

#### Reqnroll.Autofac
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Autofac 4.0.0

**Changes from 3.2.1:**
- Version constraint changed from exact to range (assumed)

#### Reqnroll.Windsor
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Castle.Windsor 6.0.0

**Changes from 3.2.1:**
- Version constraint changed from exact to range (assumed)

#### Reqnroll.Microsoft.Extensions.DependencyInjection
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Microsoft.Extensions.DependencyInjection 6.0.0
- Microsoft.Extensions.Logging.Abstractions 6.0.0

**Changes from 3.2.1:**
- Version constraint changed from exact to range (assumed)

#### Reqnroll.Assist.Dynamic
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)
- Dynamitey 3.0.3

**Changes from 3.2.1:**
- Version constraint changed from exact to range (assumed)

#### Reqnroll.Verify
**Dependencies (per TFM):**
- Reqnroll [3.3.1, 4.0.0)
- Verify.Xunit 29.0.0

**Changes from 3.2.1:**
- CHANGELOG notes: Support for Verify v29+ in 3.1.0

---

### Generator Plugin Packages

#### Reqnroll.ExternalData
**Dependencies:**
- Reqnroll [3.3.1, 4.0.0)

**Changes from 3.2.1:**
- Version constraint changed from exact to range (assumed)

#### Reqnroll.Templates.DotNet
**Dependencies:**
- None listed

**Changes from 3.2.1:**
- No dependencies (templates package)

#### Reqnroll.Tools.MsBuild.Generation
**Dependencies:**
- None listed (self-contained)

**Changes from 3.2.1:**
- No dependencies

---

## Systematic Changes from PR #914

Based on the project-based packing changes in PR #914, the following systematic changes were applied to ALL packages:

### 1. Version Constraint Changes

**Before (3.2.1):**
```xml
<dependency id="Reqnroll" version="[3.2.1]" />
```

**After (3.3.0/3.3.1):**
```xml
<dependency id="Reqnroll" version="[3.3.1, 4.0.0)" exclude="Build,Analyzers" />
```

**Changes:**
- Exact version `[3.2.1]` → Version range `[3.3.1, 4.0.0)`
- Added `exclude="Build,Analyzers"` attribute
- Upper bound set to 4.0.0 (from `CompatibilityVersionUpperRange` property)

### 2. Dependency Exclusions

All Reqnroll dependencies now have `exclude="Build,Analyzers"` to prevent:
- Build-time assets from being included
- Analyzer DLLs from being propagated

### 3. Target Framework Groups

Many packages now have explicit dependency groups per target framework:
```xml
<group targetFramework=".NETFramework4.6.2">
<group targetFramework=".NETStandard2.0">
```

This is more explicit than 3.2.1 (assumed to have been less specific).

---

## Summary of Changes by Category

### ❌ Breaking/Critical Changes
1. **Reqnroll.CustomPlugin**: Lost System.CodeDom dependency

### ⚠️ Structural Changes (Package Format)
1. **All packages**: Version constraints changed from exact to range
2. **All packages**: Added exclude="Build,Analyzers" 
3. **Many packages**: Added explicit target framework dependency groups

### ✅ Version Updates (Expected)
1. **Reqnroll.TUnit**: Updated TUnit.Core to 1.3.25
2. **Core package**: Updated Cucumber.HtmlFormatter to v22, Messages to v30
3. All version numbers updated from 3.2.1 to 3.3.x

### ℹ️ No Changes Detected
1. **Reqnroll.xUnit**: Xunit.SkippableFact was already present
2. **Test framework versions**: NUnit 3.13.1, MSTest 2.2.8, xunit.core 2.8.1 appear unchanged

---

## Recommendations

### Immediate Action Required

1. **Restore System.CodeDom to Reqnroll.CustomPlugin**
   - Add back as explicit dependency in project file
   - Version: 8.0.0 (or compatible)

### Verification Needed

To complete this analysis, maintainers should:

1. **Download 3.2.1 packages from NuGet.org** and extract for comparison
2. **Compare nuspec files** between versions for all 16 packages
3. **Document any unintended dependency changes**
4. **Verify all test framework version numbers** (NUnit, MSTest, xUnit) didn't change unintentionally

### Script for Maintainers

```bash
# Download all 3.2.1 packages
packages=("Reqnroll" "Reqnroll.Autofac" "Reqnroll.CustomPlugin" "Reqnroll.ExternalData" 
          "Reqnroll.MSTest" "Reqnroll.Microsoft.Extensions.DependencyInjection" "Reqnroll.NUnit" 
          "Reqnroll.SpecFlowCompatibility" "Reqnroll.TUnit" "Reqnroll.Tools.MsBuild.Generation" 
          "Reqnroll.Verify" "Reqnroll.Windsor" "Reqnroll.xUnit" "Reqnroll.xunit.v3" 
          "Reqnroll.Assist.Dynamic" "Reqnroll.Templates.DotNet")

for pkg in "${packages[@]}"; do
    wget "https://www.nuget.org/api/v2/package/${pkg}/3.2.1" -O "${pkg}.3.2.1.nupkg" 2>/dev/null || echo "Not found: $pkg"
    unzip -q "${pkg}.3.2.1.nupkg" -d "extracted-3.2.1/${pkg}"
done

# Compare with 3.3.1
for pkg in "${packages[@]}"; do
    echo "=== $pkg ==="
    diff -u "extracted-3.2.1/${pkg}/${pkg}.nuspec" "extracted-3.3.1/${pkg}/${pkg}.nuspec" | grep -A5 -B5 "dependency"
done
```

---

## Data Sources

1. ✅ **GitHub Issue #972**: Provided CustomPlugin 3.2.1 dependencies
2. ✅ **GitHub Issue #970**: Described SpecFlowCompatibility structure
3. ✅ **CHANGELOG.md**: Version updates for Cucumber, TUnit dependencies
4. ✅ **Project files**: Current dependency declarations
5. ✅ **Git history**: Verified Xunit.SkippableFact pre-existence
6. ❌ **NuGet.org**: Cannot access for downloading 3.2.1 packages

---

*Analysis completed: 2025-12-18*  
*Note: Full comparison requires downloading 3.2.1 packages from NuGet.org*
