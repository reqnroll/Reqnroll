# Microsoft.Extensions.DependencyInjection

## Introduction
Reqnroll plugin for using Microsoft.Extensions.DependencyInjection as a dependency injection framework for step definitions.

```{note}
Currently supports Microsoft.Extensions.DependencyInjection v6.0.0 or above
```

## Install the plugin from NuGet into your Reqnroll project

Install the `Reqnroll.Microsoft.Extensions.DependencyInjection` NuGet package directly into your test project.

```powershell
Install-Package Reqnroll.Microsoft.Extensions.DependencyInjection
```

## Using the plugin

Create a static, parameterless method in your Reqnroll project that returns an instance of `Microsoft.Extensions.DependencyInjection.IServiceCollection` and tag it with the `[ScenarioDependencies]` attribute. Configure your test dependencies for the scenario execution within this method. Step definition classes (i.e. classes with the Reqnroll `[Binding]` attribute) are automatically added to the service collection.

A typical dependency builder method looks like this:

```csharp
public class SetupTestDependencies
{
  [ScenarioDependencies]
  public static IServiceCollection CreateServices()
  {
    var services = new ServiceCollection();
    
    // TODO: add your test dependencies here
    services.AddSingleton<IMyService, MyService>();

    return services;
  }
}
```

### Configuring the scope and lifetime of the service provider

For services registered with a scoped lifetime (as opposed to singleton), it might make sense to have a new scope for each feature instead of the default per-scenario scope. If this is the case, this can be adjusted with the `ScopeLevel` property on the `[ScenarioDependencies]` attribute. For example

```csharp
[ScenarioDependencies(ScopeLevel = ScopeLevelType.Feature)]
public static IServiceCollection CreateServices()
```

It's also possible to change the lifetime of the entire service provider, rather than just its scope. This is particularly useful when you want a new instance of a singleton service for each feature or each scenario.

```csharp
[ScenarioDependencies(ServiceProviderLifetime = ServiceProviderLifetimeType.Feature)]
public static IServiceCollection CreateServices()
```

```{note}
`ServiceProviderLifetime` and `ScopeLevel` are configured independently. If the `ServiceProviderLifetime` is set to `Scenario` then the `ScopeLevel` is implicitly `Scenario` as well.
```