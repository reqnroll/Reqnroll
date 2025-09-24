# Custom Step Definition Attributes

Reqnroll provides the ability to create custom [step definition attributes](../automation/step-definitions), enabling teams to express scenarios in local languages or to define grouped categories for step types beyond `Given`, `When`, and `Then`. This flexibility is commonly leveraged:
- To use attribute names and conventions in a localized (non-English) language, increasing naturalness for teams that work in other languages.
- To create broader or grouped step definition types (such as a general-purpose attribute that covers both "Given" and "When" semantics).

## Key Requirements

To create a custom step definition attribute, the attribute class must:

- Inherit from [`StepDefinitionBaseAttribute`](https://github.com/reqnroll/Reqnroll/blob/main/Reqnroll/Attributes.cs).
- Derived constructors should use the same parameter names (expression, types) as the base class constructor.


## Example: Custom Step Attribute

Below is a sample custom attribute that demonstrates the correct pattern. This example creates a `GivenWhenAttribute` attribute, which will match both `Given` and `When` steps.

```csharp
using Reqnroll;
using System;

public class GivenWhenAttribute : StepDefinitionBaseAttribute
{
    readonly static StepDefinitionType[] types = [StepDefinitionType.Given, StepDefinitionType.When];
    public GivenWhenAttribute(string expression) : base(expression, types) { 
    }
}
```

**Usage:**

```csharp
[Binding]
public class CalculatorSteps
{
    [GivenWhen("I add {int} and {int}")]
    public void MyAddStep(int x, int y)
    {
        // Step logic
    }
}
```
