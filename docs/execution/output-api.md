# Output API

The Reqnroll Output API allows you to display texts and attachments in your IDE's test explorer output window and also in the test result logs.

To use the Reqnroll output API interface you must inject the `IReqnrollOutputHelper` interface via [Context Injection](../automation/context-injection):

```{code-block} csharp
:caption: Step Definition File

private readonly IReqnrollOutputHelper _reqnrollOutputHelper;

public CalculatorStepDefinitions(IReqnrollOutputHelper outputHelper)
{
    _reqnrollOutputHelper = outputHelper;
}
```

There are two methods available:

## `WriteLine(string text)`

This method adds text:

```{code-block} csharp
_reqnrollOutputHelper.WriteLine("TEXT");
```

## `AddAttachment(string filePath)`

This method adds an attachment and requires the file path:

```{code-block} csharp
_reqnrollOutputHelper.AddAttachment("filePath");
```

```{note}
The attachment file can be stored anywhere. But it is important to keep mind that if a local file is added, it will only work on your machine and not accessible when shared with others.
```

```{note}
Handling of attachments depends on your runner. MStest and NUnit currently support this feature but xUnit do **not**.
```
