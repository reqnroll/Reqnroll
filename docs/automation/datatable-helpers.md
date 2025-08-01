# DataTable Helpers

A number of helpers implemented as extension methods of the `DataTable` class make it easier to implement steps that accept a `DataTable` parameter. 

When  helper methods expect a generic type (usually denoted as `<T>` in the method signature), you can use:

- classes
- records (with C# 9)
- tuples

## `CreateInstance<T>`

The `CreateInstance<T>` extension method of the `DataTable` class will convert a table in your scenario to a single instance of a class. The class used to convert the table is specified by the generic type `T` in the method signature `CreateInstance<T>`. You can use two different table layouts in your scenarios with the `CreateInstance<T>` method.

- **Vertical Tables**

  A vertical table consists of two columns where values in the first column match property names, and values of the second column are the values assigned to those properties. The header row of the table is ignored. Header cells may be named to suit your use case.

  ```gherkin
  Given I entered the following data into the new account form:
      | Field                | Value             |
      | Name                 | John Galt         |
      | Birthdate            | 2/2/1902          |
      | Height In Inches     | 72                |
      | Bank Account Balance | 1234.56           |
  #     ^^^^^^^^^^^^^^^^^^^^   ^^^^^^^^^^^^^^^^^
  #        property names       property values
  ```
  
  This layout is desirable for tables containing many values making the vertical layout easier to read.

- **Horizontal Tables**

  A horizontal table consists of a header row where the header cells match property names, and subsequent data rows contain the values assigned to those properties. In order to convert a horizontal table to a single instance of a class, the table must only contain the header row and one data row.

  ```gherkin
  Given I entered the following data into the new account form:
      | Name      | Birthdate | Height In Inches | Bank Account Balance | # Header row (property names)
      | John Galt | 2/2/1902  | 72               | 1234.56              | # Data row (property values)
  ```

  This layout is desirable when the table does not require too many values. This helps save vertical space by consuming more horizontal space in your feature file.

  **Important: use the `CreateSet<T>` method described below to create a collection of objects if more than one data row is needed.**

Deciding to use a vertical or horizontal table layout is subjective. Choose the layout that is easiest to read given the information in the table.

Reqnroll matches table values to property names regardless of letter case. To Reqnroll, "BankAccount", "Bank Account", "BANK ACCOUNT" and "bank account" will all map to a property named `BankAccount`. More information on column naming is below.

### Using `CreateInstance` with a Class

You can map a table to a custom class you write. The following example will map the tables described above in the vertical or horizontal layouts. First, create the class:

```{code-block} csharp
:caption: C# File
// Class used to map table
class Account
{
    public string Name { get; set; }
    public int HeightInInches { get; set; }
    public decimal BankAccountBalance { get; set; }
}
```

Remember that property names should match values in the table in your scenario. Reqnroll requires properties to have both a public getter and a public setter. Most built-in .NET types are converted automatically. This includes the following types:

- `int` and `int?`
- `decimal` and `decimal?`
- `bool` and `bool?`
- `DateTime` and `DateTime?`

Plus [many more][supported-types].

The name of the class is put in place of the generic type `T` in the call to `CreateInstance<T>`. An example step definition is below.

```{code-block} csharp
:caption: Step Definition File
[Given(@"Given I entered the following data into the new account form:")]
public void GivenIEnteredTheFollowingDataIntoTheNewAccountForm(DataTable table)
{
    var account = table.CreateInstance<Account>();
    //                                 ^^^^^^^

    // account.Name is "John Galt"
    // account.HeightInInches is 72
    // account.BankAccountBalance is 1234.56
}
```

The `CreateInstance<T>` method will create the Account object and set properties according to what can be read from the table. Table cell values are strings by default. These strings are converted to the type specified for each property of the destination class. For example, the string `"1234.56"` in the table is converted to a decimal value before being assigned to the `BankAccountBalance` property.

### Using `CreateInstance` with ValueTuple

Alternatively you can use ValueTuples and destructuring:

```{code-block} csharp
:caption: Step Definition File
[Given(@"Given I entered the following data into the new account form:")]
public void GivenIEnteredTheFollowingDataIntoTheNewAccountForm(DataTable table)
{
    var account = table.CreateInstance<(string name, DateTime birthDate, int heightInInches, decimal bankAccountBalance)>();

    // account.name is "John Galt"
    // account.heightInInches is 72
    // account.bankAccountBalance is 1234.56
}
```

**Important:** In the case of tuples, _**you need to have the same number of parameters and types; parameter names do not matter**_, as ValueTuples do not hold parameter names at runtime using reflection.

**Scenarios with more than 7 properties are not currently supported when converting to ValueTuple, and you will receive an exception if you try to map more than 7 properties.**

The next section describes how to convert a horizontal table with more than one data row to a collection of objects.

## `FillInstance`

The `FillInstance` extension method of the `DataTable` class populates an existing object instance with values from a table. Unlike `CreateInstance<T>` which creates a new object, `FillInstance` takes an existing object and fills its properties with table data.

This method is useful when you:
- Already have an object instance that you want to populate
- Need to update an existing object with test data
- Want to reuse an object and modify its properties based on table values

### Basic Usage

The `FillInstance` method accepts any object and populates its properties based on the table structure. Like `CreateInstance<T>`, it supports both vertical and horizontal table layouts:

```{code-block} csharp
:caption: Step Definition File
[Given(@"I update the account with the following data:")]
public void GivenIUpdateTheAccountWithTheFollowingData(DataTable table)
{
    // Assume we already have an account instance
    var existingAccount = GetExistingAccount();
    
    // Fill the existing instance with table data
    table.FillInstance(existingAccount);
    
    // existingAccount properties are now updated with table values
}
```

### Using `FillInstance` with `InstanceCreationOptions`

The `FillInstance` method also supports `InstanceCreationOptions` to control how properties are populated:

```{code-block} csharp
:caption: Step Definition File
[Given(@"I update the account with the following data:")]
public void GivenIUpdateTheAccountWithTheFollowingData(DataTable table)
{
    var existingAccount = GetExistingAccount();
    
    var options = new InstanceCreationOptions 
    { 
        VerifyAllColumnsBound = true 
    };
    
    table.FillInstance(existingAccount, options);
}
```

For more information about `InstanceCreationOptions`, see the [InstanceCreationOptions section](#instancecreationoptions) below.

### Example with Vertical Table

```{code-block} gherkin
:caption: Feature File
Given I update the account with the following data:
    | Property             | Value     |
    | Name                 | Jane Doe  |
    | Bank Account Balance | 2500.75   |
```

### Example with Horizontal Table

```{code-block} gherkin
:caption: Feature File
Given I update the account with the following data:
    | Name     | Bank Account Balance |
    | Jane Doe | 2500.75              |
```

Both table formats will update the existing object's `Name` and `BankAccountBalance` properties with the specified values.

## `CreateSet<T>`

The `CreateSet<T>` extension method of the `DataTable` class converts the table into an enumerable set of objects. For example, assume you have the following step:

```gherkin
Given these products exist
    | Sku   | Name             | Price |
    | BOOK1 | Atlas Shrugged   | 25.04 |
    | BOOK2 | The Fountainhead | 20.15 |
```

And you want to map rows in that table to the following class:

```{code-block} csharp
:caption: C# File
public class Product
{
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

You can convert the table to a collection of Product objects in your step definition using `CreateSet<Product>()`:

```{code-block} csharp
:caption: Step Definition File
[Given(@"Given these products exist")]
public void GivenTheseProductsExist(DataTable table)
{
    var products = table.CreateSet<Product>();
    // ...
}
```

The `CreateSet<T>` method returns an `IEnumerable<T>` based on the matching data in the table. It contains the values for each object, making appropriate type conversions from string to the related property. Column headers are matched to property names in the same way as `CreateInstance<T>`.

## `InstanceCreationOptions`
The `CreateInstance<T>` and `CreateSet<T>` methods have an overload with `InstanceCreationOptions` to modify creation behavior.

```{code-block} csharp
:caption: InstanceCreationOptions
public class InstanceCreationOptions
{
    public bool VerifyAllColumnsBound { get; set; }
    public bool RequireTableToProvideAllConstructorParameters { get; set; }
}
```

### `VerifyAllColumnsBound`
Default is `false`. If `true`, check if every column is bound. If not, it throws a `ColumnCouldNotBeBoundException`.

For example, given the class:

```{code-block} csharp
:caption: C# Test object
public class Person
{
    public string FirstName { get; set;}  
    public string LastName { get; set; }
}
```

And we feed a table with 'Name' as 'Mike':

```{code-block} csharp
:caption: C# Call
table.CreateInstance<Person>(new InstanceCreationOptions { VerifyAllColumnsBound = true });
```

We get a `ColumnCouldNotBeBoundException` with the message "Member or field Name not found."

Note: It won't check the other way around, e.g., we don't get an exception because FirstName is not in the table.

### `VerifyCaseInsensitive`
Default is `false`. If `true`, checking if columns are bound becomes case-insensitive. Active *only* if `VerifyAllColumnsBound` is also `true`.

For example, given the class:

```{code-block} csharp
:caption: C# Test object
public class Person
{
    public string FirstName { get; set;}  
    public string LastName { get; set; }
}
```

And we feed a table with 'firstName' as 'Mike' and 'lastName' as 'Smith':

```{code-block} csharp
:caption: C# Call
table.CreateInstance<Person>(new InstanceCreationOptions { VerifyAllColumnsBound = true, VerifyCaseInsensitive = true });
```

We do not get an exception because the table fields match the class members, ignoring case. Behaviour for invalid and missing members in the table remain unchanged when compared to `VerifyAllColumnsBound` alone.


### `RequireTableToProvideAllConstructorParameters`
Default is `false`. If `true`, use the constructor that exactly matches the table. 
If a matching constructor cannot be found, the helper will throw a `MissingMethodException`. If there are multiple candidates that take all members, prefer the constructor with the fewest parameters.

If `false`, the constructor with the most parameters is preferred.

For example:

```{code-block} csharp
:caption: C# Test object
public record Person(string FirstName, string LastName, int Age);
```

If `RequireTableToProvideAllConstructorParameters` is `false`, `table.CreateInstance<Person>` will succeed and Age will be `0`. 
If `RequireTableToProvideAllConstructorParameters` is `true`, we get a `System.MissingMethodException` with the message "Unable to find a suitable constructor to create an instance of ..."

## `CompareToInstance<T>`

The `CompareToInstance<T>` extension method of the `DataTable` class makes it easy to compare the properties of an object to the table in your scenario. For example, you have a class like this:

```{code-block} csharp
:caption: C# File
public class Person
{
    public string FirstName { get; set;}  
    public string LastName { get; set; }
    public int YearsOld { get; set; }
}
```

You want to compare it to a table in a step like this:

```{code-block} gherkin
:caption: Feature File
Then the person should have the following values
    | Field      | Value |
    | First Name | John  |
    | Last Name  | Galt  |
    | Years Old  | 54    |
```
  
You can assert that the properties match with this simple step definition:

```{code-block} csharp
:caption: Step Definition File
[Binding]
public class PersonSteps
{
    ScenarioContext _scenarioContext;

    public PersonSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then("the person should have the following values")]
    public void ThenThePersonShouldHaveTheFollowingValues(DataTable table){
        // you don't have to get person this way, this is just for demonstration purposes
        var person = _scenarioContext.Get<Person>();

        table.CompareToInstance<Person>(person);
    }
}
```

If FirstName is not "John", LastName is not "Galt", or YearsOld is not 54, a descriptive error showing the differences is thrown.

If the values match, no exception is thrown, and Reqnroll continues to process your scenario.

## `CompareToSet<T>`

The `CompareToSet<T>` extension method of the `DataTable` class works similarly to `CompareToInstance<T>`, except it compares a collection of objects. For example, you have a class like this:

```{code-block} csharp
:caption: C# File
public class Account
{
    public string Id { get; set;}
    public string FirstName { get; set;}
    public string LastName { get; set;}
    public string MiddleName { get; set;}
}
```

You want to test that your system returns a specific set of accounts, like this:

```{code-block} gherkin
:caption: Feature File

Then I get back the following accounts
    | Id | First Name | Last Name |
    | 1  | John       | Galt      |
    | 2  | Howard     | Roark     |
```

You can test your results with one call to CompareToSet<T>:

```{code-block} csharp
:caption: Step Definition File
[Binding]
public class AccountSteps
{
    ScenarioContext _scenarioContext;

    public AccountSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then("I get back the following accounts")]
    public void ThenIGetBackTheFollowingAccounts(DataTable table)
    {
        // (or get the accounts from the database or web service)
        var accounts = _scenarioContext.Get<IEnumerable<Account>>();

        table.CompareToSet<Account>(accounts);
    }
}
```

In this example, `CompareToSet<T>` checks that two accounts are returned, and only tests the properties you defined in the table. **It does not test the order of the objects, only that one was found that matches.**  If no record matching the properties in your table is found, an exception is thrown that includes the row number(s) that do not match up.

### Comparing Sets When Order Matters

In use cases where the order should match, pass `true` as the second argument to `CompareToSet`:

```{code-block} csharp
:caption: Step Definition File
table.CompareToSet<Account>(accounts, true);
//                                    ^^^^
```

In addition to throwing an exception if property values do not match, Reqnroll will throw an exception if the order of the accounts doesn't match your expectations. This is useful when the order of things is determined by business rules, or in use cases like search results.

## Column naming

The Reqnroll DataTable helpers use the values in your table to determine what properties to set in your object. However, the names of the columns do not need to match exactly - whitespace and casing is ignored. For example, the following two tables are treated as identical:

```{code-block} gherkin
:caption: Feature File
| FirstName | LastName | DateOfBirth | HappinessRating |
```

```{code-block} gherkin
:caption: Feature File
| First name | Last name | Date of birth | HAPPINESS rating |
```

This allows you to make your tables more readable to others.

## Aliasing

If you have properties in your objects that are known by different terms within the business domain, these can be Aliased in your model by applying the attribute `TableAliases`. This attribute takes a collection of aliases as regular expressions that can be used to refer to the property in question.

For example, if you have an object representing an Employee, you might want to alias the `Surname` property:

```{code-block} csharp
:caption: C# File
public class Employee
{
    public string FirstName { get; set; }
    public string MiddleName { get; set; }

    [TableAliases("Last[]?Name", "Family[]?Name")]
    public string Surname { get; set; }
}
```

Test writers can then refer to this property as "Surname", "Last Name", "Lastname", "Family Name" or "FamilyName", and it will still be mapped to the correct column in your scenario.

The `TableAliases` attribute can be applied to a field, a property as a single attribute with multiple regular expressions, or as multiple attributes, depending on your preference.

## Extensions

Out-of-the-box, the Reqnroll table helpers knows how to handle most C# base types. Types like `String`, `Bool`, `Enum`, `Int`, `Decimal`, `DateTime`, etc. are all covered (see [full list of supported times][supported-types]). If you want to cover more types, including your own custom types, you can do so by registering your own instances of `IValueRetriever` and `IValueComparer`.

For example, you have a complex object like this:

```{code-block} csharp
:caption: C# File
public class Shirt
{
    public string Name { get; set; }
    public Color Color { get; set; }
}
```

You have a table like this:

```{code-block} gherkin
:caption: Feature File
| Name | Color |
| XL   | Blue  |
| L    | Red   |
```

If you want to map `Blue` and `Red` to the appropriate instance of the `Color` class, you need to create an instance of `IValueRetriever` that can convert the strings to the `Color` instance.

You can register your custom `IValueRetriever` (and/or an instance of `IValueComparer` if you want to compare colors) like this:

```{code-block} csharp
:caption: Step Definition File
[Binding]
public static class Hooks1
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Service.Instance.ValueRetrievers.Register(new ColorValueRetriever());
        Service.Instance.ValueComparers.Register(new ColorValueComparer());
    }
}
```

Examples on implementing these interfaces can be found as follows:

- [IValueRetriever](https://github.com/reqnroll/Reqnroll/tree/main/Reqnroll/Assist/ValueRetrievers)
- [IValueComparer](https://github.com/reqnroll/Reqnroll/tree/main/Reqnroll/Assist/ValueComparers)

### Configuration

Some built in classes support configuration to adjust the default behaviour.

- [DateTimeValueRetriever](https://github.com/reqnroll/Reqnroll/tree/main/Reqnroll/Assist/ValueRetrievers/DateTimeValueRetriever.cs) and [DateTimeOffsetValueRetriever](https://github.com/reqnroll/Reqnroll/tree/main/Reqnroll/Assist/ValueRetrievers/DateTimeOffsetValueRetriever.cs) have a static DateTimeStyles property to adjust the style used to parse date times.

Example of usage:

```{code-block} csharp
:caption: Hook File
[Binding]
public static class Hooks1
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        DateTimeValueRetriever.DateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
    }
}
```

### `NullValueRetriever`

```{note}
If you are not looking to transform data from `DataTable` objects, but rather looking to transform values in your step definitions, you'll likely want to look at [Step Argument Conversions](step-argument-conversions) instead.
```

By default, non-specified (empty string) values are considered:

- An empty string for `String` and `System.Uri` values
- A null value for `Nullable<>` primitive types
- An error for non-nullable primitive types

To specify null values explicitly, add a `NullValueRetriever` to the set of registered retrievers, specifying the text to be treated as a null value, e.g.:

```{code-block} csharp
:caption: Hook File
[Binding]
public static class Hooks1
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Service.Instance.ValueRetrievers.Register(new NullValueRetriever("<null>"));
    }
}
```

```{note}
The comparison is case-insensitive.
```

## Using LINQ-based instance and set comparison

The **CompareToSet** extension method only checks for equivalence of collections which is a reasonable default. The **Reqnroll.Assist** namespace also contains extension methods for With based operations.

Consider the following steps:

```{code-block} gherkin
:caption: Feature File

Scenario: Matching music collections
    When I have a music collection
        | Artist     | Album       |
        | Beatles    | Rubber Soul |
        | Pink Floyd | Animals     |
        | Muse       | Absolution  |
    Then it should match
        | Artist     | Album       |
        | Beatles    | Rubber Soul |
        | Pink Floyd | Animals     |
        | Muse       | Absolution  |
    And it should match
        | Artist     | Album       |
        | Beatles    | Rubber Soul |
        | Muse       | Absolution  |
        | Pink Floyd | Animals     |
    And it should exactly match
        | Artist     | Album       |
        | Beatles    | Rubber Soul |
        | Pink Floyd | Animals     |
        | Muse       | Absolution  |
    But it should not match
        | Artist  | Album       |
        | Beatles | Rubber Soul |
        | Queen   | Jazz        |
        | Muse    | Absolution  |
    And it should not match
        | Artist  | Album       |
        | Beatles | Rubber Soul |
        | Muse    | Absolution  |
    And it should not exactly match
        | Artist     | Album       |
        | Beatles    | Rubber Soul |
        | Muse       | Absolution  |
        | Pink Floyd | Animals     |
```

With LINQ-based operations each of the above comparisons can be expressed using a single line of code:

```{code-block} csharp
:caption: Step Definition File

[Binding]
public class MusicCollectionSteps
{
    ScenarioContext _scenarioContext;

    public MusicCollectionSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When(@"I have a music collection")]
    public void WhenIHaveAMusicCollection(DataTable table)
    {
        var collection = table.CreateSet<Item>();

        _scenarioContext.Add("Collection", collection);
    }

    [Then(@"it should match")]
    public void ThenItShouldMatch(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;
    
        Assert.IsTrue(table.RowCount == collection.Count() && table.ToProjection<Item>().Except(collection.ToProjection()).Count() == 0);
    }

    [Then(@"it should exactly match")]
    public void ThenItShouldExactlyMatch(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;

        Assert.IsTrue(table.ToProjection<Item>().SequenceEqual(collection.ToProjection()));
    }

    [Then(@"it should not match")]
    public void ThenItShouldNotMatch(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;
    
        Assert.IsFalse(table.RowCount == collection.Count() && table.ToProjection<Item>().Except(collection.ToProjection()).Count() == 0);
    }

    [Then(@"it should not exactly match")]
    public void ThenItShouldNotExactlyMatch(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;
    
        Assert.IsFalse(table.ToProjection<Item>().SequenceEqual(collection.ToProjection()));
    }
}
```

In a similar way we can implement containment validation:

```{code-block} gherkin
:caption: Feature File

Scenario: Containment
    When I have a music collection
        | Artist     | Album       |
        | Beatles    | Rubber Soul |
        | Pink Floyd | Animals     |
        | Muse       | Absolution  |
    Then it should contain all items
        | Artist  | Album       |
        | Beatles | Rubber Soul |
        | Muse    | Absolution  |
    But it should not contain all items
        | Artist  | Album       |
        | Beatles | Rubber Soul |
        | Muse    | Resistance  |
    And it should not contain any of items
        | Artist  | Album      |
        | Beatles | Abbey Road |
        | Muse    | Resistance |
```

```{code-block} csharp
:caption: Step Definition File

[Binding]
public class MusicCollectionSteps
{
    ScenarioContext _scenarioContext;

    public MusicCollectionSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then(@"it should contain all items")]
    public void ThenItShouldContainAllItems(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;
    
        Assert.IsTrue(table.ToProjection<Item>().Except(collection.ToProjection()).Count() == 0);
    }

    [Then(@"it should not contain all items")]
    public void ThenItShouldNotContainAllItems(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;
    
        Assert.IsFalse(table.ToProjection<Item>().Except(collection.ToProjection()).Count() == 0);
    }

    [Then(@"it should not contain any of items")]
    public void ThenItShouldNotContainAnyOfItems(DataTable table)
    {
        var collection = _scenarioContext["Collection"] as IEnumerable<Item>;
    
        Assert.IsTrue(table.ToProjection<Item>().Except(collection.ToProjection()).Count() == table.RowCount);
    }
}
```

What if Artist and Album are properties of different entities? Look at this piece of code:

```{code-block} csharp
:caption: C# File

var collection = from artist in ctx.Artists
                 where artist.Name == "Muse"
                 join album in ctx.Albums
                     on album.ArtistId equals artist.ArtistId
                 select new
                 {
                     Artist = artist.Name,
                     Album = album.Name
                 };
```

The **Reqnroll.Assist** namespace has a generic class named `EnumerableProjection<T>`. If a type `T` is known at compile time, the `ToProjection` method converts a table or a collection into an instance of `EnumerableProjection`:

```{code-block} csharp
:caption: C# File
table.ToProjection<Item>();
```

But if we need to compare a table with the collection of anonymous types from the example above, we need to express this type in some way so ToProjection will be able to build an instance of specialized `EnumerableProjection`. This is done by sending a collection as an argument to **ToProjection**. And to support both sets and instances and avoid naming ambiguity, corresponding methods are called **ToProjectionOfSet** and **ToProjectionOfInstance**:

```{code-block} csharp
:caption: C# File
table.ToProjectionOfSet(collection);
table.ToProjectionOfInstance(instance);
```

Here are the definitions of Reqnroll DataTable extensions methods that convert tables and collections of IEnumerables to EnumerableProjection:

```{code-block} csharp
:caption: C# File
public static IEnumerable<Projection<T>> ToProjection<T>(this IEnumerable<T> collection, DataTable table = null)
{
    return new EnumerableProjection<T>(table, collection);
}

public static IEnumerable<Projection<T>> ToProjection<T>(this DataTable table)
{
    return new EnumerableProjection<T>(table);
}

public static IEnumerable<Projection<T>> ToProjectionOfSet<T>(this DataTable table, IEnumerable<T> collection)
{
    return new EnumerableProjection<T>(table);
}

public static IEnumerable<Projection<T>> ToProjectionOfInstance<T>(this DataTable table, T instance)
{
    return new EnumerableProjection<T>(table);
}
```

Note that last arguments of `ToProjectionOfSet` and `ToProjectionOfInstance` methods are not used in method implementation. Their only purpose is to bring information about `T`, so the `EnumerableProjection` adapter class can be built properly. Now we can perform the following comparisons with anonymous types collections and instances:

```{code-block} csharp
:caption: C# Test File
[Test]
public void Table_with_subset_of_columns_with_matching_values_should_match_collection()
{
    var table = CreateTableWithSubsetOfColumns();
    
    table.AddRow(1.ToString(), "a");
    table.AddRow(2.ToString(), "b");

    var query = from x in testCollection
                select new { x.GuidProperty, x.IntProperty, x.StringProperty };

    Assert.AreEqual(0, table.ToProjectionOfSet(query).Except(query.ToProjection()).Count());
}

[Test]
public void Table_with_subset_of_columns_should_be_equal_to_matching_instance()
{
    var table = CreateTableWithSubsetOfColumns();
    
    table.AddRow(1.ToString(), "a");

    var instance = new { IntProperty = testInstance.IntProperty, StringProperty = testInstance.StringProperty };

    Assert.AreEqual(table.ToProjectionOfInstance(instance), instance);
}
```

[supported-types]: https://github.com/reqnroll/Reqnroll/tree/main/Reqnroll/Assist/ValueRetrievers
