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
* IStepArgumentTypeConverter
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
  