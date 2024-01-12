# Generate Tests From MsBuild

## General

You need to use the MSBuild code behind file generation for SpecFlow 3.0.  

After version SpecFlow 3.3.30 don't need to add the `Reqnroll.Tools.MSBuild.Generation` package anymore to your project, if you are using one of our [Unit-Test-Provider](../Installation/Unit-Test-Providers.md) NuGet packages.

**Note:** You will need at least VS2017/MSBuild 15 to use this package.

### Configuration

1. Add the NuGet package `Reqnroll.Tools.MsBuild.Generation` with the same version as Reqnroll to your project.
2. Remove all `ReqnrollSingleFileGenerator` custom tool entries from your feature files.<br><img src=http://www.reqnroll.net/screenshots/CustomTool.png>
3. Select <b>Tools | Options | Reqnroll</b> from the menu in Visual Studio, and set <b>Enable ReqnrollSingleFileGenerator CustomTool</b> to "false".

### SDK Style project system

<b>Please use the SpecFlow 2.4.1 NuGet package or higher, as this version fixes an issue with previous versions (see *Known Issues* below)</b>
<!--
1. Add the NuGet package `Reqnroll.Tools.MsBuild.Generation` with the same version as Reqnroll to your project.
2. Remove all `ReqnrollSingleFileGenerator` custom tool entries from your feature files.<br><img src=http://www.reqnroll.net/screenshots/CustomTool.png>
-->

## Additional Legacy Options (Prior to SpecFlow 3)

The `Reqnroll.targets` file defines a number of default options in the following section:

```xml
<PropertyGroup>
    <ShowTrace Condition="'$(ShowTrace)'==''">false</ShowTrace>
    <OverwriteReadOnlyFiles Condition="'$(OverwriteReadOnlyFiles)'==''">false</OverwriteReadOnlyFiles>
    <ForceGeneration Condition="'$(ForceGeneration)'==''">false</ForceGeneration>
    <VerboseOutput Condition="'$(VerboseOutput)'==''">false</VerboseOutput>
</PropertyGroup>
```

* `ShowTrace`: Set this to true to output trace information.
* `OverwriteReadOnlyFiles`: Set this to true to overwrite any read-only files in the target directory. This can be useful if your feature files are read-only and part of your repository.
* `ForceGeneration`: Set this to true to forces the code-behind files to be regenerated, even if the content of the feature has not changed. 
* `VerboseOutput`: Set to true to enable verbose output for troubleshooting.

To change these options, add the corresponding element to your project file **before** the `<Import>` element you added earlier.

**Example:**

```xml
<PropertyGroup>
  <ShowTrace>true</ShowTrace>
  <VerboseOutput>true</VerboseOutput>
</PropertyGroup>
...
</ItemGroup>
<Import Project="$(MSBuildBinPath)\Microsoft.CSharp.targets" />
<Import Project="..\packages\Reqnroll.2.2.0\tools\Reqnroll.tasks"  Condition="Exists('..\packages\Reqnroll.2.2.0\tools\Reqnroll.tasks')" />
<Import Project="..\packages\Reqnroll.2.2.0\tools\Reqnroll.targets" Condition="Exists('..\packages\Reqnroll.2.2.0\tools\Reqnroll.targets')" />
...
</Project>
```

## Known Issues

### Reqnroll prior to 2.4.1

When using Reqnroll NuGet packages prior to SpecFlow 2.4.1, Visual Studio sometimes does not recognize that a feature file has changed. To generate the code-behind file, you therefore need to rebuild your project. We recommend upgrading your Reqnroll NuGet package to 2.4.1 or higher, where this is no longer an issue.

### Code-behind files not generating at compile time

When using the classic project system, the previous MSBuild target may no longer be located at the end of your project. NuGet ignores entries added manually. NuGet places the MSBuild imports at the end.  However, the `AfterUpdateFeatureFilesInProject` target needs to be defined after the imports. Otherwise it will be overwritten with an empty definition. If this happens, your code-behind files are not compiled as part of the assembly.

### Linked files are not included

If you link feature files into a project, no code-behind file is generated for them (see GitHub Issue [1295](https://github.com/reqnroll/Reqnroll/issues/1295)).
