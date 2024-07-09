# Mark Steps as Not Implemented

To mark a step as not implemented at runtime, you need to throw a `PendingStepException`. The Runtime of Reqnroll will detect this and will report the appropriate test result back to your test runner.

There are multiple ways to throw the exception.

## Throwing the `PendingStepException`

You can throw the exception using a `throw` statement. In this case you have the possibility to provide a custom message.

### Default Message

```{code-block} csharp
:caption: Step Definition File

[When("I set the current ScenarioContext to pending")]
public void WhenIHaveAPendingStep()
{
    throw new PendingStepException();
}
```

### Custom Message

```{code-block} csharp
:caption: Step Definition File

[When("I set the current ScenarioContext to pending")]
public void WhenIHaveAPendingStep()
{
    throw new PendingStepException("custom pendingstep message");
}
```

## Using ScenarioContext.Pending helper method

The `ScenarioContext` class has a static helper method to throw the default `PendingStepException`.

```{code-block} csharp
:caption: Step Definition File

[When("I set the current ScenarioContext to pending")]
public void WhenIHaveAPendingStep()
{
    ScenarioContext.StepIsPending();
}
```
