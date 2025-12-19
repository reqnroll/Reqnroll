# Suggested Fixes for Reqnroll 3.3.0 Package Issues

## Executive Summary

This document provides concrete, actionable fixes for the two critical packaging issues identified in Reqnroll 3.3.0.

---

## Fix #1: Reqnroll.CustomPlugin Package

### Problem
The package is incorrectly configured as a generator plugin, resulting in missing `lib/` folder and compile-time assemblies.

### Solution: Change to Library Package Structure

**File to modify:** `/Plugins/Reqnroll.CustomPlugin/Reqnroll.CustomPlugin.csproj`

**Current code:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0</TargetFrameworks>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>
    
    <!-- NuGet configuration -->
    <PackageId>Reqnroll.CustomPlugin</PackageId>
    <Description>Package for writing custom generator extensions for Reqnroll.</Description>
    <PackageTags>reqnroll bdd gherkin cucumber generator</PackageTags>
  </PropertyGroup>

</Project>
```

**Fixed code:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0</TargetFrameworks>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <IsPackable>true</IsPackable>
    
    <!-- NuGet configuration -->
    <PackageId>Reqnroll.CustomPlugin</PackageId>
    <Description>Package for writing custom generator extensions for Reqnroll.</Description>
    <PackageTags>reqnroll bdd gherkin cucumber generator</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <!-- Add explicit package reference to System.CodeDom -->
    <PackageReference Include="System.CodeDom" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Reference Reqnroll.Generator as a regular dependency (not PrivateAssets) -->
    <ProjectReference Include="$(MSBuildThisFileDirectory)../../Reqnroll.Generator/Reqnroll.Generator.csproj" />
  </ItemGroup>

</Project>
```

**Changes made:**
1. ❌ **Removed:** `<IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>` - This was causing the package to be structured as a generator plugin
2. ✅ **Added:** `<IsPackable>true</IsPackable>` - Explicitly mark as packable
3. ✅ **Added:** System.CodeDom package reference (version 8.0.0)
4. ✅ **Added:** ProjectReference to Reqnroll.Generator (without PrivateAssets=all)

**Result:**
- Package will have `lib/netstandard2.0/` folder with assemblies
- Developers can reference `IGeneratorPlugin` and other generator types
- NuGet dependencies will include `System.CodeDom` and `Reqnroll`

---

## Fix #2: Reqnroll.SpecFlowCompatibility Package

### Problem
The `PackAsReqnrollGeneratorPlugin="true"` attribute on the generator plugin project reference is not working correctly. The generator plugin DLLs are not being included in the `build/net462/` and `build/netstandard2.0/` subdirectories.

### Root Cause Analysis

Looking at the `Reqnroll.Plugins.TestFrameworkAdapter.targets` file (lines 133-147), the `ResolveReqnrollGeneratorPluginFiles` target is supposed to:

```xml
<Target
  Name="ResolveReqnrollGeneratorPluginFiles"
  DependsOnTargets="$(ResolveReqnrollGeneratorPluginFilesDependsOn)">

  <ItemGroup>
    <ReqnrollGeneratorPluginFile
      Include="@(ReferenceCopyLocalPaths)"
      Condition="'%(ReferenceCopyLocalPaths.PackAsReqnrollGeneratorPlugin)' == 'true'" />
  </ItemGroup>

  <ItemGroup>
    <TfmSpecificPackageFile Include="@(ReqnrollGeneratorPluginFile)" PackagePath="build/$(TargetFramework)/" />
    <TfmSpecificPackageFile Update="@(ReqnrollGeneratorPluginFile)" PackFolder="Symbols" Condition="'%(Extension)' == '.pdb'" />
  </ItemGroup>
</Target>
```

This target should include files from referenced projects that have `PackAsReqnrollGeneratorPlugin="true"`, but it's only checking `ReferenceCopyLocalPaths` items.

### Solution Option A: Ensure Generator Plugin is Properly Multi-Targeted

The issue is that `Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin` has `IsPackable>false</IsPackable>`, which might prevent it from being properly included.

**File to modify:** `/Plugins/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.csproj`

**Current code:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net462</TargetFrameworks>
    <IsReqnrollGeneratorPlugin>true</IsReqnrollGeneratorPlugin>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Reqnroll.SpecFlowCompatibility\Reqnroll.SpecFlowCompatibility.csproj" />
  </ItemGroup>
</Project>
```

**Check if this is correct** - This project should remain `IsPackable>false` because it's being included via the parent package.

### Solution Option B: Verify MSBuild Metadata Propagation

The issue might be that the `PackAsReqnrollGeneratorPlugin` metadata is not being properly propagated to the `ReferenceCopyLocalPaths` items.

**File to check:** `/Plugins/Reqnroll.SpecFlowCompatibility.ReqnrollPlugin/Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.csproj`

**Current code:**
```xml
<ItemGroup>
  <ProjectReference
    Include="..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.csproj"
    PrivateAssets="all"
    PackAsReqnrollGeneratorPlugin="true" />
  <ProjectReference
    Include="..\Reqnroll.SpecFlowCompatibility\Reqnroll.SpecFlowCompatibility.csproj"
    PrivateAssets="all" />
</ItemGroup>
```

**Issue:** The `PrivateAssets="all"` might be preventing the assemblies from being included in `ReferenceCopyLocalPaths`.

**Proposed fix:**
```xml
<ItemGroup>
  <ProjectReference
    Include="..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.csproj"
    PrivateAssets="all"
    PackAsReqnrollGeneratorPlugin="true"
    ReferenceOutputAssembly="false" />
  <ProjectReference
    Include="..\Reqnroll.SpecFlowCompatibility\Reqnroll.SpecFlowCompatibility.csproj"
    PrivateAssets="all" />
</ItemGroup>
```

However, this might not be enough. Let me check what xUnit does (which works correctly).

### Solution Option C: Manual Build Output Inclusion (Workaround)

If the MSBuild metadata propagation is broken, we can manually include the generator plugin assemblies.

**File to modify:** `/Plugins/Reqnroll.SpecFlowCompatibility.ReqnrollPlugin/Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.csproj`

**Add after the ItemGroup with ProjectReferences:**

```xml
<PropertyGroup>
  <TargetsForTfmSpecificContentInPackage>$(TargetsForTfmSpecificContentInPackage);IncludeSpecFlowCompatibilityGeneratorPlugin</TargetsForTfmSpecificContentInPackage>
</PropertyGroup>

<Target Name="IncludeSpecFlowCompatibilityGeneratorPlugin" DependsOnTargets="ResolveProjectReferences">
  <ItemGroup>
    <!-- Include the generator plugin DLL for each target framework -->
    <TfmSpecificPackageFile 
      Include="$(MSBuildThisFileDirectory)..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\bin\$(Configuration)\net462\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll" 
      PackagePath="build/net462/" 
      Condition="Exists('$(MSBuildThisFileDirectory)..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\bin\$(Configuration)\net462\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll')" />
    <TfmSpecificPackageFile 
      Include="$(MSBuildThisFileDirectory)..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\bin\$(Configuration)\netstandard2.0\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll" 
      PackagePath="build/netstandard2.0/" 
      Condition="Exists('$(MSBuildThisFileDirectory)..\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin\bin\$(Configuration)\netstandard2.0\Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll')" />
  </ItemGroup>
</Target>
```

### Solution Option D: Fix the TestFrameworkAdapter.targets (Recommended)

The real issue is that the `ResolveReqnrollGeneratorPluginFiles` target needs to be enhanced to better handle the `PackAsReqnrollGeneratorPlugin` metadata.

**File to modify:** `/Plugins/Reqnroll.Plugins.TestFrameworkAdapter.targets`

**Current code (lines 133-147):**
```xml
<Target
  Name="ResolveReqnrollGeneratorPluginFiles"
  DependsOnTargets="$(ResolveReqnrollGeneratorPluginFilesDependsOn)">

  <ItemGroup>
    <ReqnrollGeneratorPluginFile
      Include="@(ReferenceCopyLocalPaths)"
      Condition="'%(ReferenceCopyLocalPaths.PackAsReqnrollGeneratorPlugin)' == 'true'" />
  </ItemGroup>

  <ItemGroup>
    <TfmSpecificPackageFile Include="@(ReqnrollGeneratorPluginFile)" PackagePath="build/$(TargetFramework)/" />
    <TfmSpecificPackageFile Update="@(ReqnrollGeneratorPluginFile)" PackFolder="Symbols" Condition="'%(Extension)' == '.pdb'" />
  </ItemGroup>
</Target>
```

**Issue:** The `PackAsReqnrollGeneratorPlugin` metadata might not be flowing through to `ReferenceCopyLocalPaths`.

**Enhanced code:**
```xml
<Target
  Name="ResolveReqnrollGeneratorPluginFiles"
  DependsOnTargets="$(ResolveReqnrollGeneratorPluginFilesDependsOn)">

  <!-- Get the project references that should be packed as generator plugins -->
  <ItemGroup>
    <_GeneratorPluginProjectReference 
      Include="@(ProjectReference)" 
      Condition="'%(ProjectReference.PackAsReqnrollGeneratorPlugin)' == 'true'" />
  </ItemGroup>

  <!-- Find the output assemblies from those project references -->
  <ItemGroup>
    <ReqnrollGeneratorPluginFile
      Include="@(ReferenceCopyLocalPaths)"
      Condition="'%(ReferenceCopyLocalPaths.PackAsReqnrollGeneratorPlugin)' == 'true' OR 
                 '@(_GeneratorPluginProjectReference)' != '' AND 
                 $([System.String]::new('%(ReferenceCopyLocalPaths.MSBuildSourceProjectFile)').Contains('%(_GeneratorPluginProjectReference.Identity)'))" />
  </ItemGroup>

  <ItemGroup>
    <TfmSpecificPackageFile Include="@(ReqnrollGeneratorPluginFile)" PackagePath="build/$(TargetFramework)/" />
    <TfmSpecificPackageFile Update="@(ReqnrollGeneratorPluginFile)" PackFolder="Symbols" Condition="'%(Extension)' == '.pdb'" />
  </ItemGroup>
</Target>
```

**Changes made:**
1. Added intermediate step to collect project references with `PackAsReqnrollGeneratorPlugin="true"`
2. Enhanced condition to check if the assembly comes from one of those project references
3. This ensures the generator plugin DLLs are included even if metadata doesn't flow through

---

## Recommended Implementation Order

1. **Fix #1 (CustomPlugin)** - HIGH PRIORITY, SIMPLE FIX
   - Modify `Reqnroll.CustomPlugin.csproj` as shown above
   - Test by building and extracting the package
   - Verify `lib/netstandard2.0/` folder exists with assemblies

2. **Fix #2 (SpecFlowCompatibility)** - HIGH PRIORITY, COMPLEX FIX
   - Start with **Solution Option D** (fix the targets file)
   - If that doesn't work, fall back to **Solution Option C** (manual inclusion)
   - Test by building and extracting the package
   - Verify `build/net462/` and `build/netstandard2.0/` folders exist with generator DLLs

---

## Testing Checklist

### For CustomPlugin Fix:
- [ ] Build the package: `dotnet pack -c Release`
- [ ] Extract the `.nupkg`: `unzip Reqnroll.CustomPlugin.3.3.x.nupkg -d test/`
- [ ] Verify structure:
  - [ ] `lib/netstandard2.0/Reqnroll.CustomPlugin.dll` exists
  - [ ] `lib/netstandard2.0/Reqnroll.Generator.dll` exists (or as dependency)
  - [ ] `lib/netstandard2.0/System.CodeDom.dll` exists (or as dependency)
- [ ] Check `.nuspec` dependencies include System.CodeDom
- [ ] Create test project referencing package
- [ ] Verify `IGeneratorPlugin` interface is resolvable

### For SpecFlowCompatibility Fix:
- [ ] Build the package: `dotnet pack -c Release`
- [ ] Extract the `.nupkg`: `unzip Reqnroll.SpecFlowCompatibility.3.3.x.nupkg -d test/`
- [ ] Verify structure:
  - [ ] `build/net462/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll` exists
  - [ ] `build/netstandard2.0/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll` exists
  - [ ] `build/Reqnroll.SpecFlowCompatibility.targets` exists
  - [ ] `lib/net462/Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll` exists
  - [ ] `lib/netstandard2.0/Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.dll` exists
- [ ] Create test project referencing package
- [ ] Verify build succeeds without FileNotFoundException

---

## Additional Recommendations

### 1. Add Package Content Validation Tests

Create automated tests that extract and validate package structure:

```csharp
[Test]
public void Reqnroll_CustomPlugin_Should_Have_Lib_Folder()
{
    var packagePath = GetPackagePath("Reqnroll.CustomPlugin");
    using var package = new ZipArchive(File.OpenRead(packagePath));
    
    Assert.That(package.Entries.Any(e => e.FullName.StartsWith("lib/netstandard2.0/")), 
        "Package should have lib/netstandard2.0/ folder");
    Assert.That(package.Entries.Any(e => e.FullName == "lib/netstandard2.0/Reqnroll.CustomPlugin.dll"), 
        "Package should include Reqnroll.CustomPlugin.dll in lib folder");
}

[Test]
public void Reqnroll_SpecFlowCompatibility_Should_Have_Generator_Plugin_In_Build()
{
    var packagePath = GetPackagePath("Reqnroll.SpecFlowCompatibility");
    using var package = new ZipArchive(File.OpenRead(packagePath));
    
    Assert.That(package.Entries.Any(e => 
        e.FullName == "build/net462/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll"), 
        "Package should include generator plugin DLL in build/net462/");
    Assert.That(package.Entries.Any(e => 
        e.FullName == "build/netstandard2.0/Reqnroll.SpecFlowCompatibility.Generator.ReqnrollPlugin.dll"), 
        "Package should include generator plugin DLL in build/netstandard2.0/");
}
```

### 2. Update CI/CD Pipeline

Add package validation step after build:
```yaml
- name: Validate Packages
  run: |
    dotnet tool install --global Meziantou.Framework.NuGetPackageValidation.Tool
    for pkg in $(find . -name "*.nupkg"); do
      meziantou.validate-nuget-package "$pkg"
    done
```

### 3. Documentation Updates

Update plugin development documentation to reflect correct usage:
- Document that CustomPlugin is a library package (not a generator plugin)
- Provide examples of creating custom generator plugins
- Clarify the difference between generator plugins and library packages

---

## Files Changed Summary

| File | Change Type | Priority |
|------|-------------|----------|
| `/Plugins/Reqnroll.CustomPlugin/Reqnroll.CustomPlugin.csproj` | Modify structure | HIGH |
| `/Plugins/Reqnroll.Plugins.TestFrameworkAdapter.targets` | Enhance target | HIGH |
| `/Tests/PackageValidation/` (new) | Add validation tests | MEDIUM |
| `/docs/extend/plugins.md` | Update documentation | LOW |

---

*Fixes proposed: 2025-12-19*  
*Based on analysis of Reqnroll 3.3.0 packaging issues*
