# Executing Reqnroll Scenarios

In order to execute your Reqnroll tests, you need to define the tests as [Gherkin feature files](../Gherkin/Gherkin-Reference.md), [bind](../Bindings/Bindings.md) the steps defined in your feature files to your code, and configure a unit test provider to execute the tests. Reqnroll generates executable unit tests from your Gherkin files.

We recommend that you add a separate project to your solution for your tests.

## Configuring the Unit Test Provider

Tests are executed using a [unit test provider](../Installation/Unit-Test-Providers.md). Add the corresponding NuGet package to your project to define your unit test provider:

* SpecRun.Runner
* Reqnroll.xUnit
* Reqnroll.MsTest
* Reqnroll.NUnit

**You can only have one unit test provider!**

## Configuring the Unit Test Provider with SpecFlow 2 (Legacy)

Configure your unit test provider in your project's app.config file, e.g.:

```xml
<reqnroll>
  <unitTestProvider name="MsTest" />
</reqnroll>
```
