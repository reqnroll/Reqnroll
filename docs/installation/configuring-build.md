# Configuring Build
The main use case of Reqnroll involves adding one of the Reqnroll unit test NuGet packages to the test project in question, e.g.:
- `Reqnroll.MSTest`
- `Reqnroll.NUnit`
- `Reqnroll.TUnit`
- `Reqnroll.xUnit`

All of them depend on the package `Reqnroll.Tools.MsBuild.Generation`, including which modifies the project's build process. The obvious effect is generation of the *.cs files from the respective *.feature files and inclusion of the former in the compilation.

## Embedding feature files as resource
The `Reqnroll.Tools.MsBuild.Generation` package is essentially identical to its previous incarnation - `SpecFlow.Tools.MsBuild.Generation`. However, there is one original behavior which is now opt-in instead of unconditional - embedding the *.feature files as resources in the generated assembly. This embedding can be enabled with the dedicated build variable - `ReqnrollEmbedFeatureFiles`.

Suppose we have a test project using Reqnroll:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    ...
  </PropertyGroup>
  ...
</Project>
```
One way to set the `ReqnrollEmbedFeatureFiles` build variable is by adding `<ReqnrollEmbedFeatureFiles>true</ReqnrollEmbedFeatureFiles>` to the project file:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ReqnrollEmbedFeatureFiles>true</ReqnrollEmbedFeatureFiles>
    ...
  </PropertyGroup>
  ...
</Project>
```
If many test projects are involved and you want to enable this behavior for all of them, a convenient way to do so would be creating a file `Directory.Build.props` at the parent level such that all the projects are below it:
```xml
<Project>
  <PropertyGroup>
    <ReqnrollEmbedFeatureFiles>true</ReqnrollEmbedFeatureFiles>
    <UpstreamDirectoryBuildProps>$([MSBuild]::GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)\..\'))</UpstreamDirectoryBuildProps>
  </PropertyGroup>
  <Import Project="$(UpstreamDirectoryBuildProps)" Condition="$(UpstreamDirectoryBuildProps) != ''" />
</Project>
```
The `Import` statement is there to make sure any `Directory.Build.props` files at the higher levels (if any) are not cut off.

You can find more details about build customization at https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022

You are welcome to consult Microsoft documentation (or your favorite AI) for other ways to pass build variables to the build.

## Code-behind file location

By default, the generated code-behind (`*.feature.cs`) files are placed next to the feature files in the project folder. From v3.3 you can configure them to be generated to the intermediate output folder (e.g. `obj/Debug/net8.0`) by setting the `ReqnrollUseIntermediateOutputPathForCodeBehind` MSBuild property to `true`. According to the plans, this will be the default behavior from v4.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ReqnrollUseIntermediateOutputPathForCodeBehind>true</ReqnrollUseIntermediateOutputPathForCodeBehind>
    ...
  </PropertyGroup>
  ...
</Project>
```

Placing the generated files in the intermediate output folder makes it easier to exclude them from source control. Support for linked feature files (files referenced from outside the project folder) also requires this setting.

## Obsolete code-behind file handling

When feature files are renamed or deleted, old `*.feature.cs` code-behind files can be left behind. Reqnroll provides two properties to control how these obsolete files are handled:

- `ReqnrollWarnForObsoleteCodeBehindFiles` (default: `true`) — emits a build warning for each `*.feature.cs` file that no longer has a corresponding feature file.
- `ReqnrollDeleteObsoleteCodeBehindFilesOnClean` (default: `false`) — when set to `true`, obsolete `*.feature.cs` files are automatically deleted during a `Clean` or `Rebuild` operation.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ReqnrollDeleteObsoleteCodeBehindFilesOnClean>true</ReqnrollDeleteObsoleteCodeBehindFilesOnClean>
    ...
  </PropertyGroup>
  ...
</Project>
```

## All available MSBuild properties

All available MSBuild properties defined by `Reqnroll.Tools.MsBuild.Generation` can be found in the [`Reqnroll.Tools.MsBuild.Generation.props`](https://github.com/reqnroll/Reqnroll/blob/main/Reqnroll.Tools.MsBuild.Generation/build/Reqnroll.Tools.MsBuild.Generation.props) file in the Reqnroll repository, where each property is documented with an inline comment.