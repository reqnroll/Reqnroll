# Plugins

Reqnroll supports the following types of plugins:

* Runtime
* Generator

```{note}
The versioning and compatibility of the Reqnroll plugins is described in detail in the [Compatibility page](../installation/compatibility.md#versioning-policy).
```

All types of plugins are created in a similar way.

## Runtime plugins

Runtime plugins should target .NET Standard 2.0 to be compatible with all .NET versions supported 
by Reqnroll. Targetting a more specific version will limit the compatibility of your plugin.

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

Runtime plugins should target .NET Standard 2.0 to be compatible with all scenarios supported by 
Reqnroll. 

The generator plugins are invoked during build. They are usually invoked in a .NET environment 
according to your .NET SDK (e.g. .NET 8.0), but in some cases (when built using MSBuild or in Visual Studio) 
they might be invoked in a .NET 4.8 environment. Therefore, you have to make sure that your plugin
works in both environments. If necessary, you can multi-target your plugin, but using the right compiled
version of your plugin is the responsibility of the plugin itself.

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

### Third-Party Dependencies

If your plugin uses third party assemblies, you need to make sure that the dependencies can be found.

Reqnroll will attempt to find your plugin's dependencies by

1. Loading assemblies from the same directory as your assembly.
2. Loading your plugin's .deps.json and loading the specified runtime assembly (e.g. from NuGet cache).

   Note: If you are using Nuget to publish your plugin, make sure your NuGet package contains the correct dependencies, otherwise the NuGet cache may be empty.

3. Check if the assembly is provided by Reqnroll itself (e.g. System.CodeDom).

   Note: The assemblies included by Reqnroll can change between versions, e.g. we switched to System.Text.Json at some point.
   So if you want to be on the safe side, do not rely on this approach.

## Combined Package with both plugins

You can have a single NuGet package that contains both the runtime and generator plugins. 
We use this approach for the `Reqnroll.xUnit`, `Reqnroll.NUnit`, `Reqnroll.TUnit` and `Reqnroll.xUnit` packages
for example.

The combined package can be built from a single project, or from two projects. The latter 
allows you to have different dependencies for the runtime and generator plugins.

If you use two projects for the combined package, the **NuSpec files should only be present 
in the generator project**. This is because the generators typically have more dependencies.

You can simply combine the contents of the `.targets` and `.props` file to a single one.

## Tips & Tricks

### Building Plugins on non-Windows machines

For building .NET 4.6.2 projects on non-Windows machines, the .NET Framework reference assemblies are needed.

You can add them with following PackageReference to your project:

```xml
<ItemGroup>
    <PackageReference Include="Microsoft.NETFramework.ReferenceAssemblies" Version="1.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
</ItemGroup>
```
