# Autofac

## Introduction
Reqnroll plugin for using Autofac as a dependency injection framework for step definitions.

```{note}
Currently supports Autofac v4.0.0 or above
```

## Step by step walkthrough of using Reqnroll.Autofac


### 1.  Install plugin from NuGet into your Reqnroll project.

```csharp
PM> Install-Package Reqnroll.Autofac
```
### 2. Create static methods somewhere in the Reqnroll project

  Plugin supports both registration of dependencies globally and per scenario:
  
  #### 2.1 Optionally configure dependencies that need to be shared globally for all scenarios:
  
  Create a static method somewhere in the Reqnroll project to register scenario dependencies: 
  (Recommended to put it into the `Support` folder) that returns `void` and has one parameter of Autofac `ContainerBuilder`, tag it with the `[GlobalDependencies]` attribute.

  When registering global dependencies, it is also a requirement to configure scenario dependencies as well in order to register classes  marked with the `[Binding]` attribute as shown below.

  Globally registered dependencies may be resolved in the `[BeforeTestRun]` and `[AfterTestRun]` methods.
    
  #### 2.2 Configure dependencies to be resolved each time for a scenario:
  
  Create a static method somewhere in the Reqnroll project to register scenario dependencies: 
  (Recommended to put it into the `Support` folder) that returns `void` and has one parameter of Autofac `ContainerBuilder`, tag it with the `[ScenarioDependencies]` attribute. 

  #### 2.3 Configure your dependencies for the scenario execution within either the two methods `[GlobalDependencies]` and `[ScenarioDependencies]` or the single `[ScenarioDependencies]` method. 

  #### 2.4 You also have to register the step definition classes in the `[ScenarioDependencies]` method, that you can do by either registering all public types from the Reqnroll project:

```csharp
builder.RegisterAssemblyTypes(typeof(YourClassInTheReqnrollProject).Assembly).SingleInstance();
```
  #### 2.5 or by registering all classes marked with the `[Binding]` attribute:

  You may use a provided extension method to do this, but importing:
```csharp
using Reqnroll.Autofac.ReqnrollPlugin;
```
Then
```csharp
containerBuilder.AddReqnrollBindings<AnyClassInTheReqnrollProject>()
```
Or overload
```csharp
containerBuilder.AddReqnrollBindings(Assembly.GetExecutingAssembly())
```

  Or manually register like so:
```csharp
builder
  .RegisterAssemblyTypes(typeof(AnyClassInTheReqnrollProject).Assembly)
  .Where(t => Attribute.IsDefined(t, typeof(BindingAttribute)))
  .SingleInstance();
```
  ### 3. A typical dependency builder method for `[GlobalDependencies]` with `[ScenarioDependencies]` probably looks like this:

```csharp
public class SetupTestDependencies
{
  [GlobalDependencies]
  public static void SetupGlobalContainer(ContainerBuilder containerBuilder)
  {
    // Register globally scoped runtime dependencies
    containerBuilder
      .RegisterType<MyGlobalService>()
      .As<IMyGlobalService>()
      .SingleInstance();
  }

  [ScenarioDependencies]
  public static void SetupScenarioContainer(ContainerBuilder containerBuilder)
  {
    // Register scenario scoped runtime dependencies
    containerBuilder
      .RegisterType<MyService>()
      .As<IMyService>()
      .SingleInstance();

    // register binding classes
    containerBuilder.AddReqnrollBindings<SetupTestDependencies>();
  }
}
```

  ### 4. It is also possible to continue to use the legacy method as well, however this method is __not__ compatible with global dependency registration and can only be used on its own like so:
  Create a static method somewhere in the Reqnroll project to register scenario dependencies: 
  (Recommended to put it into the `Support` folder) that returns an Autofac `ContainerBuilder` and tag it with the `[ScenarioDependencies]` attribute. 


  ### 5. If you have an existing container, built and owned by your application under test, you can use that instead of letting Reqnroll manage your container
  Create a static method in your Reqnroll project to return a lifetime scope from your container. Note that Reqnroll creates a second scope under yours, 
  so be sure to pair this use-case with the `CreateContainerBuilder` method above to add your step bindings.

```csharp
[FeatureDependencies]
public static ILifetimeScope GetFeatureLifetimeScope()
{
    // TODO: Add any top-level dependencies here, though note that usually step bindings
	//       should be declared in the Configure method below, as this will ensure they
	//       are in the correct scope to inject ScenarioContext etc.
    return containerScope.BeginLifetimeScope();
}

[ScenarioDependencies]
public static void ConfigureContainerBuilder(ContainerBuilder containerBuilder)
{
    //TODO: add customizations, stubs required for testing
    containerBuilder.AddReqnrollBindings<SetupTestDependencies>();
}
```

