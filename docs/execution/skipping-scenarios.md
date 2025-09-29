# Skipping Scenarios

You can skip programmatically scenarios using the `IUnitTestRuntimeProvider` interface.

## Example Code

```{code-block} csharp
:caption: Step Definition File

[Binding]
public sealed class StepDefinitions
{
    private readonly IUnitTestRuntimeProvider _unitTestRuntimeProvider;

    public CalculatorStepDefinitions(IUnitTestRuntimeProvider unitTestRuntimeProvider)
    {
        _unitTestRuntimeProvider = unitTestRuntimeProvider;
    }

    [When("your binding")]
    public void YourBindingMethod()
    {
        _unitTestRuntimeProvider.TestIgnore("This scenario is always skipped");
    }
}
```

Ignoring is like skipping the scenario. Be careful, as it behaves a little bit different for the different unit test runners (xUnit, NUnit, TUnit, MSTest).

