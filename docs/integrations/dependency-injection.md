# Microsoft.Extensions.DependencyInjection

## Introduction
Reqnroll plugin for using Microsoft.Extensions.DependencyInjection as a dependency injection framework for step definitions.

```{note}
Currently supports Microsoft.Extensions.DependencyInjection v6.0.0 or above
```

## Step by step walkthrough of using Reqnroll.Microsoft.Extensions.DependencyInjection


### 1.  Install plugin from NuGet into your Reqnroll project.

```csharp
PM> Install-Package Reqnroll.Microsoft.Extensions.DependencyInjection
```
### 2. Create static methods somewhere in the Reqnroll project

Create a static method in your SpecFlow project that returns a Microsoft.Extensions.DependencyInjection.IServiceCollection and tag it with the [ScenarioDependencies] attribute. Configure your test dependencies for the scenario execution within this method. Step definition classes (i.e. classes with the SpecFlow [Binding] attribute) are automatically added to the service collection.
  
### 3. A typical dependency builder method looks like this:

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
