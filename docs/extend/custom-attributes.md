# Custom StepDefinition Attributes

Reqnroll provides the ability to create custom StepDefinition attributes, enabling teams to express scenarios in local languages or to define grouped categories for step types beyond `Given`, `When`, and `Then`. This flexibility is commonly leveraged:
- To use attribute names and conventions in a localized (non-English) language, increasing naturalness for teams that work in other languages.
- To create broader or grouped step definition types (such as a general-purpose attribute that covers both "Given" and "When" semantics).

## Key Requirements

To create a custom StepDefinition attribute, the attribute class must:

- Inherit from `StepDefinitionBaseAttribute`.
- Any public property or field intended to map to a constructor parameter must have a constructor parameter with a name that matches the property or field name—except the capitalization of the first character may differ (e.g., Expression property or field, expression parameter).
- Not every property or field must be present in the constructor, but every constructor parameter must have a corresponding property or field with the same name (case-insensitive on the first character).


## Example: Custom Step Attribute

Below is a sample custom attribute that demonstrates the correct pattern. This example creates a `GivenWhenAttribute` attribute, which will match both `Given` and `When` steps.

```csharp
using Reqnroll;
using System;

public class GivenWhenAttribute : StepDefinitionBaseAttribute
{
    readonly static StepDefinitionType[] types = [StepDefinitionType.Given, StepDefinitionType.When];
    public GivenWhenAttribute() : this(null!) { }
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


## References

- Reqnroll Step Definitions: [Documentation](https://docs.reqnroll.net/latest/automation/step-definitions.html)
- Reqnroll Base Attribute Source: [GitHub - Attributes.cs](https://github.com/reqnroll/Reqnroll/blob/main/Reqnroll/Attributes.cs)
