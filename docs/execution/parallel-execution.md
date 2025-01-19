# Parallel Execution

Reqnroll scenarios are often automated as integration or system level tests. The system under test (SUT) might have several external dependencies and a more complex internal architecture. The key design question when running the tests in parallel is how the parallel test executions can be isolated from each other.

## Test Isolation Levels

Determining the ideal level of isolation for your automated tests is a tradeoff. The higher the isolation of the parallel tests the smaller the likelihood of conflicts on shared state and dependencies, but at the same time the higher the execution time and amount of resources needed to maintain the isolated environments.

| Isolation level | Description | Runner support |
| --------------- | ----------- | -------------- |
| Thread | Test threads run as threads in the same process and application domain. Only the thread-local state is isolated. | NUnit, MsTest, xUnit |
| Process | Test threads run in separate processes. | VSTest per test assembly |
| Agent | Test threads run on multiple agents. | E.g. VSTest task |

## Parallel Scheduling Unit

Depending on the test isolation level and the used test runner tools you can consider different "units of scheduling" that can run in parallel with each other.
When using Reqnroll we can consider the parallel scheduling on the level of scenarios, features and test assemblies.

| Scheduling unit  | Description          | Runner support       |
| ---------------- | -------------------- | -------------------- |
| Scenario         | Scenarios can run in parallel with each other (also from different features) | NUnit, MsTest     |
| Feature          | Features can run in parallel with each other. Scenarios from the same feature are running on the same test thread. | NUnit, MsTest, xUnit |
| Test assembly    | Different test assemblies can run in parallel with each other | e.g. VSTest |

## Running Reqnroll features in parallel with thread-level isolation

![Parallel execution of features in Test Explorer](../_static/images/parallel_execution_test_explorer.png)

### Properties

* Tests are running in multiple threads within the same process and the same application domain.
* Only the thread-local state is isolated.
* Smaller initialization footprint and lower memory requirements.
* The Reqnroll binding registry (step definitions, hooks, etc.) and some other core services are shared across test threads.

### Requirements

* You have to use a test runner that supports in-process parallel execution (NUnit and MsTest supports scenario-level, xUnit supports feature-level)
* You have to ensure that your code does not conflict on static state.
* You must not use the static context properties of Reqnroll `ScenarioContext.Current`, `FeatureContext.Current` or `ScenarioStepContext.Current` (see further information below).
* You have to configure the test runner to execute the Reqnroll features in parallel with each other (see configuration details below).

### Execution Behavior

* `[BeforeTestRun]` and `[AfterTestRun]` hooks (events) are executed only once on the first thread that initializes the framework. Executing tests in the other threads is blocked until the hooks have been fully executed on the first thread.
* As a general guideline, **we do not recommend using the `[BeforeFeature]` and `[AfterFeature]` hooks and the `FeatureContext` when running the tests with method-level parallelism**, because in this case there is no guarantee that these hooks will be executed only once per feature and that there will be only one instance of the `FeatureContext` per feature. The lifetime of the `FeatureContext` (that starts and finishes by invoking the `[BeforeFeature]` and `[AfterFeature]` hooks) is controlled by the test runner.  So in the case of running scenarios with method-level parallelism, a feature's scenarios could be distributed across multiple workers and run in parallel. Therefore, each scenario could have its own dedicated `FeatureContext`, or some scenarios of a feature could share the same `FeatureContext`. It all depends on how the test runner (e.g. NUnit or MsTest) distributes the scenarios among the worker threads - which is not predictable or controllable. Because of this behavior of the test runner, Reqnroll can't share the `FeatureContext` between parallel threads. If you want to have a truly singleton `FeatureContext`, and `[BeforeFeature]` and `[AfterFeature]` hook execution, you must use either class-level parallelism or disable parallelism entirely so that scenarios of a feature are all executed on the **same worker thread**.
    * However, if you still want to use method-level parallelism and a `FeatureContext` in your test suite, then **the following things will be true**:
        * The `FeatureContext` and feature-level DI container will remain consistent **per feature, per test thread**. This means that anything you register in the feature container will be resolvable in the `[AfterFeature]` **per test thread**.
        * A given `[BeforeFeature]` or `[AfterFeature]` will only be executed once **per test thread** that runs a scenario of a feature.
        * Types you register in the feature-level DI container that implement `IDisposable` will still be disposed **per feature, per test thread**. (Keep this in mind if you try to work around this parallelism behavior to regain singleton-like behavior. E.g. by using static instances, `Lazy<>`, thread-safe collections, etc.) 
* Scenarios and their related hooks (Before/After scenario, scenario block, step) are isolated in the different threads during execution and do not block each other. Each thread has a separate (and isolated) `ScenarioContext`.
* The test trace listener (that outputs the scenario execution trace to the console by default) is invoked asynchronously from the multiple threads and the trace messages are queued and passed to the listener in serialized form. If the test trace listener implements `Reqnroll.Tracing.IThreadSafeTraceListener`, the messages are sent directly from the threads.

### NUnit Configuration

By default, [NUnit does not run the tests in parallel](https://docs.nunit.org/articles/nunit/writing-tests/attributes/parallelizable.html).
Parallelization must be configured by setting an assembly-level attribute in the Reqnroll project.

```{code-block} csharp
:caption: C# file for configuring feature-level parallelization

using NUnit.Framework;
[assembly: Parallelizable(ParallelScope.Fixtures)]
```

```{code-block} csharp
:caption: C# file for configuring scenario-level parallelization

using NUnit.Framework;
[assembly: Parallelizable(ParallelScope.Children)]
```

### MSTest Configuration

By default, [MsTest does not run the tests in parallel](https://devblogs.microsoft.com/devops/mstest-v2-in-assembly-parallel-test-execution/).
Parallelisation must be configured by setting an assembly-level attribute in the Reqnroll project.

```{code-block} csharp
:caption: C# file for configuring feature-level parallelization

using Microsoft.VisualStudio.TestTools.UnitTesting;
[assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]
```

```{code-block} csharp
:caption: C# file for configuring scenario-level parallelization

using Microsoft.VisualStudio.TestTools.UnitTesting;
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
```

### xUnit Configuration

By default xUnit runs all Reqnroll features [in parallel](https://xunit.net/docs/running-tests-in-parallel) with each other. No additional configuration is necessary.

### Thread-safe ScenarioContext, FeatureContext and ScenarioStepContext

When using parallel execution accessing the obsolete `ScenarioContext.Current`, `FeatureContext.Current` or `ScenarioStepContext.Current` static properties is not allowed.  Accessing these static properties during parallel execution throws a `ReqnrollException`.

To access the context classes in a thread-safe way you can either use context injection or the instance properties of the `Steps` base class. For further details please see the [FeatureContext](../automation/feature-context) and [ScenarioContext](../automation/scenario-context) documentation.

### Excluding Reqnroll features from parallel execution

To exclude specific features from running in parallel with any other features, see the `addNonParallelizableMarkerForTags` [configuration](../installation/configuration.md#generator) option.

Please note that xUnit requires additional configuration to ensure that non parallelizable features do not run in parallel with any other feature. This configuration is automatically provided for users via the xUnit plugin (so no additional effort is required). The following class will be defined within your test assembly for you:

```{code-block} csharp
:caption: C# File

[CollectionDefinition("ReqnrollNonParallelizableFeatures", DisableParallelization = true)]
public class ReqnrollNonParallelizableFeaturesCollectionDefinition
{
}
```

## Running Reqnroll scenarios in parallel with process isolation

If there are no external dependencies or they can be cloned for parallel execution, but the application architecture depends on static state (e.g. static caches etc.), the best way is to execute tests in parallel isolated by process. This ensures that every test execution thread is hosted in a separate process and hence static state is not accessed in parallel. 

### Properties

* Tests threads are separated by a process boundary.
* Also the static memory state is isolated. Conflicts might be expected on external dependencies only.
* Bigger initialization footprint and higher memory requirements.

### Requirements

* You have to use VSTest task.

### Execution Behavior

* `[BeforeTestRun]` and `[AfterTestRun]` hooks are executed for each individual test execution thread, so you can use them to initialize/reset shared memory.
* Each test thread manages its own enter/exit feature execution workflow. The `[BeforeFeature]` and `[AfterFeature]` hooks may be executed multiple times in different test threads if they run scenarios from the same feature file. The execution of these hooks do not block one another, but the Before/After feature hooks are called in pairs within a single thread (the `[BeforeFeature]` hook of the next scenario is only executed after the `[AfterFeature]` hook of the previous one). Each test thread has a separate (and isolated) `FeatureContext`.
