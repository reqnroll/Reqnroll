# Plugins

Reqnroll supports the following types of plugins:

* Runtime
* Generator

All types of plugins are created in a similar way.

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

* `RegisterGlobalDependencies` - registers a new interface in the global container, see [Available Containers & Registrations](available-containers.md#global-container)
* `CustomizeGlobalDependencies` - overrides registrations in the global container, see [Available Containers & Registrations](available-containers.md#global-container)
* `ConfigurationDefaults` - adjust configuration values
* `CustomizeTestThreadDependencies` - overrides or registers a new interface in the test thread container, see [Available Containers & Registrations](available-containers.md#test-thread-container)
* `CustomizeFeatureDependencies` - overrides or registers a new interface in the feature container, see [Available Containers & Registrations](available-containers.md#feature-container)
* `CustomizeScenarioDependencies` - overrides or registers a new interface in the scenario container, see [Available Containers & Registrations](available-containers.md#scenario-container)

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

## Combined Package with both plugins

If you need to update generator and runtime plugins with a single NuGet package (as we are doing with the `Reqnroll.xUnit`, `Reqnroll.NUnit` and `Reqnroll.xUnit` packages), you can do so.

As with the separate plugins, you need two projects. One for the runtime plugin, and one for the generator plugin. As you only want one NuGet package, the **NuSpec files must only be present in the generator project**.
This is because the generator plugin is built with a higher .NET Framework version (.NET 4.7.1), meaning you can add a dependency on the Runtime plugin (which is only .NET 4.6.1). This will not working the other way around.

You can simply combine the contents of the `.targets` and `.props` file to a single one.


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
