# Feature Context

Reqnroll provides access to the current test context using both `FeatureContext` and the more commonly used [ScenarioContext](scenario-context). `FeatureContext` persists for the duration of the execution of an entire feature, whereas `ScenarioContext` only persists for the duration of a scenario.

## Accessing the FeatureContext

### in Bindings

To access the `FeatureContext` you have to get it via [Context-Injection](context-injection).

Example:  

```{code-block} csharp
:caption: Step Definition File

[Binding]
public class Binding
{
    private FeatureContext _featureContext;

    public Binding(FeatureContext featureContext)
    {
        _featureContext = featureContext;
    }
}

```

Now you can access the FeatureContext in all your Bindings with the `_featureContext` field.

### in Hooks

#### Before/AfterTestRun

Accessing the `FeatureContext` is not possible, as no `Feature` is executed, when the hook is called.

#### Before/AfterFeature

You can get the `FeatureContext` via parameter of the static method.

Example:

```{code-block} csharp
:caption: Hook File

[Binding]
public class Hooks
{
    [BeforeFeature]
    public static void BeforeFeature(FeatureContext featureContext)
    {
        Console.WriteLine("Starting " + featureContext.FeatureInfo.Title);
    }

    [AfterFeature]
    public static void AfterFeature(FeatureContext featureContext)
    {
        Console.WriteLine("Finished " + featureContext.FeatureInfo.Title);
    }
}
```

#### Before/AfterScenario

Accessing the `FeatureContext` is done like in [normal bindings](#in-bindings)

#### Before/AfterStep

Accessing the `FeatureContext` is done like in [normal bindings](#in-bindings)

## Storing data in the FeatureContext

`FeatureContext` implements Dictionary<string, object>. So you can use the `FeatureContext` like a property bag.  

## FeatureContext.FeatureInfo

`FeatureInfo` provides more information than `ScenarioInfo`, but it works the same:

In the feature file:

```{code-block} gherkin
:caption: Feature File

Scenario: Showing information of the feature

When I execute any scenario in the feature
Then the FeatureInfo contains the following information
    | Field          | Value                               |
    | Tags           | showUpInScenarioInfo, andThisToo    |
    | Title          | FeatureContext features             |
    | TargetLanguage | CSharp                              |
    | Language       | en-US                               |
    | Description    | In order to                         |
```

...and in the step definition:

```{code-block} csharp
:caption: Step Definition File

private class FeatureInformation
{
    public string Title { get; set; }
    public GenerationTargetLanguage TargetLanguage { get; set; }
    public string Description { get; set; }
    public string Language { get; set; }
    public string[] Tags { get; set; }
}

[When(@"I execute any scenario in the feature")]
public void ExecuteAnyScenario() { }

[Then(@"the FeatureInfo contains the following information")]
public void FeatureInfoContainsInterestingInformation(DataTable table)
{
    // Create our small DTO for the info from the step
    var fromStep =  table.CreateInstance<FeatureInformation>();
    fromStep.Tags = table.Rows[0]["Value"].Split(',');

    var fi = _featureContext.FeatureInfo;

    // Assertions
    fi.Title.Should().Equal(fromStep.Title);
    fi.GenerationTargetLanguage.Should().Equal(fromStep.TargetLanguage);
    fi.Description.Should().StartWith(fromStep.Description);
    fi.Language.IetfLanguageTag.Should().Equal(fromStep.Language);
    for (var i = 0; i < fi.Tags.Length - 1; i++)
    {
        fi.Tags[i].Should().Equal(fromStep.Tags[i]);
    }
}
```

`FeatureContext` exposes a Binding Culture property that simply points to the culture the feature is written in (en-US in our example).
