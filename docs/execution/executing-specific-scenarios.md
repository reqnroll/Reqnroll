# Executing Specific Scenarios

Executing a subset or only specific scenarios might be important locally and on the build pipeline.

Reqnroll converts the tags in your feature files to test case categories:

- NUnit: Category or TestCategory
- MSTest: TestCategory
- xUnit: Trait (similar functionality, Reqnroll will insert a Trait attribute with `Category` name)

This category can be used to filter the test execution in your build pipeline. 

```{note}
Incorrect filter can lead to no test getting executed.
```

You don't have to include the `@` prefix in the filter expression.

Learn more about the filters in Microsoft's [official documentation](https://docs.microsoft.com/en-us/dotnet/core/testing/selective-unit-tests?pivots=xunit).

## Examples

All the examples here are using `TestCategory`, but if you are using `xUnit` then you should use `Category` instead.

### How to use the filters

Below are 2 scenarios where one of them has a tag: `@done`, and the other one does not have a tag.

```{code-block} gherkin
:caption: Feature File

Feature: Breakfast

@done
Scenario: Eating cucumbers
  Given there are 12 cucumbers
  When I eat 5 cucumbers
  Then I should have 7 cucumbers

Scenario: Use all the sugar
  Given there is some sugar in the cup
  When I put all the sugar to my coffee
  Then the cup is empty
```

If we would like to run only the scenario with `@done` tag, then the filter should look like:

```bash
TestCategory=done
```

---

Below are 2 scenarios where one of them has a tag: `@done`, and the other one has `@automated`.

```{code-block} gherkin
:caption: Feature File

Feature: Breakfast

@done
Scenario: Eating cucumbers
  Given there are 12 cucumbers
  When I eat 5 cucumbers
  Then I should have 7 cucumbers

@automated
Scenario: Use all the sugar
  Given there is some sugar in the cup
  When I put all the sugar to my coffee
  Then the cup is empty
```

If we would like to run scenarios which have either `@done` or `@automated`:

```bash
TestCategory=done|TestCategory=automated
```

---

Below are 2 scenarios where one of them has a tag: `@done`, and the other one has `@automated`. There is also a `@US123` tag at Feature level.

```{code-block} gherkin
:caption: Feature File

@US123
Feature: Breakfast

@done
Scenario: Eating cucumbers
  Given there are 12 cucumbers
  When I eat 5 cucumbers
  Then I should have 7 cucumbers

@automated
Scenario: Use all the sugar
  Given there is some sugar in the cup
  When I put all the sugar to my coffee
  Then the cup is empty
```

If we would like to run only those scenarios, which have both `@US123` and `@done`:

```bash
TestCategory=US123&TestCategory=done
```

Below are 2 scenarios where one of them has two tags: `@done` and `@important`. There is another scenario, which has the `@automated` tag, and there is a `@us123` tag at Feature level.

```{code-block} gherkin
:caption: Feature File

@US123
Feature: Breakfast

@done @important
Scenario: Eating cucumbers
  Given there are 12 cucumbers
  When I eat 5 cucumbers
  Then I should have 7 cucumbers

@automated
Scenario: Use all the sugar
  Given there is some sugar in the cup
  When I put all the sugar to my coffee
  Then the cup is empty
```

If we would like to run only those scenarios, which have both `@done` and `@important`:

```bash
TestCategory=done&TestCategory=important
```

### dotnet test

Use the `--filter` command-line option:

```bash
dotnet test --filter TestCategory=done
```

```bash
dotnet test --filter "TestCategory=us123&TestCategory=done"
```

```bash
dotnet test --filter "TestCategory=done|TestCategory=automated"
```

### vstest.console.exe

Use the `/TestCaseFilter` command-line option:

```bash
vstest.console.exe "C:\Temp\BookShop.AcceptanceTests.dll" /TestCaseFilter:"TestCategory=done"
```

```bash
vstest.console.exe "C:\Temp\BookShop.AcceptanceTests.dll" /TestCaseFilter:"TestCategory=us123&TestCategory=done"
```

```bash
vstest.console.exe "C:\Temp\BookShop.AcceptanceTests.dll" /TestCaseFilter:"TestCategory=done|TestCategory=automated"
```

### Azure DevOps - Visual Studio Test task

The filter expression should be provided in the "Test filter criteria" setting in the `Visual Studio Test` task:

![Visual Studio Test task](/_static/images/task_filter1.png)

![Visual Studio Test task](/_static/images/task_filter2.png)

### Azure DevOps - .NET task

Alternatively you could use the dotnet task (DotNetCoreCLI) to run your tests. This works on all kinds of build agents:

```yaml
- task: DotNetCoreCLI@2
  displayName: 'dotnet test'
  inputs:
    command: test
    projects: 'BookShop.AcceptanceTests'
    arguments: '--filter "TestCategory=done"'
```

```yaml
- task: DotNetCoreCLI@2
  displayName: 'dotnet test'
  inputs:
    command: test
    projects: 'BookShop.AcceptanceTests'
    arguments: '--filter "TestCategory=us123&TestCategory=done"'
```
