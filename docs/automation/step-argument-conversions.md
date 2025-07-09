# Step Argument Conversions

[Step definitions](step-definitions) can use parameters to make them reusable for similar steps. The parameters are taken from either the step's text or from the values in additional examples. These arguments are provided as either strings or `Reqnroll.DataTable` instances.

To avoid cumbersome conversions in the step binding methods, Reqnroll can perform an automatic conversion from the arguments to the parameter type in the binding method. All conversions are performed using the culture of the feature file, unless the [binding setting of the language section](../installation/configuration) is defined in your `reqnroll.json` configuration file (see [](../gherkin/feature-language)). The following conversions can be performed by Reqnroll (in the following precedence):

* no conversion, if the argument is an instance of the parameter type (e.g. the parameter type is `object` or `string`)
* step argument transformation
* standard conversion

## Step Argument Transformation

```{note}
Step argument transformations don't support [Cucumber Expressions](cucumber-expressions); use Regular Expressions (regex)
```

Step argument transformations can be used to apply a custom conversion step to the arguments in step definitions. The step argument transformation is a method that converts from text (specified by a regular expression) or a `DataTable` instance to an arbitrary .NET type.

A step argument transformation is used to convert an argument if:

* The return type of the transformation is the same as the parameter type
* The regular expression (if specified) matches the original (string) argument

```{note}
If multiple matching transformations are available, a warning is output in the trace and the first transformation is used.
```

The following example transforms a relative period of time (`in 3 days`) into a `DateTime` structure.

```{code-block} csharp
:caption: C# File
[Binding]
public class Transforms
{
   [StepArgumentTransformation(@"in (\d+) days?")]
   public DateTime InXDaysTransform(int days)
   {
      return DateTime.Today.AddDays(days);
   }
}
```

The following example transforms any string input (no regex provided) into an `XmlDocument`.

```{code-block} csharp
:caption: C# File
[Binding]
public class Transforms
{
    [StepArgumentTransformation]
    public XmlDocument XmlTransform(string xml)
    {
       XmlDocument result = new XmlDocument();
       result.LoadXml(xml);
       return result;
    }
}
```

The following example transforms a table argument into a list of `Book` entities (using the [Reqnroll Assist Helpers](datatable-helpers)).  

```{code-block} csharp
:caption: C# File
[Binding]
public class Transforms
{
    [StepArgumentTransformation]
    public IEnumerable<Book> BooksTransform(DataTable booksTable)
    {
       return booksTable.CreateSet<Books>();
    }
}
```

By default, selection among matching step argument transformations is undeterministic.
To specify selection order, use the `Order` property in the `StepArgumentTransformation` attribute, where the transformation with lower numbers takes precedence. 
If no order is specified, the default value is 10000.

The following example transforms a string argument to a Rating model. If regex matches the expression, the given rating score will be parsed. Otherwise, the default rating will be used.

```{code-block} csharp
:caption: C# File
[Binding]
public class Transforms
{
    [StepArgumentTransformation(@"with (\d+) score", Order = 1)]
    public Rating RatingTransformation(int score)
    {
      return new Rating(score);
    }
    
    [StepArgumentTransformation]
    public Rating GlobalRatingTransformation(string input) 
    {
        return Rating.DefaultRating;
    }
}

public record Rating(int Value) 
{
    public static Rating DefaultRating => new Rating(50);
}
```

## Standard Conversion

A standard conversion is performed by Reqnroll in the following cases:

* The argument can be converted to the parameter type using `Convert.ChangeType()`
* The parameter type is an `enum` type and the (string) argument is an enum value
* The parameter type is `Guid` and the argument contains a full GUID string or a GUID string prefix. In the latter case, the value is filled with trailing zeroes.
