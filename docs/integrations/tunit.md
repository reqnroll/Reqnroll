# TUnit

Reqnroll supports TUnit 0.55.23 or later.

Documentation for TUnit can be found [here](https://tunit.dev/).

## Needed NuGet Packages

For Reqnroll: [Reqnroll.TUnit](https://www.nuget.org/packages/Reqnroll.TUnit/)

For TUnit: [TUnit](https://www.nuget.org/packages/TUnit/)  

## Access TestContext

The TUnit test context (`TUnit.Core.TestContext`) is registered in the scenario dependency scope. You can get access to it via [Context-Injection](../automation/context-injection.md) when needed.

## Parallel Execution

TUnit supports test-level (scenario-level) parallel test execution by default. The parallel execution can be disabled for the entire test project using the `[assembly: TUnit.Core.NotInParallel]` attribute or use [](../execution/parallel-execution.md#excluding-reqnroll-features-from-parallel-execution).
