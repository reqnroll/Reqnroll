# xUnit

Reqnroll supports both xUnit v2 and xUnit v3.

- **xUnit v2**: Supported from xUnit v2 2.8 or later
- **xUnit v3**: Supported since Reqnroll 3.1

Documentation for xUnit can be found [here](https://xunit.net/#documentation).

## xUnit v2

### Needed NuGet Packages

For Reqnroll: [Reqnroll.xUnit](https://www.nuget.org/packages/Reqnroll.xUnit/)

For xUnit: [xUnit](https://www.nuget.org/packages/xunit/)  

For Test Discovery & Execution:

- [xunit.runner.visualstudio](https://www.nuget.org/packages/xunit.runner.visualstudio/)
- [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)

## xUnit v3

### Needed NuGet Packages

For Reqnroll: [Reqnroll.xunit.v3](https://www.nuget.org/packages/Reqnroll.xunit.v3/)

For xUnit: [xunit.v3](https://www.nuget.org/packages/xunit.v3/)

For Test Discovery & Execution:

- [xunit.runner.visualstudio](https://www.nuget.org/packages/xunit.runner.visualstudio/) (version 3.0.2 or later)
- [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)

## Access ITestOutputHelper

The xUnit `ITestOutputHelper` is registered in the ScenarioContainer. You can get access to simply via getting it via [Context-Injection](../automation/context-injection.md).

## Migrating from SpecFlow

Reqnroll generates Task based async code, which is different from the SpecFlow generated synchronous code.
This change has a big effect on how xUnit runs the Tests.

xUnit since Version 2.8 has two modes for running tests in parallel: `conservative` (new and default since 2.8) and `aggressive` the default (older and default before 2.8). For more details about both algorithms and their 
configuration, see the [xUnit Running Tests in Parallel](https://xunit.net/docs/running-tests-in-parallel). 
Reqnroll.xUnit also supports both modes since Version 2.4.0; on older versions, only the aggressive mode was possible (even with xUnit 2.8 or higher).

TLDR is:

* Conservative mode starts just as many tests, which are configured in max parallel threads.
* Aggressive mode starts all tests, and lets the Task Scheduler handle the maximum parallel threads. 

Because of the async nature of Reqnroll generated test code, the aggressive mode together with resource
intensive test (i.e. Browser-based tests) can lead to the impression that more tests are run in parallel then
configured (more Browsers opened than expected). 
This is normally a wanted async/await behavior. To fully utilize the available resources.

If you have resource-intensive tests, use the default conservative mode and limit the parallel tests in xUnit.
But you can opt in the aggressive mode if your tests are lighter on the resources, to optimize the time to finish the tests.

We recommend sticking to the default conservative mode unless you have good reason to use the aggressive mode.

### Example

``` csharp

using System;
using Reqnroll;

[Binding]
public class BindingClass
{
    private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
    public BindingClass(Xunit.Abstractions.ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [When(@"I do something")]
    public void WhenIDoSomething()
    {
        _testOutputHelper.WriteLine("EB7C1291-2C44-417F-ABB7-A5154843BC7B");
    }
}

```