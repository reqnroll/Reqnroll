# Available Containers

## Global Container

The global container captures global services for test execution and the step definition, hook and transformation discovery result (i.e. what step definitions you have).

* IRuntimeConfigurationProvider
* ITestRunnerManager
* IStepFormatter
* ITestTracer
* ITraceListener
* ITraceListenerQueue
* IErrorProvider
* IRuntimeBindingSourceProcessor
* IRuntimeBindingRegistryBuilder
* IBindingRegistry
* IBindingFactory
* IStepDefinitionRegexCalculator
* IBindingInvoker
* IStepDefinitionSkeletonProvider
* ISkeletonTemplateProvider
* IStepTextAnalyzer
* IRuntimePluginLoader
* IBindingAssemblyLoader
* IBindingInstanceResolver
* RuntimePlugins
  * RegisterGlobalDependencies- Event
  * CustomizeGlobalDependencies- Event

## Test Thread Container

```{note}
Parent Container is the Global Container
```

The test thread container captures the services and state for executing scenarios on a particular test thread. For parallel test execution, multiple test runner containers are created, one for each thread.

* ITestRunner
* IContextManager
* ITestExecutionEngine
* [IStepArgumentTypeConverter](#istepargumenttypeconverter)
* IStepDefinitionMatchService
* ITraceListener
* ITestTracer
* RuntimePlugins
  * CustomizeTestThreadDependencies- Event

## Feature Container

```{note}
Parent Container is the Test Thread Container
```

The feature container captures a feature's execution state. It is disposed after the feature is executed.

* FeatureContext (also available from the *test thread container* through `IContextManager`)
* RuntimePlugins
  * CustomizeFeatureDependencies- Event

## Scenario Container

```{note}
Parent Container is the Feature Container
```

The scenario container captures the state of a scenario execution. It is disposed after the scenario is executed.

* (step definition classes)
* (dependencies of the step definition classes, aka context injection)
* ScenarioContext (also available from the *Test Thread Container* through `IContextManager`)
* RuntimePlugins
  * CustomizeScenarioDependencies- Event

## IStepArgumentTypeConverter

The `IStepArgumentTypeConverter` service is responsible for converting step arguments from their original form (typically strings or DataTable objects) into the target parameter types expected by step definition methods. This is a key component in Reqnroll's step argument conversion pipeline.

### Interface Definition

```csharp
public interface IStepArgumentTypeConverter
{
    Task<object> ConvertAsync(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo);
    bool CanConvert(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo);
}
```

### Default Implementation

Reqnroll provides a default implementation `StepArgumentTypeConverter` that handles:

* Standard conversions (using `Convert.ChangeType()`)
* Enum conversions from string values
* GUID conversions (including partial GUIDs)
* Step argument transformations (methods decorated with `[StepArgumentTransformation]`)
* DataTable conversions
* Custom type converters (classes with `[TypeConverter]` attribute)

### When to Customize

You might want to create a custom implementation of `IStepArgumentTypeConverter` when:

* You need specialized conversion logic that goes beyond step argument transformations
* You want to modify the conversion precedence or selection logic
* You need to integrate with a custom dependency injection container for argument resolution
* You want to add logging, caching, or other cross-cutting concerns to the conversion process
* You need to handle custom data types that require complex conversion logic

### Container Registration

`IStepArgumentTypeConverter` is registered in the **Test Thread Container** as:

```csharp
testThreadContainer.RegisterTypeAs<StepArgumentTypeConverter, IStepArgumentTypeConverter>();
```

To register a custom implementation, you can override this registration in a plugin:

```csharp
[RuntimePlugin]
public class CustomConverterPlugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestFeatureGeneratorProvider unitTestFeatureGeneratorProvider)
    {
        runtimePluginEvents.CustomizeTestThreadDependencies += (sender, args) =>
        {
            args.ObjectContainer.RegisterTypeAs<MyCustomStepArgumentTypeConverter, IStepArgumentTypeConverter>();
        };
    }
}
```

### Implementation Example

Here's an example of a custom step argument type converter that extends the default behavior:

```csharp
public class CustomStepArgumentTypeConverter : StepArgumentTypeConverter
{
    public CustomStepArgumentTypeConverter(
        ITestTracer testTracer, 
        IBindingRegistry bindingRegistry, 
        IContextManager contextManager, 
        IAsyncBindingInvoker bindingInvoker) 
        : base(testTracer, bindingRegistry, contextManager, bindingInvoker)
    {
    }

    public override async Task<object> ConvertAsync(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo)
    {
        // Add custom logging
        if (testTracer != null)
            testTracer.TraceStep($"Converting '{value}' to type '{typeToConvertTo}'", false);

        // Handle special cases before falling back to default behavior
        if (typeToConvertTo is RuntimeBindingType runtimeType)
        {
            // Example: Custom handling for specific types
            if (runtimeType.Type == typeof(Uri) && value is string uriString)
            {
                return new Uri(uriString);
            }
            
            // Example: Custom complex type conversion
            if (runtimeType.Type.Name.EndsWith("DTO") && value is string jsonString)
            {
                return JsonSerializer.Deserialize(jsonString, runtimeType.Type);
            }
        }

        // Fall back to default conversion logic
        return await base.ConvertAsync(value, typeToConvertTo, cultureInfo);
    }

    public override bool CanConvert(object value, IBindingType typeToConvertTo, CultureInfo cultureInfo)
    {
        // Check for custom conversions first
        if (typeToConvertTo is RuntimeBindingType runtimeType)
        {
            if (runtimeType.Type == typeof(Uri) && value is string)
                return Uri.TryCreate(value as string, UriKind.Absolute, out _);
                
            if (runtimeType.Type.Name.EndsWith("DTO") && value is string)
                return true;
        }

        // Fall back to default logic
        return base.CanConvert(value, typeToConvertTo, cultureInfo);
    }
}
```

### Alternative: Using Step Argument Transformations

In many cases, instead of implementing a custom `IStepArgumentTypeConverter`, you can use [Step Argument Transformations](../automation/step-argument-conversions.md#step-argument-transformation), which are simpler to implement and more focused:

```csharp
[Binding]
public class CustomTransformations
{
    [StepArgumentTransformation(@"https?://[^\s]+")]
    public Uri UrlTransform(string url)
    {
        return new Uri(url);
    }

    [StepArgumentTransformation]
    public ComplexType JsonTransform(string json)
    {
        return JsonSerializer.Deserialize<ComplexType>(json);
    }
}
```
