# Castle Windsor

## Introduction
Reqnroll plugin for using Castle Windsor as a dependency injection framework for step definitions.

```{note}
Currently supports Castle Windsor v5.0.1 or above
```

## Step by step walkthrough of using Reqnroll.Windsor

### 1. Install plugin

**-** Install plugin from NuGet into your Reqnroll project.

```csharp
PM> Install-Package Reqnroll.Windsor
```
### 2. Create static method

**-** Create a static method somewhere in the Reqnroll project
  
  (Recommended to put it into the `Support` folder) that returns a Windsor `IWindsorContainer` and tag it with the `[ScenarioDependencies]` attribute.
  
**-** Configure your dependencies for the scenario execution within the method.
  
**-** All your binding classes are automatically registered, including ScenarioContext etc.

### 3. Sample dependency builder method

**-** A typical dependency builder method probably looks like this:

```csharp
[ScenarioDependencies]
public static IWindsorContainer CreateContainer()
{
  var container = new WindsorContainer();

  //TODO: add customizations, stubs required for testing

  return container;
}
```

### 4. Reusing a container

**-** To re-use a container between scenarios, try the following:

Your shared services will be resolved from the root container, while scoped objects
such as ScenarioContext will be resolved from the new container.
```csharp
[ScenarioDependencies]
public static IWindsorContainer CreateContainer()
{
  var container = new WindsorContainer();
  container.Parent = sharedRootContainer;

  return container;
}
```

### 5. Customize binding behavior

**-** To customize binding behavior, use the following:

Default behavior is to auto-register bindings. To manually register these during `CreateContainer`
you can use the following attribute:

```csharp
[ScenarioDependencies(AutoRegisterBindings = false)]
public static IWindsorContainer CreateContainer()
{
    // Register your bindings here
}
```