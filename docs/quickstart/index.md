 # Quickstart

This guide gives a quick introduction to Reqnroll.

```{note}
The guide uses Visual Studio 2022 as an IDE, but you can also follow it with other tools.
```

In this tutorial we demonstrate the usage of Reqnroll by implementing the *price calculation* module of an *online instrument & accessories shop*.

## Setup environment & get starting point

When you use Visual Studio 2022 for Reqnroll, please make sure you install the Reqnroll for Visual Studio extension. See [](../installation/setup-ide.md#setup-visual-studio-2022) for details. Please make sure that the SpecFlow extension is disabled or removed for this Quickstart.

We prepared a simple starting point for this tutorial that you can find on GitHub: [https://github.com/reqnroll/Quickstart](https://github.com/reqnroll/Quickstart). Clone this project to your machine or [download it as zip](https://github.com/reqnroll/Quickstart/archive/refs/heads/main.zip) and extract it to a local folder.

Open the solution file (`ReqnrollQuickstart.sln`) in Visual Studio 2022 and let's have a look at the content:

* The solution contains two projects: `ReqnrollQuickstart.App` is our application that we build, `ReqnrollQuickstart.Specs` contains the automated specification for it, so basically the Reqnroll acceptance tests. We will refer to this as Reqnroll project in this guide.
* The application contains one important class for now, the `PriceCalculator`, this is a very simple class, with an unfinished method for calculating the price.
* The Reqnroll project has a single specification file (called [feature file](../gherkin/feature-files)), the `PriceCalculation.feature` in the `Features` folder, with our first scenario for the pricing module.

The Reqnroll project has been configured for using Reqnroll with MsTest. You can check the [](../installation/setup-project.md) guide for more details about the installation and setup options.

Make sure you build your solution, otherwise the feature file editor might behave incorrectly in Visual Studio.

Once you have done this, you should see these files. 

```{code-block} csharp
:caption: PriceCalculator.cs
namespace ReqnrollQuickstart.App;

public class PriceCalculator
{
    // the item prices are hard coded for now
    private readonly Dictionary<string, decimal> _priceTable = new()
    {
        { "Electric guitar", 180.0 },
        { "Guitar pick", 1.5 }
    };

    public decimal CalculatePrice(Dictionary<string, int> basket)
    {
        throw new NotImplementedException();
    }
}
```

```{code-block} gherkin
:caption: PriceCalculation.feature
Feature: Price calculation

This feature is about calculating the basket price.

We work with fixed item prices for now:
* Electric guitar: $180
* Guitar pick: $1.5

Rule: The price for a basket with items can be calculated based on the item prices

Scenario: Client has a simple basket
    Given the client started shopping
    And the client added 1 pcs of "Electric guitar" to the basket
    When the basket is prepared
    Then the basket price should be $180
```

## Automating the first scenario

Run the tests from the Reqnroll project by opening the *Test Explorer* window (using the *Test / Test Explorer* menu command) and run all tests. You can find more details about running the tests in the [](../execution/executing-reqnroll-scenarios.md) guide.

The test execution reports a so called "undefined" state for our scenario. That means that Reqnroll has detected the scenario, but we did not *define* how the scenario steps should be automated. We will do this now.

In order to define the steps, we need to create a *step definition class*. This can be done be copying the code snippet from the test result output, but with Visual Studio we can also use the *Define Steps* dialog. You can access it by invoking the "Define Steps..." command from the feature file editor context menu or with the *Ctrl+B, D* keyboard shortcut from the editor.

## Generate step definition snippets

For now, we can simply accept the suggestion provided by the *Define Steps* dialog by clicking to the *Create* button.

This will create a new class `PriceCalculationStepDefinitions` in the `StepDefinitions` folder. 

The class contains suggestions provided by the Visual Studio extension. In many cases the suggestions are just perfect and you don't need to change them. In some other cases, like in ours, you need to make some small corrections on the generated names and types.

In our case we can provide more meaningful parameter names (instead of `p0` and `p1`). You can see the updated parameter names in the emphasized lines below. 

After converting it to file-scoped namespace, the generated snippet looks like this.

```{code-block} csharp
:caption: PriceCalculationStepDefinitions.cs
:emphasize-lines: 13, 25
namespace ReqnrollQuickstart.Specs.StepDefinitions;

[Binding]
public class PriceCalculationStepDefinitions
{
    [Given("the client started shopping")]
    public void GivenTheClientStartedShopping()
    {
        throw new PendingStepException();
    }

    [Given("the client added {int} pcs of {string} to the basket")]
    public void GivenTheClientAddedPcsOfToTheBasket(int quantity, string product)
    {
        throw new PendingStepException();
    }

    [When("the basket is prepared")]
    public void WhenTheBasketIsPrepared()
    {
        throw new PendingStepException();
    }

    [Then("the basket price should be ${float}")]
    public void ThenTheBasketPriceShouldBe(decimal expectedPrice)
    {
        throw new PendingStepException();
    }
}
```

As you can see, each step in our scenario has a corresponding method, called a *step definition method*. These are currently unfinished, but guide us to provide the necessary automation code to verify our application. You can find more information about step definitions in the [](../automation/step-definitions.md) guide.

```{note}
After adding step definitions or changing their expressions, you have to build the project in Visual Studio, so that the changes are shown in the feature file editor.
```

## Prepare fields for the step definitions

Let's provide the automation code. First, let's declare a few class-level fields.

```{code-block} csharp
:caption: PriceCalculationStepDefinitions.cs
:emphasize-lines: 6-8
namespace ReqnrollQuickstart.Specs.StepDefinitions;

[Binding]
public class PriceCalculationStepDefinitions
{
    private readonly PriceCalculator _priceCalculator = new();
    private readonly Dictionary<string, int> _basket = new();
    private decimal _calculatedPrice;
    
    [...]
}
```

These fields will be used for different purposes:

* The field `_priceCalculator` contains the module class that we would like to test.
* The `_basket` field will be used to collect the item/s the client puts in the basket, an item is a pair of product and quantity.
* The `_calculatedPrice` field will contain the price calculated by the application, so that we can make assertions for it.

These fields will provide data (or with other word *state*) for the step definitions. For now, all our step definition methods were in the same class, therefore declaring them as simple class-level fields was enough. For learning more about sharing data between step definition methods please check the [](../automation/sharing-data-between-bindings.md) guide.

## Automate steps

Now let's provide the automation code for the steps. Our plan is the following:

* In the "Given" steps we will prepare the items in the basket, 
* in the "When" step we invoke the `CalculatePrice` method of our price calculator class and save the result, and
* in the "Then" step we make sure that the saved price is the same as what we expected using an assertion.

After adding all these, our code looks like this (changes emphasized):

```{code-block} csharp
:caption: PriceCalculationStepDefinitions.cs
:emphasize-lines: 13-14, 20, 26, 32
namespace ReqnrollQuickstart.Specs.StepDefinitions;

[Binding]
public class PriceCalculationStepDefinitions
{
    private readonly PriceCalculator _priceCalculator = new();
    private readonly Dictionary<string, int> _basket = new();
    private decimal _calculatedPrice;

    [Given("the client started shopping")]
    public void GivenTheClientStartedShopping()
    {
        _basket.Clear();
        _calculatedPrice = 0.0m;
    }

    [Given("the client added {int} pcs of {string} to the basket")]
    public void GivenTheClientAddedPcsOfToTheBasket(int quantity, string product)
    {
        _basket.Add(product, quantity);
    }

    [When("the basket is prepared")]
    public void WhenTheBasketIsPrepared()
    {
        _calculatedPrice = _priceCalculator.CalculatePrice(_basket);
    }

    [Then("the basket price should be ${float}")]
    public void ThenTheBasketPriceShouldBe(decimal expectedPrice)
    {
        Assert.AreEqual(expectedPrice, _calculatedPrice);
    }
}
```

```{note}
In our example we call methods of our application code from the step definitions. In other projects, you might need to invoke REST HTTP requests there or interact with a web browser in the step definitions. Reqnroll does not prescribe any particular automation model.
```

## Run tests and implement application code

We seem to have completed our automation code, still if we run our tests it shows an error.

```{code-block} text
:caption: Test Output
Test method ReqnrollQuickstart.Specs.Features.PriceCalculationFeature.ClientHasASimpleBasket threw exception: 
System.NotImplementedException: The method or operation is not implemented.
```

```{hint}
What we have done so far was a *test-first development* that might be known to you from Test-Driven Development (TDD). We automated the scenario (the "test") before implementing the production code. You can use Reqnroll for "test-after" development as well, but we encourage you to try test-first, because the automated tests can help to shape the implementation and can help to avoid unnecessary or unused code.
```

Our test fails, because we haven't implemented the price calculation module yet.

In our case it would be easy to implement the "final" version of the calculation module immediately, but currently our scenario illustrates a very simple case, when we only add a single item of a product to the basket. For complex or complicated system the "final" solution that you have in your mind might not be the best one, so it is better to make it iteratively. Let's imagine that we have a complex system, and therefore we will start with a temporary, basic implementation for now.

Open the `PriceCalculator` class and add the emphasized lines from the code below.

```{code-block} csharp
:caption: PriceCalculator.cs
:emphasize-lines: 14-16
namespace ReqnrollQuickstart.App;

public class PriceCalculator
{
    // the item prices are hard coded for now
    private readonly Dictionary<string, decimal> _priceTable = new()
    {
        { "Electric guitar", 180.0m },
        { "Guitar pick", 1.5m }
    };

    public decimal CalculatePrice(Dictionary<string, int> basket)
    {
        //TODO: complete the price calculation once we defined more scenarios
        var item = basket.First();
        return _priceTable[item.Key];
    }
}
```

## Add a new scenario and extend code

So it is time to add a new scenario where the client has multiple items in the basket. The scenario can be drafted as:

```{code-block} gherkin
:caption: New scenario
Scenario: Client has multiple items in their basket
    Given the client started shopping
    And the client added
        | product         | quantity |
        | Electric guitar | 1        |
        | Guitar pick     | 10       |
    When the basket is prepared
    Then the basket price should be $195.0
```

*Where should we document this scenario?*

This scenario is also related to price calculation, so we should include it to our `PriceCalculation.feature` file, but let's look at the current structure of the file. You can notice that it also contains a `Rule` block. Rules are optional in Gherkin but they are very useful to group the scenarios by acceptance criteria. You can learn more about the `Rule` keyword in the [](../gherkin/gherkin-reference.md#rule) page.

Currently we have a single rule: "The price for a basket with items can be calculated based on the item prices" and it is clear that the new scenario also belongs to that, so we can just include it to the end of the rule block (that is in our case the end of the file). Later we might need to introduce additional rules, like applying discounts.

```{code-block} gherkin
:caption: PriceCalculation.feature
:emphasize-lines: 8-15
Feature: Price calculation
[...]
Rule: The price for a basket with items can be calculated based on the item prices

Scenario: Client has a simple basket
[...]

Scenario: Client has multiple items in their basket
    Given the client started shopping
    And the client added
        | product         | quantity |
        | Electric guitar | 1        |
        | Guitar pick     | 10       |
    When the basket is prepared
    Then the basket price should be $195.0
```

Visual Studio shows most of the steps of the new scenario with default font color, except the "And the client added" step. This is because all other steps have been already used in our other scenario as well, so we can automatically reuse the automation we provided for them. Great! But the "And the client added" step is still *undefined*. This is a special step as it contains an attached tabular parameter with the products and the quantities to be added to the basket. This parameter is called *Data Table* in Gherkin and you can read more about it in the [](../gherkin/gherkin-reference.md#data-tables) section of our Gherkin page.

```{tip}
You can easily find the step definition method of a *defined* step by invoking the *Go To Definition* command from the context menu of the step. And once you are at the step definition, the *Find Step Definition Usages* command shows where it was used.
```

Actually even undefined step could have been rephrased in a way that we use only existing steps. We could have written:

```{code-block} gherkin
:caption: Building basket with multiple items using existing steps
And the client added 1 pcs of "Electric guitar" to the basket
And the client added 10 pcs of "Guitar pick" to the basket
```

This way of phasing becomes cumbersome with multiple items. The one with the data table is nicer. But we need to define it still.

We can use the *Define Steps* dialog as before, but to extend an existing step definition class with a new snippet, you need to click on the *Copy to clipboard* button on the dialog and paste the snippet to our step definition class, for example right after the other "Given" step dealing with basket addition.

The content of the data table is provided as a parameter of type `DataTable`. We can rename the parameter to `itemsTable`.

```{code-block} csharp
:caption: PriceCalculationStepDefinitions.cs
:emphasize-lines: 12-16
[...]
public class PriceCalculationStepDefinitions
{
    [...]

    [Given("the client added {int} pcs of {string} to the basket")]
    public void GivenTheClientAddedPcsOfToTheBasket(int quantity, string product)
    {
        _basket.Add(product, quantity);
    }

    [Given("the client added")]
    public void GivenTheClientAdded(DataTable itemsTable)
    {
        throw new PendingStepException();
    }

    [When("the basket is prepared")]
    public void WhenTheBasketIsPrepared()
    {
        _calculatedPrice = _priceCalculator.CalculatePrice(_basket);
    }

    [...]
}
```

For handling data tables you can find more information in the [](../automation/step-definitions.md#data-table-or-docstring-arguments) section of the step definition guide. In this Quickstart we use one of the [](../automation/datatable-helpers.md) method to convert the table structure to a strongly typed structure (a list of tuples). 

```{code-block} csharp
:caption: PriceCalculationStepDefinitions.cs
:emphasize-lines: 9-13
[...]
public class PriceCalculationStepDefinitions
{
    [...]

    [Given("the client added")]
    public void GivenTheClientAdded(DataTable itemsTable)
    {
        var items = itemsTable.CreateSet<(string Product, int Quantity)>();
        foreach (var item in items)
        {
            _basket.Add(item.Product, item.Quantity);
        }
    }

    [...]
}
```

Let's run the tests now. As we expected, the first scenario still passes, but the new one fails, because our basic implementation of the calculator does not support this case yet.

```{code-block} text
:caption: Test Output
Assert.AreEqual failed. Expected:<195.0>. Actual:<180.0>. 
```

Now based on this example we can complete the calculation method.

```{code-block} csharp
:caption: PriceCalculator.cs
:emphasize-lines: 7-12
public class PriceCalculator
{
    [...]

    public decimal CalculatePrice(Dictionary<string, int> basket)
    {
        decimal price = 0;
        foreach (var item in basket)
        {
            price += _priceTable[item.Key] * item.Value;
        }
        return price;
    }
}
```

Now both of our tests pass!

## Next Steps

Congratulations! You have completed our Quickstart tutorial and now you have a working Reqnroll automation solution that you can experiment with.

If you get lost, you can check out our sample result in the [`completed` branch](https://github.com/reqnroll/Quickstart/tree/completed) of our Quickstart repository.

If you need inspirations how to extend the solution, here are a few ideas:

* Consider introducing a `Currency` class and create a argument transformation that recognizes currencies like `$195.0` and converts them to a currency value. You can find more about step argument transformations in [](../automation/step-argument-conversions.md).
* You can replace the hard-coded product prices with "Given" steps that describe the available products and their prices. You can also use the [](../gherkin/gherkin-reference.md#background) section for that.
* You can consider implementing a new rule that provides 10% discount when the basket value is over $200. Separate their scenarios with the [Rule keyword](../gherkin/gherkin-reference.md#rule).
* If you are really adventureous, you can turn the app into a backed service that provides the price calculation as a REST HTTP service. In this case that step definitions can make HTTP requests to test the service. In that case you can use the `BeforeScenario` and `AfterScenario` [hooks](../automation/hooks) to start and stop the application.

Share your results at our [Reqnroll discussion topic](https://github.com/reqnroll/Reqnroll/discussions/6)!
