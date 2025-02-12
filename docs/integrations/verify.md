# Verify

Reqnroll supports Verify 24.2.0 or later.  

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

    public StepDefinitions(VerifySettings settings)
    {
        _settings = settings;
    }
    
    [When("I calculate (\d+) + (\d+)")]
    public void WhenICalculate(int value, int value2)
    {
        _settings.Verify(value + value, _settings);
    }
    
    [Then("I expect the result is correct")]
    public void ThenIExpectTheResultIsCorrect()
    {
    }
}

```

**Note:** in a single-threaded environment, the plugin will work without the injected VerifySettings instance. However, in a multithreaded environment, the VerifySettings instance must be injected into the step definition class for a deterministic outcome.
