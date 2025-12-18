# Reqnroll 3.3.0 Package Analysis - Detailed Package Comparison

## Summary of All Analyzed Packages (3.3.1 local build)

### Packages with Issues

| Package | Issue | Severity | Status |
|---------|-------|----------|--------|
| **Reqnroll.CustomPlugin** | Missing `lib/` folder - no compile-time assemblies available | 🔴 Critical | Broken |
| **Reqnroll.SpecFlowCompatibility** | Missing generator plugin DLLs in `build/net462/` and `build/netstandard2.0/` | 🔴 Critical | Broken |

### Packages WITHOUT `lib/` folder (Generator Plugins Only)

These packages are **generator-only plugins** and this is expected:

| Package | Note |
|---------|------|
| Reqnroll.ExternalData | ✓ Correct - Generator plugin only |

### Packages WITH Both `lib/` and `build/` (Hybrid Packages)

These packages work correctly with both runtime and build-time components:

| Package | Runtime (lib/) | Generator (build/) | Status |
|---------|----------------|-------------------|--------|
| Reqnroll | ✓ Multiple TFMs | ✓ Props/Targets | ✓ OK |
| Reqnroll.Assist.Dynamic | ✓ netstandard2.0 | ✓ Props/Targets | ✓ OK |
| Reqnroll.Autofac | ✓ netstandard2.0 | ✓ Props/Targets | ✓ OK |
| Reqnroll.MSTest | ✓ netstandard2.0 | ✓ Generator DLL | ✓ OK |
| Reqnroll.Microsoft.Extensions.DependencyInjection | ✓ netstandard2.0 | ✓ Props/Targets | ✓ OK |
| Reqnroll.NUnit | ✓ netstandard2.0 | ✓ Generator DLL | ✓ OK |
| Reqnroll.TUnit | ✓ netstandard2.0 | ✓ Generator DLL | ✓ OK |
| Reqnroll.Verify | ✓ net472, net8.0 | ✓ Props/Targets | ✓ OK |
| Reqnroll.Windsor | ✓ netstandard2.0 | ✓ Props/Targets | ✓ OK |
| Reqnroll.xUnit | ✓ net462, netstandard2.0 | ✓ Generator DLLs in subdirs | ✓ OK |
| Reqnroll.xunit.v3 | ✓ netstandard2.0 | ✓ Generator DLL | ✓ OK |

---

## Detailed Issue Analysis

### Issue 1: Reqnroll.CustomPlugin (Critical)

#### Problem
The CustomPlugin package is missing the `lib/` folder entirely, which means developers cannot reference the generator types at compile time.

#### What's in 3.3.1:
```
build/
  netstandard2.0/
    Reqnroll.CustomPlugin.dll (4KB stub)
    Reqnroll.CustomPlugin.deps.json
```

#### What SHOULD be there (based on use case):
```
lib/
  netstandard2.0/
    Reqnroll.Generator.dll
    Reqnroll.dll
    System.CodeDom.dll
    [other dependencies]
```

#### Dependencies in 3.3.1:
```xml
<dependency id="Reqnroll" version="[3.3.1, 4.0.0)" exclude="Build,Analyzers" />
```

#### Dependencies in 3.2.1 (from issue #972):
```xml
<dependency id="Reqnroll" version="[3.2.1]" />
<dependency id="System.CodeDom" version="8.0.0" />
```

#### Missing Types
When developers try to use CustomPlugin 3.3.0, they cannot find:
- `Reqnroll.Generator.IGeneratorPlugin`
- `Reqnroll.Generator.CodeDom` namespace
- `Reqnroll.Generator.UnitTestProvider` namespace
- Any other generator extension points

#### Root Cause
The project is marked as `<IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>`, which causes it to be packaged as a **generator plugin** (files in `build/`) rather than a **library** (files in `lib/`).

However, the PURPOSE of this package is to provide compile-time types for developers writing custom plugins, not to be a plugin itself.

#### Fix Required
The package structure needs to be changed to:
1. **Option A** (Recommended): Change to library package structure
   - Remove `<IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>`
   - Add explicit references to Reqnroll.Generator as a package dependency
   - Package will have `lib/` folder with necessary assemblies

2. **Option B**: Hybrid package with both `lib/` and `build/`
   - Keep generator plugin structure for examples
   - Add `lib/` folder with compile-time assemblies
   - Requires custom packaging logic

---

### Issue 2: Reqnroll.SpecFlowCompatibility (Critical)

#### Problem
The package is missing the generator plugin DLL files that the `.targets` file expects to find.

#### What's in 3.3.1:
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

#### What SHOULD be there (based on 3.2.1 structure from issue #970):
```
build/
  net462/
    Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
    [dependencies]
  netstandard2.0/
    Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
    [dependencies]
  Reqnroll.SpecFlowCompatibility.props
  Reqnroll.SpecFlowCompatibility.targets
lib/
  net462/
    Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll
  netstandard2.0/
    Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll
```

#### What the Targets File Expects
From `Reqnroll.SpecFlowCompatibility.targets`:
```xml
<_ReqnrollSpecFlowCompatibilityGeneratorPluginPath>
  $(MSBuildThisFileDirectory)\$(_ReqnrollSpecFlowCompatibilityGeneratorPlugin)\
  Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll
</_ReqnrollSpecFlowCompatibilityGeneratorPluginPath>
```

Where `$(_ReqnrollSpecFlowCompatibilityGeneratorPlugin)` is either `net462` or `netstandard2.0`.

This path resolves to:
- `build/net462/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll`
- `build/netstandard2.0/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll`

#### Root Cause
The `PackAsReqnrollGeneratorPlugin="true"` attribute in the project reference should trigger the inclusion of generator plugin DLLs, but the MSBuild targets (`Reqnroll.Plugins.GeneratorPlugin.targets`) are not correctly packaging them into the TFM-specific subdirectories.

Looking at the project structure:
```xml
<!-- Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.csproj -->
<ProjectReference
  Include="..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\..."
  PrivateAssets="all"
  PackAsReqnrollGeneratorPlugin="true" />
```

The `PackAsReqnrollGeneratorPlugin` attribute should be processed by custom MSBuild logic to include the generator plugin, but this is not working.

#### Comparison with Working Packages
Other packages that DO include generator plugins correctly:

**Reqnroll.xUnit** (working):
```
build/
  net462/
    Reqnroll.xUnit.Generator.ReqnrollPlugin.dll ✓
  netstandard2.0/
    Reqnroll.xUnit.Generator.ReqnrollPlugin.dll ✓
```

**Reqnroll.MSTest** (working):
```
build/
  netstandard2.0/
    Reqnroll.MSTest.Generator.ReqnrollPlugin.dll ✓
```

#### Fix Required
The MSBuild packaging logic needs to be fixed to:
1. Identify project references with `PackAsReqnrollGeneratorPlugin="true"`
2. Build those projects for the appropriate target frameworks
3. Include the built DLLs in `build/<tfm>/` subdirectories
4. Include their dependencies in the same subdirectories

---

## Technical Details: How Packaging SHOULD Work

### For Runtime Plugins (lib/ folder)
```xml
<PropertyGroup>
  <IsReqnrollRuntimePlugin>true</IsReqnrollRuntimePlugin>
</PropertyGroup>
```
Results in:
```
lib/
  <tfm>/
    Plugin.dll
```

### For Generator Plugins (build/ folder)
```xml
<PropertyGroup>
  <IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>
</PropertyGroup>
```
Results in:
```
build/
  <tfm>/
    Plugin.dll
    Plugin.deps.json
    [dependencies]
```

### For Hybrid Packages (both lib/ and build/)
```xml
<PropertyGroup>
  <IsReqnrollRuntimePlugin>true</IsReqnrollRuntimePlugin>
</PropertyGroup>
<ItemGroup>
  <ProjectReference ... PackAsReqnrollGeneratorPlugin="true" />
</ItemGroup>
```
Results in:
```
lib/
  <tfm>/
    RuntimePlugin.dll
build/
  <tfm>/
    GeneratorPlugin.dll
    [dependencies]
  Package.props
  Package.targets
```

---

## Testing Recommendations

### Pre-Release Package Validation

Before releasing any NuGet package, the following should be verified:

1. **Extract and Inspect**
   ```bash
   unzip Package.X.Y.Z.nupkg -d extracted/
   tree extracted/
   ```

2. **Verify nuspec Dependencies**
   ```bash
   cat extracted/*.nuspec | grep -A 20 "<dependencies>"
   ```

3. **Compare with Previous Version**
   ```bash
   diff -r extracted-old/ extracted-new/
   ```

4. **Check for Required Files**
   - Generator plugins: Verify `build/<tfm>/Plugin.dll` exists
   - Runtime plugins: Verify `lib/<tfm>/Plugin.dll` exists
   - Targets files: Verify paths in .targets match actual file locations

5. **Integration Test**
   - Create a minimal test project
   - Install the package
   - Build and run tests
   - Verify all expected functionality works

### Automated CI Checks

Add CI validation step that:
1. Extracts all built packages
2. Validates structure matches expected patterns
3. Compares with previous version
4. Fails if critical files are missing

---

## Comparison: 3.2.1 vs 3.3.0 Changes

### Package Structure Changes (from PR #914)

**Before (3.2.1):**
- Used `.nuspec` files for package metadata
- Manual maintenance of dependencies
- Custom packaging logic

**After (3.3.0):**
- Project-based packing using MSBuild properties
- Auto-generated `.nuspec` from project properties
- Standardized packaging via `.targets` files

### What Went Wrong
The migration to project-based packing introduced issues where:
1. Custom plugin packaging (`PackAsReqnrollGeneratorPlugin`) doesn't work correctly
2. Some packages changed structure unintentionally (CustomPlugin)
3. Testing didn't catch the structural changes

---

## Affected Users

### SpecFlowCompatibility Users
**Impact:** Cannot build projects after upgrading to 3.3.0

**Error Message:**
```
[Reqnroll] System.IO.FileNotFoundException: Could not load file or assembly 
'...reqnroll.specflowcompatibility\3.3.0\build\net462\
Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll'
```

**Workaround:** Downgrade to 3.2.1

### CustomPlugin Users  
**Impact:** Cannot create custom generator plugins

**Error Messages:**
```
Error CS0234: The type or namespace name 'CodeDom' does not exist in the namespace 'Reqnroll.Generator'
Error CS0234: The type or namespace name 'UnitTestProvider' does not exist in the namespace 'Reqnroll.Generator'
```

**Workaround:** Downgrade to 3.2.1

---

## Recommendations

### Immediate (Release 3.3.1)

1. ✅ Fix Reqnroll.SpecFlowCompatibility packaging
2. ✅ Fix Reqnroll.CustomPlugin packaging
3. ✅ Unlist 3.3.0 versions of affected packages
4. ✅ Release 3.3.1 with fixes
5. ✅ Update CHANGELOG with package fixes

### Short-term

1. Add package content validation tests
2. Add integration tests for all packages
3. Document package structure requirements
4. Create package release checklist

### Long-term

1. Automate package validation in CI
2. Add package comparison checks
3. Consider split CustomPlugin into library + templates
4. Review all packages for similar issues

---

*Analysis completed: 2025-12-18*
*Packages analyzed: 16 Reqnroll packages (3.3.1 local build)*
