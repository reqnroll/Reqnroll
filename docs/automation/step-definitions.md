# Step Definitions

The step definitions provide the connection between your feature files and application interfaces. You have to add the `[Binding]` attribute to the class where your step definitions are:

```{code-block} csharp
:caption: Step Definition File
[Binding]
public class StepDefinitions
{
	...
}
```

```{note}
Bindings (step definitions, hooks, step argument transformations) are global for the entire Reqnroll project.
```

For better reusability, the step definitions can include parameters. This means that it is not necessary to define a new step definition for each step that just differs slightly. For example, the steps `When I perform a simple search on 'Domain'` and `When I perform a simple search on 'Communication'` can be automated with a single step definition, with 'Domain' and 'Communication' as parameters.  

The following example shows a simple step definition that matches to the step `When I perform a simple search on 'Domain'`:

```{code-block} csharp
:caption: Step Definition File
[When("I perform a simple search on {string}")]
public void WhenIPerformASimpleSearchOn(string searchTerm)
{
    var controller = new CatalogController();
    actionResult = controller.Search(searchTerm);
}
```

Here the method is annotated with the `[When]` attribute, and includes the expression `I perform a simple search on {string}` used to match the step's text. This expression is called a [Cucumber expression](cucumber-expressions). The term `{string}` is used to define a (string) parameter for the method. For detailed description of the expression syntax, check the [](cucumber-expressions) page.

The matching can also be specified using [regular expressions](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions). The step definition above could be also written as:

```{code-block} csharp
:caption: Step Definition File
[When(@"^I perform a simple search on '(.*)'$")]
public void WhenIPerformASimpleSearchOn(string searchTerm)
{
    var controller = new CatalogController();
    actionResult = controller.Search(searchTerm);
}
```

When using regular expressions, the groups (e.g. `(.*)`) define the step definition parameters.

## Supported Step Definition Attributes

* `[Given(expression)]` or `[Given]` - `Reqnroll.GivenAttribute`
* `[When(expression)]` or `[When]` - `Reqnroll.WhenAttribute`
* `[Then(expression)]` or `[Then]` - `Reqnroll.ThenAttribute`
* `[StepDefinition(expression)]` or `[StepDefinition]` - `Reqnroll.StepDefinitionAttribute`, matches for given, when or then attributes

The `expression` can be either a Cucumber Expression or a Regular Expression. 

You can annotate a single method with multiple attributes in order to support different phrasings in the feature file for the same automation logic.

```{code-block} csharp
:caption: Step Definition File
[When("I perform a simple search on {string}")]
[When("I search for {string}")]
public void WhenIPerformASimpleSearchOn(string searchTerm)
{
  ...
}
```

## Other Attributes

The `[Obsolete]` attribute from the system namespace is also supported, the [runtime section](../installation/configuration.md#runtime) of the configuration can be used to influence how Reqnroll behaves when an obsolete step definition is used.

```{code-block} csharp
:caption: Step Definition File
[Given("Stuff is done")]
[Obsolete]
public void GivenStuffIsDone()
{
    var x = 2+3;
}
```


## Step Definition Methods Rules

* Must be in a public class, marked with the `[Binding]` attribute.
* Must be a public method.
* Can be either a static or an instance method. If it is an instance method, the containing class will be instantiated once for every scenario.
* Cannot have `out` or `ref` parameters.
* Cannot have optional parameters.
* Should return `void` or `Task`. Note `async` methods must return `Task`

(step-matching-styles-rules)=
## Step Definition Styles

There are multiple options for step definition matching:

* Use attributes with [cucumber expressions](cucumber-expressions); [async](asynchronous-bindings) or normal
* Use attributes with regular expressions; [async](asynchronous-bindings) or normal

## Parameter Matching Rules

* Step definitions can specify parameters. These will match to the parameters of the step definition method.
* The method parameter type can be `string` or other .NET type. In the later case a [configurable conversion](step-argument-conversions) is applied.
* With cucumber expressions
  * The parameter placeholders (`{parameter-type}`) define the arguments for the method based on the order (the match result of the first group becomes the first argument, etc.).
  * For the exact parameter rules please check the [cucumber expressions](cucumber-expressions) page.
* With regular expressions
  * The match groups (`(…)`) of the regular expression define the arguments for the method based on the order (the match result of the first group becomes the first argument, etc.).
  * You can use non-capturing groups `(?:regex)` in order to use groups without a method argument.
* With method name matching
  * You can refer to the method parameters with either the parameter name (ALL-CAPS) or the parameter index (zero-based) with the `P` prefix, e.g. `P0`.

## Data Table or DocString Arguments

If the step definition method should match for steps having [Data Table or DocString text arguments](../gherkin/gherkin-reference), additional `DataTable` or `string` parameters have to be defined in the method signature to be able to receive these arguments. You cannot have both of these arguments in a step definition.

```{code-block} gherkin
:caption: Feature File
Given the following books
  |Author        |Title                          |
  |Martin Fowler |Analysis Patterns              |
  |Gojko Adzic   |Bridging the Communication Gap |
Given a blog post named "Random" with Markdown body
  """
  Some Title, Eh?
  ===============
  Here is the first paragraph of my blog post. Lorem ipsum dolor sit amet,
  consectetur adipiscing elit.
  """
```

```{code-block} csharp
:caption: Step Definition File
[Given("the following books")]
public void GivenTheFollowingBooks(DataTable table)
{
  ...
}

[Given("a blog post named {string} with Markdown body")]
public void GivenABlogPostWithMarkdownBody(string postName, string bodyText)
{
  ...
}
```

```{note}
For backwards compatibility with SpecFlow, you can also declare data table parameters with the `Reqnroll.Table` class. It is recommended to use the `DataTable` class whenever it is possible.
```
