# MSTest

Reqnroll supports MsTest V2 (NuGet Version 2.2.8 or higher).

Documentation for MSTest can be found [here](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-your-code?view=vs-2019).

## Needed NuGet Packages

For Reqnroll: [Reqnroll.MSTest](https://www.nuget.org/packages/Reqnroll.MSTest/)  

For MSTest: [MSTest.TestFramework](https://www.nuget.org/packages/MSTest.TestFramework/)  

For Test Discovery & Execution:

- [MSTest.TestAdapter](https://www.nuget.org/packages/MSTest.TestAdapter/)
- [Microsoft.NET.Test.Sdk](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk)

## Accessing TestContext

You can access the MsTest TestContext instance in your step definition or hook classes by constructor injection:

``` csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

[Binding]
public class MyStepDefs
{
    private readonly TestContext _testContext;
    public MyStepDefs(TestContext testContext) // use it as ctor parameter
    { 
        _testContext = testContext;
    }

    [Given("a step")]
    public void GivenAStep()
    {
        //you can access the TestContext injected in the ctor
        _testContext.WriteLine(_testContext.TestRunDirectory);
    }


    [BeforeScenario()]
    public void BeforeScenario()
    {
        //you can access the TestContext injected in the ctor
        _testContext.WriteLine(_testContext.TestRunDirectory);
    } 
}
```

In the static BeforeTestRun/AfterTestRun hooks you can use parameter injection:

``` csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

[Binding]
public class Hooks
{
    [BeforeTestRun]
    public static void BeforeTestRun(TestContext testContext)
    {
        //you can access the TestContext injected as parameter
        testContext.WriteLine(testContext.TestRunDirectory);
    }

    [AfterTestRun]
    public static void AfterTestRun(TestContext testContext)
    {
        //you can access the TestContext injected as parameter
        testContext.WriteLine(testContext.DeploymentDirectory);
    }
}
```

## Tags for TestClass Attributes

The MsTest Generator can generate test class attributes from tags specified on a **feature**.

### Owner

Tag:

``` gherkin
@Owner:John
```

Output:

``` csharp
[Microsoft.VisualStudio.TestTools.UnitTesting.OwnerAttribute("John")]
```

### Priority

Tag:

``` gherkin
@Priority:1
```

Output:

``` csharp
[Microsoft.VisualStudio.TestTools.UnitTesting.PriorityAttribute(1)]
```

Remarks:

The attribute is generated only when the value is a valid integer (valid means supported by [` int.TryParse `](https://learn.microsoft.com/it-it/dotnet/api/system.int32.tryparse?#system-int32-tryparse(system-string-system-int32@)))

### WorkItem

Tag:

``` gherkin
@WorkItem:123
```

Output:

``` csharp
[Microsoft.VisualStudio.TestTools.UnitTesting.WorkItemAttribute(123)]
```

### DeploymentItem

#### Example 1 : Copy a file to the same directory as the deployed test assemblies

Tag:

``` gherkin
@MsTest:DeploymentItem:test.txt
```

Output:

``` csharp
[Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("test.txt")]
```

#### Example 2 : Copy a file to a sub-directory relative to the deployment directory

Tag:

``` gherkin
@MsTest:DeploymentItem:Resources\DeploymentItemTestFile.txt:Data
```

Output:

``` csharp
[Microsoft.VisualStudio.TestTools.UnitTesting.DeploymentItemAttribute("Resources\\DeploymentItemTestFile.txt", "Data")]
```
