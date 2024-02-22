# Asynchronous Bindings

If you have code that executes an [asynchronous task](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/index), you can define asynchronous bindings to execute the corresponding code using the `async` and `await` keywords.

The following example shows a step definition with an asynchronous When step:

```{code-block} csharp
:caption: Step Definition File
[When(@"I want to get the web page '(.*)'")]
public async Task WhenIWantToGetTheWebPage(string url)
{
    var message = await _httpClient.GetAsync(url);
    // ...
}
```

```{hint}
You can also use asynchronous [step argument transformations](step-argument-conversions).
```

```{hint}
It is also possible to use [`ValueTask`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask?view=net-6.0) and [`ValueTask<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.valuetask-1?view=net-6.0) return types.
```
