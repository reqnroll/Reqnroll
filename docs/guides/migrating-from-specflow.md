# Migrating from SpecFlow

Reqnroll has been created based on the open-source codebase of SpecFlow, therefore it provides a high level of compatibility with SpecFlow. We can generally say that everything that has worked with SpecFlow also works with Reqnroll, but some names and the namespaces have been modified. 

The key differences between SpecFlow and Reqnroll are the following:

* All packages have been renamed from `SpecFlow.*` to `Reqnroll.*`. E.g. `Reqnroll.MsTest`.
* The namespace of the classes has been changed from `TechTalk.SpecFlow` to `Reqnroll` and some classes that had SpecFlow in their name (e.g. `ISpecFlowOutputHelper`) have been renamed accordingly. An optional *SpecFlow Compatibility Package* has been created to migrate without changing all namespaces, see below.
* There is a new `DataTable` alias for the `Table` class to better match Gherkin terminology. The `Table` class can still be used.
* The main extension methods of the *Assist helpers* have been moved to the `Reqnroll` namespace, so that they can be used without an additional namespace using statement. The helpers are now referred to as [](../automation/datatable-helpers.md).
* The namespace of `IObjectContainer` has been changed from `BoDi` to `Reqnroll.BoDi`. If you have used special customizations that required to access the dependency injection container directly, you might need to update the namespace usings.
* The [Reqnroll Visual Studio extension](../installation/setup-ide.md#setup-visual-studio-2022) has been reworked in a way that it can handle both SpecFlow and Reqnroll projects (also for .NET 8.0).
* The integration plugins that have been managed by SpecFlow have been also ported to work with Reqnroll (e.g. `Reqnroll.Autofac`). See [](../integrations/available-plugins.md).
* The "SpecFlow.Actions" plugins that provide a ready-to-use support for different automation technologies (e.g. Selenium) are ported as `Reqnroll.SpecFlowCompatibility.Actions.*` packages (e.g. `Reqnroll.SpecFlowCompatibility.Actions.Selenium`). 

This article provides you a step-by-step guidance to migrate SpecFlow projects to Reqnroll. There are two migration paths you can choose from:

1. [](#specflow-compatibility-package): requires minimal change in the codebase.
2. [](#with-namespace-changes): requires simple changes, mostly doable with search-and-replace.

It is also worth mentioning that Reqnroll is based on the SpecFlow v4 codebase, so if you migrate from SpecFlow v3, you should consider the [](#breaking-changes-specflow-v3) section as well.

```{tip}
It is recommended to upgrade directly to the latest version of Reqnroll (e.g. v2.0), as we keep improving the migration experience and fix issues that might block migration.
```

```{attention} Living Documentation Support
The SpecFlow+ LivingDoc was part of the closed source implementation of SpecFlow and therefore we could not take it over for Reqnroll. We are currently in a process of re-building the tool (or something similar to that), but with a workaround you can also use Reqnroll with the SpecFlow Living Doc Generator CLI tool. See more information about the plans and the workaround at the [Living Documentation discussion topic](https://github.com/orgs/reqnroll/discussions/196).
```

```{attention} MsTest Scenario Outline Handling
Reqnroll generates data-driven tests from scenario outlines with MsTest. With some tooling (`VSTest` pipeline task, `VSTest.Console.exe` filtering) this might cause compatibility issues. Please check the [](#mstest-scenario-outline-handling) section below about the details and how to switch back to the SpecFlow compatible behavior.
```

{#specflow-compatibility-package}
## Migrate with the Reqnroll SpecFlow Compatibility Package

Reqnroll contains a *SpecFlow Compatibility Package* ([`Reqnroll.SpecFlowCompatibility`](https://www.nuget.org/packages/Reqnroll.SpecFlowCompatibility)) that allows to use the Reqnroll classes using the SpecFlow namespace (`TechTalk.SpecFlow`). This allows a quick migration of SpecFlow project that requires minimal code changes. Later the migrated project can be incrementally transformed to use the Reqnroll namespaces.

In order to migrate a SpecFlow project using the SpecFlow compatibility package, you need to perform the following steps.

### Step 1 - Change NuGet packages

You need to remove the SpecFlow NuGet package references from the project and replace them with the Reqnroll ones. This can be done with the Visual Studio NuGet package manager or by modifying the project file in an editor.

* Packages to be removed: 
  * any package where the name starts with SpecFlow, e.g. `SpecFlow` or `SpecFlow.MsTest`
  * the `CucumberExpressions.SpecFlow.*` packages (Reqnroll has built-in [Cucumber Expression](../automation/cucumber-expressions) support)
* Packages to add:
  * The Reqnroll package according to the test execution framework you use: [`Reqnroll.NUnit`](https://www.nuget.org/packages/Reqnroll.NUnit), [`Reqnroll.MsTest`](https://www.nuget.org/packages/Reqnroll.MsTest) or [`Reqnroll.xUnit`](https://www.nuget.org/packages/Reqnroll.xUnit)
  * The SpecFlow Compatibility package: [`Reqnroll.SpecFlowCompatibility`](https://www.nuget.org/packages/Reqnroll.SpecFlowCompatibility)
  * If you have used any of the `SpecFlow.Actions.*` package (e.g. `SpecFlow.Actions.Selenium`), you need to add the matching `Reqnroll.SpecFlowCompatibility.Actions.*` package (`Reqnroll.SpecFlowCompatibility.Actions.Selenium`).

After the change, your project file might look like this:

```{code-block} XML
:caption: C# Project File (.csproj)
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- test project dependencies (MsTest) -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.2.0" />
    <PackageReference Include="MSTest.TestFramework" Version="3.2.0" />

    <!-- Reqnroll -->
    <PackageReference Include="Reqnroll.MsTest" Version="2.0.0" />
    <PackageReference Include="Reqnroll.SpecFlowCompatibility" Version="2.0.0" />
  </ItemGroup>
  [...]
</Project>
```

```{tip}
For most of the SpecFlow projects this is the only change you need to do and your project is ready to run with Reqnroll.
```

### Step 2 - Review code compatibility

Build the project with the changed package references. If the project builds successfully, you can move on to the next step.

If you see build errors, they might belong to one of the following categories.

1. If the C# compiler complains of a missing `TechTalk.SpecFlow.<component>` namespace or a missing class, it means that the code has used some infrastructural elements of SpecFlow. For these files, simply add a namespace using for the related Reqnroll namespace: `using Reqnroll.<component>`. This might happen for special hook classes or step argument transformations.
2. Any other compilation error might be caused by the breaking changes between SpecFlow v3 and v4. Please check the section [](#breaking-changes-specflow-v3) below for the fixes.

After fixing these issues, your project should compile successfully. 

### Step 3 - Review SpecFlow App.config settings (if applicable)

Reqnroll uses a JSON configuration file named `reqnroll.json`, but it is also compatible with the `specflow.json` configuration files. So if you have used `specflow.json` or have not used custom SpecFlow configuration, you can move on to the next step.

If you have used the legacy `App.config` file to configure SpecFlow, your configuration is also handled by the SpecFlow Compatibility package, except the configuration section declaration. So you need to change only one line in your configuration file as highlighted below.

```{code-block} XML
:caption: App.config
:emphasize-lines: 4
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="specFlow" type="Reqnroll.SpecFlowCompatibility.ReqnrollPlugin.ConfigurationSectionHandler, Reqnroll.SpecFlowCompatibility.ReqnrollPlugin" />
  </configSections>
  <specFlow>
    <language feature="hu-HU" />
    <stepAssemblies>
      <stepAssembly assembly="ExternalStepDefs" />
    </stepAssemblies>
  </specFlow>
</configuration>
```

### Step 4 - Review execution compatibility

Now it is time to run your tests. If the tests were passing before, they should be still passing, there is no reported compatibility issue.

If you run into any problems, it might be caused by the breaking changes between SpecFlow v3 and v4. Please check the section [](#breaking-changes-specflow-v3) below for the fixes.

**Congratulations you are done!**

Our more complex sample application, [ReqOverflow](https://github.com/reqnroll/Sample-ReqOverflow) has been also migrated from SpecFlow. You can check what changes we had to do in order to get it working with Reqnroll using the SpecFlow compatibility package. See the changes on [GitHub](https://github.com/reqnroll/Sample-ReqOverflow/compare/before-migration...migration-with-compatiblity-package).

{#with-namespace-changes}
## Migrate with namespace changes

Thanks to the high level of compatibility, it is also easy to perform a full migration from SpecFlow projects that requires simple changes, it is mostly doable with search-and-replace.

In order to migrate a SpecFlow project, you need to perform the following steps.

### Step 1 - Change NuGet packages

You need to remove the SpecFlow NuGet package references from the project and replace them with the Reqnroll ones. This can be done using the Visual Studio NuGet package manager or by modifying the project file in an editor.

* Packages to be removed: 
  * any package where the name starts with SpecFlow, e.g. `SpecFlow` or `SpecFlow.MsTest`
  * the `CucumberExpressions.SpecFlow.*` packages (Reqnroll has built-in [Cucumber Expression](../automation/cucumber-expressions) support)
* Packages to add:
  * The Reqnroll package according to the test execution framework you use: [`Reqnroll.NUnit`](https://www.nuget.org/packages/Reqnroll.NUnit), [`Reqnroll.MsTest`](https://www.nuget.org/packages/Reqnroll.MsTest) or [`Reqnroll.xUnit`](https://www.nuget.org/packages/Reqnroll.xUnit)
  * If you have used any of the `SpecFlow.Actions.*` package (e.g. `SpecFlow.Actions.Selenium`), you need to add the matching `Reqnroll.SpecFlowCompatibility.Actions.*` package (`Reqnroll.SpecFlowCompatibility.Actions.Selenium`).

After the change, your project file might look like this:

```{code-block} XML
:caption: C# Project File (.csproj)
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <!-- test project dependencies (MsTest) -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="MSTest.TestAdapter" Version="3.2.0" />
    <PackageReference Include="MSTest.TestFramework" Version="3.2.0" />

    <!-- Reqnroll -->
    <PackageReference Include="Reqnroll.MsTest" Version="2.0.0" />
  </ItemGroup>
  [...]
</Project>
```

### Step 2 - Replace namespaces

Now open the project in Visual Studio or in a code editor and replace all usages of the `TechTalk.SpecFlow` namespace to `Reqnroll`. This can be done with a search-and-replace operation in your editor. Make sure you perform the replace in all files (usual shortcut: *Shift-Ctrl-H*).

* Search for: `TechTalk.SpecFlow`, enable *Match case* and *Match whole word*
* Replace with: `Reqnroll`

This will replace the namespace in namespace usings (e.g. `using TechTalk.SpecFlow;`) or fully qualified class names (e.g. `TechTalk.SpecFlow.ScenarioContext`).

### Step 3 - Review code compatibility

Build the project with the changed package references. If the project builds successfully, you can move on to the next step.

If you see build errors, they might belong to one of the following categories.

1. You might have used a SpecFlow class that had SpecFlow in the name. The most common example is `ISpecFlowOutputHelper`. Replace these accordingly (e.g. `IReqnrollOutputHelper`). If you use them extensively, you can also use a "replace in all files" function.
2. Any other compilation error might be caused by the breaking changes between SpecFlow v3 and v4. Please check the section [](#breaking-changes-specflow-v3) below for the fixes.

After fixing these issues, your project should compile successfully. 

### Step 4 - Migrate config settings

If you have not used custom SpecFlow configuration, you can move on to the next step.

Reqnroll uses a JSON configuration file named `reqnroll.json`. The format is compatible with the `specflow.json` configuration file format, so the migration is simple, you just need to rename the file to `reqnroll.json`. It is recommended to set the JSON schema reference, so that your editor offers completion for the settings. The official schema reference is `https://schemas.reqnroll.net/reqnroll-config-latest.json`, which you can use like the example shows below.

```{code-block} json
:caption: reqnroll.json
:emphasize-lines: 2
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "language": {
    "feature": "hu-HU"
  },
  "bindingAssemblies": [
    {
      "assembly": "ExternalStepDefs"
    }
  ]
}
```

There are two settings in the `reqnroll.json` that have different name, although the names used in SpecFlow are also accepted. It is recommended though to update these as well:
* The `stepAssemblies` section has been renamed to `bindingAssemblies`. See [](../installation/configuration.md#bindingassemblies).
* The `bindingCulture/name` setting has been moved to the language section as `language/binding`. See [](../installation/configuration.md#language).


If you have used the legacy `App.config` file to configure SpecFlow, you need to migrate the settings to `reqnroll.json` based on our [](../installation/configuration) reference.

### Step 5 - Review execution compatibility

Now it is time to run your tests. If the tests were passing before, they should be still passing, there is no reported compatibility issue.

If you run into any problems that might be caused by the breaking changes between SpecFlow v3 and v4. Please check the section [](#breaking-changes-specflow-v3) below for the fixes.

**Congratulations you are done!**

Our more complex sample application, [ReqOverflow](https://github.com/reqnroll/Sample-ReqOverflow) has been also migrated from SpecFlow. You can check what changes we had to do in order to get it working with Reqnroll with a complete migration. See the changes on [GitHub](https://github.com/reqnroll/Sample-ReqOverflow/compare/before-migration...full-migration).

{#breaking-changes-specflow-v3}
## Breaking changes since SpecFlow v3

As Reqnroll is based on SpecFlow v4, if you migrate from SpecFlow v3, you might encounter problems that are caused by the breaking changes between SpecFlow v3 and v4. The following list contains the most important breaking changes and the suggestions to resolve them.

### Cucumber Expressions support, compatibility of existing expressions

Reqnroll supports [Cucumber Expressions](../automation/cucumber-expressions) natively for [step definitions](../automation/step-definitions). This means that whenever you define a step using the `[Given]`, `[When]` or `[Then]` attribute, you can either provide a regular expression for it as a parameter or a cucumber expression.

Most of your existing regex step definitions will be compatible, because they are either properly recognized as regex or the expression works the same way with both expression types (e.g. simple text without parameters). 

In case your regular expression is wrongly detected as cucumber expression, you can always force to use regular expression by specifying the regex start/end markers (`^`/`$`).

```
[When(@"^this expression is treated as a regex$")]
```

There are a few special cases listed below.

#### Invalid expressions after upgrade

In some cases you may see an error after upgrading to the Reqnroll. For example if you had a step definition with an attribute like:

```
[When(@"I \$ something")]
```

```
This Cucumber Expression has a problem ...
```

In this case the problem is that Reqnroll wrongly identified your expression as a cucumber expression.

**Solution 1:** Force the expression to be a regular expression by specifying the regex start/end markers (`^`/`$`):

```
[When(@"^I \$ something$")]
```

If you have many of such step definitions, you can force all of them to be treated as regex by including the start/end markers using Visual Studio "Find and Replace in Files" (Ctrl+Alt+H) option:

* Set search text to `\[(Given|When|Then)\((@?)"(.*?)"\)\]`
* Set replacement text to `[$1($2"^$3$$")]`
* Check "Use regular expressions" setting
* Click on "Replace All"

This will add the markers to all step definition attributes.

**Solution 2:** Change the expression to be a valid cucumber expression. For the example above, you need to remove the masking character (`\`), because the `$` sign does not have to be masked in cucumber expressions:

```
[When("I $ something")]
```

**Solution 3:** If none of the above solutions are possible, you can also consider changing the cucumber expression detection strategy as described in [](how-to-configure-cucumber-expression-behavior.md).

#### Expression matching problems during test execution

In some very special cases it can happen that the expression is wrongly identified as cucumber expression, but you only get the step binding error during test execution (usually `No matching step definition found` error), because the expression is valid as regex and as cucumber expression as well, but with different meaning. 

For example if you had a step definition that matches the step `When I a/b something`, it will be considered as a cucumber expression, but in cucumber expressions, the `/` is used for alternation (so it matches either `When I a something` or `When I b something`).

```
[When(@"I a/b something")]
```

**Solutions:** You can apply the same solutions as above: either force it to be a regular expression by specifying the regex start/end markers or make it a valid cucumber expression. 

For the latter case, you would need to mask the `/` character:

```
[When(@"I a\/b something")]
```

#### Cucumber Expression step definition skeletons

Reqnroll will by default generate step definition skeletons (snippets) for the new steps. So in case you write a new step as

```
When I have 42 cucumbers in my belly
```

Reqnroll will suggest the step definition to be:

```
[When("I have {int} cucumbers in my belly")]
public void WhenIHaveCucumbersInMyBelly(int p0)
...
```

If you would like to use only regular expressions in your project, you either have to fix the expression manually, or you can configure Reqnroll to generate skeletons with regular expressions. You can achieve this with the following setting in the `reqnroll.json` file:

```{code-block} json
:caption: reqnroll.json
:emphasize-lines: 4
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "trace": {
    "stepDefinitionSkeletonStyle": "RegexAttribute"
  }
}
```

### Removed calling other steps with string

The SpecFlow v3 functionality of calling a step from a step like this is not available in Reqnroll (has been removed in SpecFlow v4):

```{code-block} csharp
:caption: Step Definition Class
:emphasize-lines: 13-14
[Binding]
public class CallingStepsFromStepDefinitionSteps : Steps
{
  [Given(@"the user (.*) exists")]
  public void GivenTheUserExists(string name) { ... }

  [Given(@"I log in as (.*)")]
  public void GivenILogInAs(string name) { ... }

  [Given(@"(.*) is logged in")]
  public void GivenIsLoggedIn(string name)
  {
    Given(string.Format("the user {0} exists", name));
    Given(string.Format("I log in as {0}", name));
  }
```

This is not possible anymore, as the methods are now removed.

If you use this feature, you have two options:
- refactor to the [Driver Pattern](../guides/driver-pattern.md)
- call the methods directly

### MsTest Scenario Outline Handling

For scenario outline examples, SpecFlow has generated multiple methods, one for each example. In the newer versions of MsTest they have introduced support for data-driven tests using the MsTest `[DataTestMethod]` and `[DataRow]` attributes. Reqnroll has been changed to use this feature, so by default it will generate a single method for scenario outlines with multiple `[DataRow]` attributes. This allows the test runners to display the related scenario outline results in a hierarchical way.

There are some tooling that does not fully support this MsTest feature. For example you might experience problems with the [VSTest task](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/vstest-v3?view=azure-pipelines) on Azure pipelines when you use distributed execution and the filtering options with [VSTest.Console.exe](https://learn.microsoft.com/en-us/visualstudio/test/vstest-console-options?view=vs-2022) might also have problems. 

If you use such incompatible tooling, you can revert back to the original behavior by setting `generator/allowRowTests` to `false` in the `reqnroll.json` configuration file.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "generator": {
    "allowRowTests": false
  }
}
```

```{tip}
If the problem is only on the pipeline, you can have a pipeline step that patches the `reqnroll.json` configuration *before the build of the project*, so that you can still enjoy the nice structured tests locally.
```


### Complete changelog of SpecFlow v4

As SpecFlow v4 was never officially released and the original GitHub project has been deleted, here is a list of changes that were planned for SpecFlow v4 and Reqnroll has overtaken.

Breaking Changes:

* Removed the ability to call steps from steps via string
* Removed .NET Core 2.1 support
* Removed .NET Framework 4.6.1 support (min .NET Framework version: 4.6.2)
* Bindings declared as `async void` are not allowed. Use `async Task` instead.

Features:

* Add an option to colorize test result output
* Support for using Cucumber Expressions for step definitions.
* Support Rule tags (can be used for hook filters, scoping and access through `ScenarioInfo.CombinedTags`)
* Support for async step argument transformations.
* Support for ValueTask and ValueTask<T> binding methods (step definitions, hooks, step argument transformations)
* Rules now support Background blocks
* Collect binding errors (type load, binding, step definition) and report them as exception when any of the tests are executed.

Changes:

* Existing step definition expressions detected to be either regular or cucumber expression. 
* Default step definition skeletons are generating cucumber expressions.
* `ScenarioInfo.ScenarioAndFeatureTags` has been deprecated in favor of `ScenarioInfo.CombinedTags`. Now both contain rule tags as well.
* AggregateExceptions thrown by async StepDefinition methods are no longer consumed; but passed along to the test host.
