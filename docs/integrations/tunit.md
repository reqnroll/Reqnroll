# TUnit

Reqnroll supports TUnit 0.55.23 or later.

Documentation for TUnit can be found [here](https://tunit.dev/).

## Needed NuGet Packages

For Reqnroll: [Reqnroll.TUnit](https://www.nuget.org/packages/Reqnroll.TUnit/)

For TUnit: [TUnit](https://www.nuget.org/packages/TUnit/)  

For Test Discovery & Execution:

- [Microsoft.Testing.Extensions.TrxReport](https://www.nuget.org/packages/Microsoft.Testing.Extensions.TrxReport/)
- [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)

## Access TestContext

The TUnit `TestContext` is registered in the ScenarioContainer. You can get access to it via [Context-Injection](../automation/context-injection.md).

### Test Output

You can write test output using the TestContext:

``` csharp
using System;
using Reqnroll;
using TUnit.Core;

[Binding]
public class BindingClass
{
    private readonly TestContext _testContext;
    
    public BindingClass(TestContext testContext)
    {
        _testContext = testContext;
    }

    [When(@"I do something")]
    public void WhenIDoSomething()
    {
        _testContext.OutputWriter.WriteLine("Test output message");
    }
}
```

### Adding Attachments

You can add file attachments to the test results using the TestContext:

``` csharp
using System;
using System.IO;
using Reqnroll;
using TUnit.Core;

[Binding]
public class BindingClass
{
    private readonly TestContext _testContext;
    
    public BindingClass(TestContext testContext)
    {
        _testContext = testContext;
    }

    [When(@"I create a screenshot")]
    public void WhenICreateAScreenshot()
    {
        var filePath = "screenshot.png";
        // ... create your file ...
        
        var artifact = new Artifact
        {
            File = new FileInfo(filePath),
            DisplayName = "Test Screenshot"
        };
        
        _testContext.AddArtifact(artifact);
    }
}
```

### TestContext in Hooks

You can access the TestContext in your hook classes by constructor injection, just like in step definitions:

``` csharp
using Reqnroll;
using TUnit.Core;

[Binding]
public class Hooks
{
    private readonly TestContext _testContext;
    
    public Hooks(TestContext testContext)
    {
        _testContext = testContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _testContext.OutputWriter.WriteLine("Starting scenario");
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _testContext.OutputWriter.WriteLine("Scenario completed");
    }
}
```

## Migrating from SpecFlow

Reqnroll generates Task-based async code, which is different from the SpecFlow generated synchronous code. TUnit natively supports async test methods, so this transition should be smooth.

## Parallel Execution

TUnit supports parallel test execution by default. You can control parallel execution at the test or class level using the `[NotInParallel]` attribute:

### Class-level Control

``` csharp
using TUnit.Core;

[NotInParallel]  // This test class will not run in parallel with other tests
public class SerializedTests
{
    // Test methods...
}
```

### Test-level Control

``` csharp
[Test]
[NotInParallel]  // This specific test will not run in parallel with other tests
public async Task MyTest()
{
    // Test implementation...
}
```

### Assembly-level Control

You can also control parallel execution at the assembly level by configuring TUnit for the entire test assembly:

``` csharp
using TUnit.Core;

[assembly: NotInParallel]
```

This is particularly useful for resource-intensive tests (e.g., browser-based tests) where you want to control the level of parallelism to avoid issues with shared resources.