# Sharing Data between Bindings

In many cases, different bindings need to exchange data during execution. Reqnroll provides several ways of sharing data between bindings.

## Instance Fields

If the binding is an instance method, Reqnroll creates a new instance of the containing class for every scenario execution. Following the [entity-based step organization rule](https://github.com/cucumber/cucumber/wiki/Step-Organisation), defining instance fields in the binding classes is an efficient way of sharing data between different steps of the same scenario that are related to the same entity. 

The following example saves the result of the MVC action to an instance field in order to make assertions for it in a "then" step.

```{code-block} csharp
:caption: Step Definition File

[Binding]
public class SearchSteps
{
    private ActionResult actionResult;

    [When(@"I perform a simple search on '(.*)'")]
    public void WhenIPerformASimpleSearchOn(string searchTerm)
    {
        var controller = new CatalogController();
        actionResult = controller.Search(searchTerm);
    }

    [Then(@"the book list should exactly contain book '(.*)'")]
    public void ThenTheBookListShouldExactlyContainBook(string title)
    {
         var books = actionResult.Model<List<Book>>();
         CustomAssert.Any(books, b => b.Title == title);
    }
}
```

## Context Injection

Reqnroll supports a very simple dependency framework that is able to instantiate and inject class instances for the scenarios. With this feature you can group the shared state to context-classes, and inject them into every binding class that is interested in that shared state.

See more about this feature in the [Context Injection](context-injection) page.

The following example defines a context class to store referred books. The context class is injected to a binding class.

```{code-block} csharp
:caption: C# File

public class CatalogContext
{
    public CatalogContext()
    {
        ReferenceBooks = new ReferenceBookList();
    }

    public ReferenceBookList ReferenceBooks { get; set; }
}
```

```{code-block} csharp
:caption: Step Definition File

[Binding]
public class BookSteps
{
    private readonly CatalogContext _catalogContext;

    public BookSteps(CatalogContext catalogContext)
    {
        _catalogContext = catalogContext;
    }

    [Given(@"the following books")]
    public void GivenTheFollowingBooks(DataTable table)
    {
        foreach (var book in table.CreateSet<Book>())
        {
            SaveBook(book);
            _catalogContext.ReferenceBooks.Add(book.Id, book);
        }
    }
}
```

## ScenarioContext and FeatureContext

Reqnroll provides two context instances. 

The [ScenarioContext](scenario-context) is created for each individual scenario execution and it is disposed when the scenario execution has been finished.

The [FeatureContext](feature-context) is created when the first scenario is executed from a feature and disposed when the execution of the feature's scenarios ends. In the rare case, when you need to preserve state in the context of a feature, the `FeatureContext` instance can be used as a property bag. 

## Static Fields

Generally, using static fields can cause synchronization and maintenance issues and makes the unit testability hard. As the Reqnroll tests are executed synchronously and people usually don't write unit tests for the tests itself, these arguments are just partly valid for binding codes. 

In some cases sharing a state through static fields can be an efficient solution.
