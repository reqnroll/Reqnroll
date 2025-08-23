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