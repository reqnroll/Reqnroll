# Build Metadata Provider

This section covers the details of customizing Reqnroll's build metadata functionality for custom source control management (SCM) and build systems.


## Overview

Reqnroll provides built-in support for extracting build metadata from various CI/CD environments including Azure Pipelines, TeamCity, Jenkins, GitHub Actions, GitLab CI, and many others. This metadata is captured through the `IBuildMetadataProvider` interface and stored in a `BuildMetadata` record.

When working with custom or unsupported build systems, you can create a runtime plugin that provides your own implementation of `IBuildMetadataProvider` to extract the relevant build information for your specific environment.

## BuildMetadata Record Properties {#build-metadata-record}

The `BuildMetadata` record contains the following properties that capture essential build and source control information:

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| `BuildUrl` | `string` | The URL to the build in the CI/CD system (e.g., link to build results page) |
| `BuildNumber` | `string` | The unique identifier or number assigned to the build by the CI/CD system |
| `Remote` | `string` | The URL of the source control repository (e.g., Git remote URL) |
| `Revision` | `string` | The specific commit hash, revision, or changeset identifier |
| `Branch` | `string` | The source control branch name from which the build was triggered |
| `Tag` | `string` | The source control tag name if the build was triggered from a tagged commit |
| `ProductName` | `string` | The name of the build server or CI/CD product (set automatically by the provider) |

### Property Details

**BuildUrl**: This should link directly to the build results page where developers can view build logs, artifacts, and status. Format varies by CI/CD system.

**BuildNumber**: Often an incrementing integer but can be any string format depending on your build system's numbering scheme.

**Remote**: Typically a Git repository URL, but can be any source control system URL. Should be in a format that allows cloning or accessing the repository.

**Revision**: For Git this would be the full commit SHA. For other SCM systems, use the equivalent unique identifier.

**Branch**: The branch name without any prefix (e.g., "main", "develop", "feature/new-functionality").

**Tag**: Only set if the build was triggered from a tagged commit. Should be the tag name without any prefix.

**ProductName**: Automatically set by the `GetBuildMetadata()` method to identify which build system provided the metadata.

## Creating a Custom Build Metadata Provider {#custom-build-metadata-provider}

To support a custom SCM or build system, you need to create a Reqnroll runtime plugin that implements the `IBuildMetadataProvider` interface.

### Step 1: Create the Plugin Project

Create a new .NET class library project that will contain your custom build metadata provider:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Reqnroll" Version="2.4.0" />
  </ItemGroup>
</Project>
```

**Note**: Target .NET Standard 2.0 for maximum compatibility with all .NET versions supported by Reqnroll.

### Step 2: Implement IBuildMetadataProvider

Create your custom implementation of the `IBuildMetadataProvider` interface:

```csharp
using Reqnroll.EnvironmentAccess;

namespace MyCompany.CustomScm.ReqnrollPlugin
{
    public class CustomBuildMetadataProvider : IBuildMetadataProvider
    {
        private readonly IEnvironmentWrapper _environment;

        public CustomBuildMetadataProvider(IEnvironmentWrapper environment)
        {
            _environment = environment;
        }

        public BuildMetadata GetBuildMetadata()
        {
            // Check if we're running in your custom build environment
            var customBuildId = GetEnvironmentVariable("CUSTOM_BUILD_ID");
            if (string.IsNullOrEmpty(customBuildId))
                return null; // Not running in our custom environment

            // Extract metadata from your custom environment variables
            var buildUrl = GetEnvironmentVariable("CUSTOM_BUILD_URL");
            var buildNumber = GetEnvironmentVariable("CUSTOM_BUILD_NUMBER");
            var remote = GetEnvironmentVariable("CUSTOM_SCM_REMOTE");
            var revision = GetEnvironmentVariable("CUSTOM_SCM_REVISION");
            var branch = GetEnvironmentVariable("CUSTOM_SCM_BRANCH");
            var tag = GetEnvironmentVariable("CUSTOM_SCM_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag)
            {
                ProductName = "Custom Build System"
            };
        }

        private string GetEnvironmentVariable(string variable)
        {
            var result = _environment.GetEnvironmentVariable(variable);
            return result is ISuccess<string> success ? success.Result : null;
        }
    }
}
```

### Step 3: Create the Runtime Plugin

Create a class that implements `IRuntimePlugin` to register your custom provider:

```csharp
using Reqnroll.BoDi;
using Reqnroll.Plugins;
using Reqnroll.EnvironmentAccess;

[assembly: RuntimePlugin(typeof(MyCompany.CustomScm.ReqnrollPlugin.CustomBuildMetadataPlugin))]

namespace MyCompany.CustomScm.ReqnrollPlugin
{
    public class CustomBuildMetadataPlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, 
                              RuntimePluginParameters runtimePluginParameters,
                              UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            // Register our custom implementation in the global container
            runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<CustomBuildMetadataProvider, IBuildMetadataProvider>();
            };
        }
    }
}
```

### Step 4: Build and Deploy the Plugin

1. **Build your plugin project** to generate the assembly
2. **Name the output assembly** with the suffix `.ReqnrollPlugin.dll` (e.g., `MyCompany.CustomScm.ReqnrollPlugin.dll`)
3. **Deploy the plugin** by copying it to one of these locations:
   - The folder containing your `Reqnroll.dll` file
   - Your working directory

Reqnroll automatically discovers and loads plugins with the `.ReqnrollPlugin.dll` naming convention.

### Step 5: Configure Environment Variables

Ensure your custom build system sets the appropriate environment variables that your provider expects:

```bash
# Example environment variables for your custom system
export CUSTOM_BUILD_ID="12345"
export CUSTOM_BUILD_URL="https://build.mycompany.com/builds/12345"
export CUSTOM_BUILD_NUMBER="1.2.3-build.12345"
export CUSTOM_SCM_REMOTE="https://scm.mycompany.com/repo/myproject.git"
export CUSTOM_SCM_REVISION="a1b2c3d4e5f6789012345678901234567890abcd"
export CUSTOM_SCM_BRANCH="main"
export CUSTOM_SCM_TAG=""  # Empty if not a tag build
```

## Advanced Scenarios

### Supporting Multiple Build Systems

If you need to support multiple custom build systems, you can create a composite provider:

```csharp
public class CompositeBuildMetadataProvider : IBuildMetadataProvider
{
    private readonly IEnvironmentWrapper _environment;
    private readonly IBuildMetadataProvider[] _providers;

    public CompositeBuildMetadataProvider(IEnvironmentWrapper environment)
    {
        _environment = environment;
        _providers = new IBuildMetadataProvider[]
        {
            new CustomSystemAProvider(environment),
            new CustomSystemBProvider(environment),
            new LegacySystemProvider(environment)
        };
    }

    public BuildMetadata GetBuildMetadata()
    {
        foreach (var provider in _providers)
        {
            var metadata = provider.GetBuildMetadata();
            if (metadata != null)
                return metadata;
        }
        
        return null; // No custom system detected
    }
}
```

### Fallback to Default Provider

To ensure compatibility with standard CI/CD systems while adding support for your custom system:

```csharp
public class CustomWithFallbackBuildMetadataProvider : IBuildMetadataProvider
{
    private readonly IBuildMetadataProvider _customProvider;
    private readonly IBuildMetadataProvider _defaultProvider;

    public CustomWithFallbackBuildMetadataProvider(
        IEnvironmentWrapper environment,
        IEnvironmentInfoProvider environmentInfoProvider)
    {
        _customProvider = new CustomBuildMetadataProvider(environment);
        _defaultProvider = new BuildMetadataProvider(environmentInfoProvider, environment);
    }

    public BuildMetadata GetBuildMetadata()
    {
        // Try custom provider first
        var customMetadata = _customProvider.GetBuildMetadata();
        if (customMetadata != null)
            return customMetadata;

        // Fallback to default provider for standard CI/CD systems
        return _defaultProvider.GetBuildMetadata();
    }
}
```

### Handling Complex Environment Detection

For sophisticated environment detection logic:

```csharp
public class SmartBuildMetadataProvider : IBuildMetadataProvider
{
    private readonly IEnvironmentWrapper _environment;

    public SmartBuildMetadataProvider(IEnvironmentWrapper environment)
    {
        _environment = environment;
    }

    public BuildMetadata GetBuildMetadata()
    {
        // Detect environment based on multiple indicators
        if (IsCustomSystemA())
            return GetCustomSystemAMetadata();
        
        if (IsCustomSystemB())
            return GetCustomSystemBMetadata();
            
        if (IsDockerEnvironment())
            return GetDockerEnvironmentMetadata();
            
        return null;
    }

    private bool IsCustomSystemA()
    {
        return !string.IsNullOrEmpty(GetVariable("CUSTOM_A_BUILD_ID")) &&
               !string.IsNullOrEmpty(GetVariable("CUSTOM_A_PROJECT"));
    }

    private bool IsCustomSystemB()
    {
        var buildTool = GetVariable("BUILD_TOOL");
        var buildId = GetVariable("BUILD_IDENTIFIER");
        return "CustomTool".Equals(buildTool, StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrEmpty(buildId);
    }

    private bool IsDockerEnvironment()
    {
        return File.Exists("/.dockerenv") ||
               !string.IsNullOrEmpty(GetVariable("DOCKER_CONTAINER_ID"));
    }

    private string GetVariable(string name)
    {
        var result = _environment.GetEnvironmentVariable(name);
        return result is ISuccess<string> success ? success.Result : null;
    }

    // Implementation methods for GetCustomSystemAMetadata(), etc.
}
```

## Testing Your Custom Provider

Create unit tests to verify your custom provider works correctly:

```csharp
[Test]
public void GetBuildMetadata_WithCustomEnvironmentVariables_ReturnsBuildMetadata()
{
    // Arrange
    var mockEnvironment = new Mock<IEnvironmentWrapper>();
    mockEnvironment.Setup(e => e.GetEnvironmentVariable("CUSTOM_BUILD_ID"))
               .Returns(new Success<string>("12345"));
    mockEnvironment.Setup(e => e.GetEnvironmentVariable("CUSTOM_BUILD_URL"))
               .Returns(new Success<string>("https://build.example.com/12345"));
    // ... setup other variables

    var provider = new CustomBuildMetadataProvider(mockEnvironment.Object);

    // Act
    var result = provider.GetBuildMetadata();

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.BuildNumber, Is.EqualTo("12345"));
    Assert.That(result.BuildUrl, Is.EqualTo("https://build.example.com/12345"));
    Assert.That(result.ProductName, Is.EqualTo("Custom Build System"));
}

[Test]
public void GetBuildMetadata_WithoutCustomEnvironment_ReturnsNull()
{
    // Arrange
    var mockEnvironment = new Mock<IEnvironmentWrapper>();
    mockEnvironment.Setup(e => e.GetEnvironmentVariable(It.IsAny<string>()))
               .Returns(new Success<string>(null));

    var provider = new CustomBuildMetadataProvider(mockEnvironment.Object);

    // Act
    var result = provider.GetBuildMetadata();

    // Assert
    Assert.That(result, Is.Null);
}
```

## Container Registration Details

The plugin system uses Reqnroll's built-in BoDi dependency injection container. The key points for container registration:

### Registration Methods

- **`RegisterTypeAs<TImplementation, TInterface>()`**: Registers a type to be instantiated when the interface is requested
- **`RegisterInstanceAs<TInterface>(instance)`**: Registers a pre-created instance
- **`RegisterFactoryAs<TInterface>(factory)`**: Registers a factory function

### Container Hierarchy

Reqnroll uses a hierarchical container system:

- **Global Container**: For global services (where you register `IBuildMetadataProvider`)
- **Test Thread Container**: Per test thread
- **Feature Container**: Per feature execution
- **Scenario Container**: Per scenario execution

### Registration Events

- **`RegisterGlobalDependencies`**: For new interface registrations
- **`CustomizeGlobalDependencies`**: For overriding existing registrations (recommended for `IBuildMetadataProvider`)

The `CustomizeGlobalDependencies` event is used because `IBuildMetadataProvider` is already registered by Reqnroll's default implementation, and you want to override it with your custom implementation.

## Best Practices

1. **Environment Detection**: Always check if you're running in your target environment before returning metadata
2. **Error Handling**: Return `null` when your environment is not detected rather than throwing exceptions
3. **Fallback Support**: Consider supporting fallback to the default provider for standard CI/CD systems
4. **Testing**: Write comprehensive unit tests for your provider logic
5. **Documentation**: Document the environment variables your provider expects
6. **Versioning**: Use semantic versioning for your plugin to manage compatibility
7. **Performance**: Cache expensive operations if environment variable access is costly in your system

By following this guide, you can successfully extend Reqnroll to work with any custom SCM or build system while maintaining compatibility with existing functionality.