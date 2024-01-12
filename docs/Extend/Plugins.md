# Plugins

Reqnroll supports the following types of plugins:

* Runtime
* Generator

All types of plugins are created in a similar way.

This information only applies to SpecFlow 3. For legacy information on plugins for previous versions, see [Plugins (Legacy)](https://docs.reqnroll.net/projects/reqnroll/en/latest/legacy/plugins-(Legacy).html). With SpecFlow 3, we have changed how you configure which plugins are used. They are no longer configured in your `app.config` (or `reqnroll.json`).

## Runtime plugins

Runtime plugins need to target .NET Framework 4.6.2 and .NET Standard 2.0.
Reqnroll searches for files that end with `.ReqnrollPlugin.dll` in the following locations:

* The folder containing your `Reqnroll.dll` file
* Your working directory

Reqnroll loads plugins in the order they are found in the folder.

### Create a runtime plugin

You can create your `RuntimePlugin` in a separate project, or in the same project where your tests are.

Optional:

1. Create a new class library for your plugin.

Mandatory:

1. Add the Reqnroll NuGet package to your project.
1. Define a class that implements the `IRuntimePlugin` interface (defined in Reqnroll.Plugins).
1. Flag your assembly with the `RuntimePlugin` attribute for the plugin to be identified by Reqnroll plugin loader. The following example demonstrates a `MyNewPlugin` class that implements the `IRuntimePlugin` interface:  
  `[assembly: RuntimePlugin(typeof(MyNewPlugin))]`
1. Implement the `Initialize` method of the `IRuntimePlugin` interface to access the `RuntimePluginEvents` and `RuntimePluginParameters`.

### RuntimePluginsEvents

* `RegisterGlobalDependencies` - registers a new interface in the global container, see [Available Containers & Registrations](Available-Containers-&-Registrations.html#global-container)
* `CustomizeGlobalDependencies` - overrides registrations in the global container, see [Available Containers & Registrations](Available-Containers-&-Registrations.html#global-container)
* `ConfigurationDefaults` - adjust configuration values
* `CustomizeTestThreadDependencies` - overrides or registers a new interface in the test thread container, see [Available Containers & Registrations](Available-Containers-&-Registrations.html#test-thread-container-parent-container-is-the-global-container)
* `CustomizeFeatureDependencies` - overrides or registers a new interface in the feature container, see [Available Containers & Registrations](Available-Containers-&-Registrations.html#feature-container-parent-container-is-the-test-thread-container)
* `CustomizeScenarioDependencies` - overrides or registers a new interface in the scenario container, see [Available Containers & Registrations](Available-Containers-&-Registrations.html#scenario-container-parent-container-is-the-test-thread-container)

### Sample runtime plugin

A complete example of a Runtime plugin can be found [here](https://github.com/reqnroll/Reqnroll-Examples/tree/master/Plugins/RuntimeOnlyPlugin). It packages a Runtime plugin as a NuGet package.

#### SampleRuntimePlugin.csproj

The sample project is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/RuntimeOnlyPlugin/RuntimePlugin/SampleRuntimePlugin.csproj).

This project targets multiple frameworks, so the project file uses `<TargetFrameworks>` instead of `<TargetFramework>`. Our target frameworks are .NET 4.6.2 and .NET Standard 2.0.

``` xml
<TargetFrameworks>net461;netstandard2.0</TargetFrameworks>
```

We set a different `<AssemblyName>` to add the required `.ReqnrollPlugin` suffix to the assembly name. You can also simply name your project with  `.ReqnrollPlugin` at the end.

``` xml
<AssemblyName>SampleRuntimePlugin.ReqnrollPlugin</AssemblyName>
```

`<GeneratePackageOnBuild>` is set to true so that the NuGet package is generated on build.
We use a NuSpec file (SamplePlugin.nuspec) to provide all information for the NuGet package. This is set with the `<NuspecFile>` property.

``` xml
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
<NuspecFile>$(MSBuildThisFileDirectory)SamplePlugin.nuspec</NuspecFile>
```

Runtime plugins only need a reference to the `Reqnroll` NuGet package.

``` xml
<ItemGroup>
    <PackageReference Include="Reqnroll" Version="3.0.199" />
</ItemGroup>
```

#### build/Reqnroll.SamplePlugin.targets

The sample targets file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/RuntimeOnlyPlugin/RuntimePlugin/build/Reqnroll.SamplePlugin.targets).

We need to copy a different assembly to the output folder depending on the target framework (.NET Core vs .NET Framework) of the project using the runtime plugin package. Because the `$(TargetFrameworkIdentifier)` property is only available in imported `targets` files, we have to work out the path to the assembly here.

``` xml
<_SampleRuntimePluginFramework Condition=" '$(TargetFrameworkIdentifier)' == '.NETCoreApp' ">netstandard2.0</_SampleRuntimePluginFramework>
<_SampleRuntimePluginFramework Condition=" '$(TargetFrameworkIdentifier)' == '.NETFramework' ">net461</_SampleRuntimePluginFramework>
<_SampleRuntimePluginPath>$(MSBuildThisFileDirectory)\..\lib\$(_SampleRuntimePluginFramework)\SampleRuntimePlugin.ReqnrollPlugin.dll</_SampleRuntimePluginPath>
```

#### build/Reqnroll.SamplePlugin.props

The sample props file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/RuntimeOnlyPlugin/RuntimePlugin/build/Reqnroll.SamplePlugin.props).

To copy the plugin assembly to the output folder, we include it in the `None` ItemGroup and set `CopyToOutputDirectory` to `PreserveNewest`. This ensures that it is still copied to the output directory even if you change it.

``` xml
<ItemGroup>
    <None Include="$(_SampleRuntimePluginPath)" >
        <Link>%(Filename)%(Extension)</Link>
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        <Visible>False</Visible>
    </None>
</ItemGroup>
```

#### Directory.Build.targets

The sample file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/GeneratorOnlyPlugin/Directory.Build.targets).

To specify the path to the assemblies which should be included in the NuGet package, we have to set various NuSpec properties.
This is done in the `Directory.Build.targets`, so it defined for all projects in subfolders. We add the value of the current configuration to the available properties.

``` xml
<Target Name="SetNuspecProperties" BeforeTargets="GenerateNuspec" >
    <PropertyGroup>
        <NuspecProperties>$(NuspecProperties);config=$(Configuration)</NuspecProperties>
    </PropertyGroup>
</Target>
```

#### SamplePlugin.nuspec

The sample file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/RuntimeOnlyPlugin/RuntimePlugin/SamplePlugin.nuspec).

This is the NuSpec file that provides information on the NuGet package. To get the files from the build directory into the NuGet package, we have to specify them in the `file` list.
The runtime plugin assemblies are also specified here, using the additional `$config$` property we added in the `Directory.Build.targets`.

``` xml
    <files>
        <file src="build\**\*" target="build"/>
        <file src="bin\$config$\net461\SampleRuntimePlugin.ReqnrollPlugin.*" target="lib\net461"/>
        <file src="bin\$config$\netstandard2.0\SampleRuntimePlugin.ReqnrollPlugin.dll" target="lib\netstandard2.0"/>
        <file src="bin\$config$\netstandard2.0\SampleRuntimePlugin.ReqnrollPlugin.pdb" target="lib\netstandard2.0"/>
    </files>
```

## Generator plugins

Generator plugins need to target .NET Framework 4.7.1 and .NET Core 3.1.
The MSBuild task needs to know which generator plugins it should use. You therefore have to add your generator plugin to the `ReqnrollGeneratorPlugins` ItemGroup.
This is passed to the MSBuild task as a parameter and later used to load the plugins.

### Create a generator plugin

1. Create a new class library for your plugin.
1. Add the Reqnroll.CustomPlugin NuGet package to your project.
1. Define a class that implements the `IGeneratorPlugin` interface (defined in Reqnroll.Generator.Plugins namespace).
1. Flag your assembly with the `GeneratorPlugin` attribute for the plugin to be identified by Reqnroll plugin loader. The following example demonstrates a `MyNewPlugin` class that implements the `IGeneratorPlugin` interface:  
  `[assembly: GeneratorPlugin(typeof(MyNewPlugin))]`
1. Implement the `Initialize` method of the `IGeneratorPlugin` interface to access `GeneratorPluginEvents` and `GeneratorPluginParameters` parameters.

### GeneratorPluginsEvents

* `RegisterDependencies` - registers a new interface in the Generator container
* `CustomizeDependencies` - overrides registrations in the Generator container
* `ConfigurationDefaults` - adjust configuration values

### Sample generator plugin

A complete example of a generator plugin can be found [here](https://github.com/reqnroll/Reqnroll-Examples/tree/master/Plugins/GeneratorOnlyPlugin). It packages a Generator plugin into a NuGet package.

#### SampleGeneratorPlugin.csproj

The sample project is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/GeneratorOnlyPlugin/GeneratorPlugin/SampleGeneratorPlugin.csproj).

The project targets multiple frameworks, so it uses `<TargetFrameworks>` and not `<TargetFramework>`.
We set a different `<AssemblyName>` to add the required `.ReqnrollPlugin` at the end. You can also name your project with  `.ReqnrollPlugin` at the end.
`<GeneratePackageOnBuild>` is set to true, so that the NuGet package is generated on build.
We use a NuSpec file (SamplePlugin.nuspec) to provide all information for the NuGet package. This is set with the `<NuspecFile>` property.

``` xml
<PropertyGroup>
    <TargetFrameworks>net471;netcoreapp3.1</TargetFrameworks>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <NuspecFile>$(MSBuildThisFileDirectory)SamplePlugin.nuspec</NuspecFile>
    <AssemblyName>SampleGeneratorPlugin.ReqnrollPlugin</AssemblyName>
</PropertyGroup>
```

For a generator plugin you  need a reference to the `Reqnroll.CustomPlugin`- NuGet package.

``` xml
<ItemGroup>
    <PackageReference Include="Reqnroll.CustomPlugin" Version="3.0.199" />
</ItemGroup>
```

#### build/Reqnroll.SamplePlugin.targets

The sample targets file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/GeneratorOnlyPlugin/GeneratorPlugin/build/Reqnroll.SamplePlugin.targets).

We have to add a different assembly to the ItemGroup depending on the MSBuild version in use (.NET Full Framework or .NET Core).
Because the `$(MSBuildRuntimeType)` property is only available in imported `target` files, we have to work out the path to the assembly here.

``` xml
<PropertyGroup>
    <_SampleGeneratorPluginFramework Condition=" '$(MSBuildRuntimeType)' == 'Core'">netecoreapp3.1</_SampleGeneratorPluginFramework>
    <_SampleGeneratorPluginFramework Condition=" '$(MSBuildRuntimeType)' != 'Core'">net471</_SampleGeneratorPluginFramework>
    <_SampleGeneratorPluginPath>$(MSBuildThisFileDirectory)\$(_SampleGeneratorPluginFramework)\SampleGeneratorPlugin.ReqnrollPlugin.dll</_SampleGeneratorPluginPath>
</PropertyGroup>
```

#### build/Reqnroll.SamplePlugin.props

The sample props file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/GeneratorOnlyPlugin/GeneratorPlugin/build/Reqnroll.SamplePlugin.props).

As we have now a property containing the path to the assembly, we can add it to the `ReqnrollGeneratorPlugins` ItemGroup here.

``` xml
<ItemGroup>
    <ReqnrollGeneratorPlugins Include="$(_SampleGeneratorPluginPath)" />
</ItemGroup>
```

#### Directory.Build.targets

The sample file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/GeneratorOnlyPlugin/Directory.Build.targets).

To specify the path to the assemblies which should be included in the NuGet package, we have to set various NuSpec properties.
This is done in `Directory.Build.targets`, so it defined for all projects in subfolders. We add the value of the current configuration to the available properties.

``` xml
<Target Name="SetNuspecProperties" BeforeTargets="GenerateNuspec" >
    <PropertyGroup>
        <NuspecProperties>$(NuspecProperties);config=$(Configuration)</NuspecProperties>
    </PropertyGroup>
</Target>
```

#### SamplePlugin.nuspec

The sample file is [here](https://github.com/reqnroll/Reqnroll-Examples/blob/master/Plugins/GeneratorOnlyPlugin/GeneratorPlugin/SamplePlugin.nuspec).

This is the NuSpec file that provides information for the NuGet package. To get the files from the build directory into the NuGet package, we have to specify them in the `file` list.
The generator plugin assemblies are also specified here, using the additional `$config$` property we added in the `Directory.Build.targets`.
It is important to ensure that they are not added to the `lib` folder. If this were the case, they would be referenced by the project where you add the NuGet package. This is something we don't want to happen!

``` xml
<files>
    <file src="build\**\*" target="build"/>
    <file src="bin\$config$\net471\SampleGeneratorPlugin.ReqnrollPlugin.*" target="build\net471"/>
    <file src="bin\$config$\netcoreapp3.1\SampleGeneratorPlugin.ReqnrollPlugin.dll" target="build\netcoreapp3.1"/>
    <file src="bin\$config$\netcoreapp3.1\SampleGeneratorPlugin.ReqnrollPlugin.pdb" target="build\netcoreapp3.1"/>
</files>
```

## Combined Package with both plugins

If you need to update generator and runtime plugins with a single NuGet package (as we are doing with the `Reqnroll.xUnit`, `Reqnroll.NUnit` and `Reqnroll.xUnit` packages), you can do so.

As with the separate plugins, you need two projects. One for the runtime plugin, and one for the generator plugin. As you only want one NuGet package, the **NuSpec files must only be present in the generator project**.
This is because the generator plugin is built with a higher .NET Framework version (.NET 4.7.1), meaning you can add a dependency on the Runtime plugin (which is only .NET 4.6.1). This will not working the other way around.

You can simply combine the contents of the `.targets` and `.props` file to a single one.

### Example

A complete example of a NuGet package that contains a runtime and generator plugin can be found [here](https://github.com/reqnroll/Reqnroll-Examples/tree/master/Plugins/CombinedPlugin).


## Tips & Tricks

### Building Plugins on non-Windows machines

For building .NET 4.6.2 projects on non- Windows machines, the .NET Framework reference assemblies are needed.

You can add them with following PackageReference to your project:

``` xml
<ItemGroup>
    <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
</ItemGroup>

```

## Plugin Developer Channel

We have set up a Discord channel for plugin developers [here](https://discord.com/invite/xQMrjDXx7a). If you have any questions regarding the development of plugins for Reqnroll, this is the place to ask them.
