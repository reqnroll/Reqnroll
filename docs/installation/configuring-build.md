# Configuring Build

The main use case of Reqnroll involves adding one of the Reqnroll unit test NuGet packages to the test project in question, e.g.:
- `Reqnroll.MSTest`
- `Reqnroll.NUnit`
- `Reqnroll.TUnit`
- `Reqnroll.xUnit`

All of them depend on the package `Reqnroll.Tools.MsBuild.Generation`, which modifies the project's build process. The most visible effect is the generation of `*.feature.cs` code-behind files from the `*.feature` files and their inclusion in the compilation.

The build integration can be customized using MSBuild properties. These properties can be set in the project file, in a `Directory.Build.props` file, or passed on the command line.

## Setting MSBuild properties

One way to set a Reqnroll MSBuild property is by adding it to the `<PropertyGroup>` in your project file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ReqnrollUseIntermediateOutputPathForCodeBehind>true</ReqnrollUseIntermediateOutputPathForCodeBehind>
  </PropertyGroup>
</Project>
```

If many test projects are involved and you want to apply a setting to all of them, a convenient way to do so is to create a `Directory.Build.props` file at a common parent level:

```xml
<Project>
  <PropertyGroup>
    <ReqnrollUseIntermediateOutputPathForCodeBehind>true</ReqnrollUseIntermediateOutputPathForCodeBehind>
    <UpstreamDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)\..\'))</UpstreamDirectoryBuildProps>
  </PropertyGroup>
  <Import Project="$(UpstreamDirectoryBuildProps)" Condition="$(UpstreamDirectoryBuildProps) != ''" />
</Project>
```

The `Import` statement ensures that any `Directory.Build.props` files at higher levels (if any) are not cut off.

You can find more details about build customization at https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022

{#msbuild-property-reference}
## MSBuild property reference

The following MSBuild properties are provided by the `Reqnroll.Tools.MsBuild.Generation` package.

{#ReqnrollUseIntermediateOutputPathForCodeBehind}
### `ReqnrollUseIntermediateOutputPathForCodeBehind`

When set to `true`, the generated code-behind (`*.feature.cs`) files are placed in the intermediate output folder (e.g. `obj/Debug/net9.0`) instead of next to the feature files in the project folder. This makes it easier to exclude the generated files from source control.

```{note}
Support for linked feature files (files referenced from outside the project folder) also requires this setting to be enabled. See [](setup-project) for more details.
```

```{list-table}
:header-rows: 1

* - Property
  - Default
  - Description
* - `ReqnrollUseIntermediateOutputPathForCodeBehind`
  - `false`
  - Places generated `*.feature.cs` files in the intermediate output folder (`obj/...`) instead of next to the feature files. Will become the default in v4.
```

Example:

```xml
<PropertyGroup>
  <ReqnrollUseIntermediateOutputPathForCodeBehind>true</ReqnrollUseIntermediateOutputPathForCodeBehind>
</PropertyGroup>
```

{#ReqnrollEmbedFeatureFiles}
### `ReqnrollEmbedFeatureFiles`

Controls whether the `*.feature` files are embedded as resources in the generated assembly. This was unconditional behavior in SpecFlow but is opt-in in Reqnroll.

```{list-table}
:header-rows: 1

* - Property
  - Default
  - Description
* - `ReqnrollEmbedFeatureFiles`
  - `false`
  - When `true`, embeds the `*.feature` files as resources in the compiled assembly.
```

Example:

```xml
<PropertyGroup>
  <ReqnrollEmbedFeatureFiles>true</ReqnrollEmbedFeatureFiles>
</PropertyGroup>
```

{#obsolete-code-behind-file-handling}
### Obsolete code-behind file handling

When feature files are renamed or deleted (or when such changes are pulled from source control), old `*.feature.cs` code-behind files can be left behind. Reqnroll provides two properties to control how these obsolete files are handled.

```{list-table}
:header-rows: 1

* - Property
  - Default
  - Description
* - `ReqnrollWarnForObsoleteCodeBehindFiles`
  - `true`
  - When `true`, Reqnroll emits a build warning for each `*.feature.cs` file that no longer has a corresponding feature file.
* - `ReqnrollDeleteObsoleteCodeBehindFilesOnClean`
  - `false`
  - When `true`, obsolete `*.feature.cs` files (those without a corresponding feature file) are automatically deleted during a `Clean` or `Rebuild` operation.
```

Example — automatically clean up obsolete files:

```xml
<PropertyGroup>
  <ReqnrollDeleteObsoleteCodeBehindFilesOnClean>true</ReqnrollDeleteObsoleteCodeBehindFilesOnClean>
</PropertyGroup>
```

{#advanced-properties}
### Advanced properties

The following properties are intended for advanced or diagnostic scenarios and are unlikely to be needed in typical projects.

```{list-table}
:header-rows: 1

* - Property
  - Default
  - Description
* - `ReqnrollDebugMSBuildTask`
  - `false`
  - When `true`, causes the MSBuild generation task to launch a debugger on startup. Useful when diagnosing code-behind generation issues.
* - `ReqnrollGenerationTaskFactory`
  - `TaskHostFactory` (Windows) / `AssemblyTaskFactory` (other)
  - Controls the MSBuild task factory used to load the generation task. `TaskHostFactory` prevents the task assembly from being locked by Visual Studio on Windows. Change only if you encounter compatibility issues.
* - `ReqnrollGenerationTasksPath`
  - *(set automatically)*
  - Path to the directory containing the Reqnroll generation task assemblies. Override only if you have a custom layout.
* - `ReqnrollGenerationTasksAssemblyFilename`
  - `Reqnroll.Tools.MsBuild.Generation.dll`
  - Filename of the Reqnroll generation task assembly.
* - `ReqnrollGenerationTasksAssemblyPath`
  - *(set automatically)*
  - Full path to the Reqnroll generation task assembly. Derived from `ReqnrollGenerationTasksPath` and `ReqnrollGenerationTasksAssemblyFilename` if not set explicitly.
```