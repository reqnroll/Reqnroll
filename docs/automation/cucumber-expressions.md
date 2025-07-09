# Cucumber Expressions

Cucumber Expression is an expression type to specify [step definitions](step-definitions). Cucumber Expressions is an alternative to [Regular Expressions](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) with a more intuitive syntax.

You can find a detailed description about cucumber expressions [on GitHub](https://github.com/cucumber/cucumber-expressions#readme). In this page we only provide a short summary and the special handling in .NET / Reqnroll.

The following step definition that uses cucumber expression matches to the step `When I have 42 cucumbers in my belly`

```{code-block} csharp
:caption: Step Definition File
[When("I have {int} cucumbers in my belly")]
public void WhenIHaveCucumbersInMyBelly(int count) { ... }
```

## Cucumber Expression basics

### Simple text

To match for a simple text, just use the text as cucumber expression. 

`[When("I do something")]` matches to `When I do something`

### Parameters

Parameters can be defined using the `{parameter-type}` syntax. The type name is case-sensitive. The `parameter-type` can be any of the following:

* The types: `{int}`, `{long}`, `{byte}`, `{float}`, `{double}`, `{decimal}`, `{DateTime}`, `{Guid}`
* Quoted strings: `{string}`. The text should have single or double quotes. E.g., `[Given("a user {string}")]` matches to `Given a user "Marvin"` or `Given a user 'Zaphod Beeblebrox'`.
* A single word without quotes, `{word}`. E.g., `[Given("a user {word}")]` matches to `Given a user Marvin`.
* Any other type, `{}`, like `(.*)` when using regex
* An enum type, with or without a namespace. E.g. `[When("I have {CustomColor} cucumbers in my belly")]` matches to `When I have green cucumbers in my belly` if `CustomColor` is an enum with `Green` as a value.
* [Step Argument Transformations](step-argument-conversions):
  * with a name property, E.g., With `[StepArgumentTransformation("v(.*)", Name = "my_version")]`, you can define a step as `[When("I download the release {my_version} of the application")]` that matches to `When I download the release v1.2.3 of the application`.
  * without a name property, use the typename of the return type without namespace. E.g. If the method  `[StepArgumentTransformation("EUR (\d+)"]` has a return type of `MyCurrency`  you could use `{MyCurrency}` in the expression.
    
#### No built-in support

These have no built-in support for:
* `{TimeSpan}`
* `{TimeOnly}`
* `{DateOnly}`

For `TimeSpan`, `TimeOnly` and `DateOnly` types, `{}` could be used. 

### Optional parameters and alternatives

Parameters that are optional should be wrapped with parentheses (`(...)`).
The `/` character could be used to provide alternatives.

For example, this step definition:

```{code-block} csharp
:caption: Step Definition File
[When("I have {int} cucumber(s) in my belly/tummy")]
public void WhenIHaveCucumbersInMyBelly(int count)
```

will match to all of the following steps

```{code-block} gherkin
:caption: Feature File
When I have 42 cucumbers in my belly
When I have 1 cucumber in my belly
When I have 8 cucumbers in my tummy
```

## Using Cucumber Expressions with Reqnroll

You can use both cucumber expressions and regular expressions in your project. Reqnroll has uses some heuristics to decide if your expression is a cucumber expression or a regular expression.

In case your regular expression is wrongly detected as cucumber expression, you can always force to use regular expression by specifying the regex start/end markers (`^`/`$`).

```{code-block} csharp
:caption: Step Definition File
[When(@"^this expression is treated as a regex$")]
```
