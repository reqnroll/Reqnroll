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

### 4. Alternatively, return `IServiceProvider` from your static method
This scenario is useful for integrating with ASP.NET Core projects, where you have a service provider that is owned by a separate class, and you want to use this pre-built container. 

Note that you will need to manually register your bindings before using them. See below for a typical example:

```csharp
public class SetupTestDependencies
{
  // This is created when doing integration testing in ASP.NET Core
  // See https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests
  private CustomWebApplicationFactory<Program> application = new();

  [ScenarioDependencies]
  public static IServiceProvider GetServices()
  {
    return application.Services;
  }

  public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        // TODO: add your test dependencies here
        services.AddSingleton<IMyService, MyService>();

        // REQUIRED: Manually register bindings and required classes from this assembly.
        services.AddReqnrollBindings<SetupTestDependencies>();
      });
    }
  }
}
```