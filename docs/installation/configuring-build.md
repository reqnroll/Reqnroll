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

## Task factory used to run the code generation task
The code generation performed by `Reqnroll.Tools.MsBuild.Generation` runs as an MSBuild task, and MSBuild loads the task assembly using a *task factory*. Which task factory is used can be controlled with the `ReqnrollGenerationTaskFactory` build variable. By default:
- On Windows, `TaskHostFactory` is used. This runs the task in a separate process, which prevents Visual Studio from locking the task assembly file (e.g. during a build while the IDE is open) and avoids problems with nested or parallel MSBuild invocations (e.g. builds that themselves invoke MSBuild, or multiple projects building in parallel) trying to load the same assembly into the same process at the same time. See the [MSBuild documentation on task factories](https://learn.microsoft.com/en-us/visualstudio/msbuild/how-to-configure-targets-and-tasks?view=vs-2022#task-factories) for more details.
- On other operating systems (e.g. Linux, macOS), `AssemblyTaskFactory` is used instead, since `TaskHostFactory` is not supported on all platforms (see [#152](https://github.com/reqnroll/Reqnroll/issues/152)). `AssemblyTaskFactory` loads the task assembly directly into the MSBuild process.

You can override this default by setting `ReqnrollGenerationTaskFactory` explicitly, e.g. to force the use of `AssemblyTaskFactory` on Windows too:
```xml
<PropertyGroup>
  <ReqnrollGenerationTaskFactory>AssemblyTaskFactory</ReqnrollGenerationTaskFactory>
</PropertyGroup>
```
This can be useful if you are experiencing issues with `TaskHostFactory` (e.g. it not being available or behaving unexpectedly) and want to fall back to loading the task assembly directly in the MSBuild process instead.