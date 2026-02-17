# Verify

Reqnroll supports Verify.Xunit 24.2.0 or later.  

Documentation for Verify can be found [here](https://github.com/VerifyTests/Verify).

## Needed NuGet Packages

* [Reqnroll.xUnit](https://www.nuget.org/packages/Reqnroll.xUnit/) and its [dependencies](xunit.md#needed-nuget-packages)
* [Reqnroll.Verify](https://www.nuget.org/packages/Reqnroll.Verify/)

## How it works

This plugin adds a VerifySettings instance to Reqnroll's scenario container, which can be used to set the correct path for the tests' verified files.

### Example

```Gherkin
Feature: Verify feature

    Scenario: Verify scenario
        When I calculate 1 + 2
        Then I expect the result is correct
```
```csharp
[Binding]
internal class StepDefinitions
{
    private readonly VerifySettings _settings;
    private int _result;

    public StepDefinitions(VerifySettings settings)
    {
        _settings = settings;
    }
    
    [When("I calculate (\d+) + (\d+)")]
    public void WhenICalculate(int v1, int v2)
    {
        _result = v1 + v2; // simulate calling the SUT to get the result
    }
    
    [Then("I expect the result is correct")]
    public void ThenIExpectTheResultIsCorrect()
    {
        Verifier.Verify(_result, _settings);
    }
}

```

## Legacy global VerifySettings support

For Verify versions prior to 29.0.0, the plugin also supported a legacy mode that uses a global `VerifySettings` instance (i.e. calling `Verifier.Verify()` without specifying the settings). This mode is not thread-safe and should only be used in single-threaded test execution.

From plugin version v3.1 this legacy support has been removed. It is recommended to always inject the `VerifySettings` instance into the step definition class like in the example above.

If this is not possible, the following workaround can be used to still support the legacy mode. The workaround works only in single-threaded test execution.

```csharp
namespace Reqnroll.Verify.ReqnrollPlugin;

[Binding]
public class VerifyHooks
{
    [BeforeTestRun]
    public static void EnableGlobalVerifySettingsForCompatibility()
    {
        Verifier.DerivePathInfo(
            (_, projectDirectory, _, _) =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var scenarioContext = Reqnroll.ScenarioContext.Current;
                var featureContext = Reqnroll.FeatureContext.Current;
#pragma warning restore CS0618 // Type or member is obsolete
                string scenarioInfoTitle = scenarioContext.ScenarioInfo.Title;

                foreach (System.Collections.DictionaryEntry scenarioInfoArgument in scenarioContext.ScenarioInfo.Arguments)
                {
                    scenarioInfoTitle += "_" + scenarioInfoArgument.Value;
                }

                return new PathInfo(
                    Path.Combine(projectDirectory, featureContext.FeatureInfo.FolderPath),
                    featureContext.FeatureInfo.Title,
                    scenarioInfoTitle);
            });
    }
}
```
