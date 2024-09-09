# Verify

Reqnroll supports Verify 24.2.0 or later.  

Documentation for Verify can be found [here](https://github.com/VerifyTests/Verify).

## Needed NuGet Packages

* [Reqnroll.xUnit](https://www.nuget.org/packages/Reqnroll.xUnit/) and its [dependencies](xunit.md#Needed%20NuGet%20Packages)
* [Reqnroll.Verify](https://www.nuget.org/packages/Reqnroll.Verify/)

## How it works

The plugin works by calling the global Verifier.DerivePathInfo method to set the correct path for the tests' verified files. For tests that are ran in sequence this is sufficient.
For parallel tests a VerifySettings instance is also registered in Reqnroll's scenario container. This instance is can then be used to set the correct path for the tests' verified files.

### Example

```Gherkin
Feature: Verify feature

    Scenario: Verify scenario
        When I calculate 1 + 2
        Then I expect the result is correct
```

Example **without** support for parallelization:
```csharp
[Binding]
internal class StepDefinitions
{
    [When("I calculate (\d+) + (\d+)")]
    public void WhenICalculate(int value, int value2)
    {
        _settings.Verify(value + value);
    }
    
    [Then("I expect the result is correct")]
    public void ThenIExpectTheResultIsCorrect()
    {
    }
}
```

Example **with** support for parallelization:
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
