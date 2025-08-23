# Setup Reqnroll Project

This page guides you through setting up your Reqnroll project. It is also recommended to configure your Integrated Development Environment (IDE, e.g. Visual Studio 2022) to work conveniently with Reqnroll. To set up your IDE, please follow the [](setup-ide) guide first.

{#choosing-your-test-execution-framework}
## Choosing your test execution framework

Reqnroll uses *test execution frameworks* (MsTest, NUnit, TUnit or xUnit) to run the tests. So first of all, you need to decide, which one you would like to use. Reqnroll does not have a favorite one, so you should better choose the one you have the most experience with. If you don't have any preference, choose NUnit. The following table gives you a quick comparison of the different supported execution frameworks.

```{list-table}
:header-rows: 1

* - Framework
  - NuGet package
  - Description
* - [NUnit](https://nunit.org/)
  - [`Reqnroll.NUnit`](https://www.nuget.org/packages/Reqnroll.NUnit)
  - Easy to use testing framework with respectful history. Supports test attachments and comes with an extensive assertion library.
* - [MsTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)
  - [`Reqnroll.MsTest`](https://www.nuget.org/packages/Reqnroll.MsTest)
  - A widely supported framework by Microsoft. Supports test attachments and input parameters through the [`TestContext`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.testcontext?view=visualstudiosdk-2022) class.
* - [TUnit](https://tunit.dev/)
  - [`Reqnroll.TUnit`](https://www.nuget.org/packages/Reqnroll.TUnit)
  - A modern testing framework for .NET built with performance in mind. Leveraging source generation and AOT compilation for efficient test execution. 
* - [xUnit](https://xunit.net/)
  - [`Reqnroll.xUnit`](https://www.nuget.org/packages/Reqnroll.xUnit)
  - Simple and modern testing framework that reports the original names of the scenarios during execution. It does not support test attachments and writing test execution output cannot be done with `Console.WriteLine`, so it is less practical for integration tests.
```

```{note}

If you changed your mind and you would like to switch to another test execution framework, check out the [](../guides/how-to-change-test-execution-framework) guide. Using independent assertion frameworks, like [Fluent Assertions](https://fluentassertions.com/) makes the change much easier.
```

## Setting up a Reqnroll project

Once you have chosen your test execution framework, you need to setup the Reqnroll project. This can be done by creating a new Reqnroll project or setup an existing .NET test project.

Depending on your situation, you can find the necessary setup instructions in one of the following sub-sections.

* [](#new-project-visual-studio)
* [](#new-project-console)
* [](#setup-existing-project)

{#new-project-visual-studio}
### Creating a new Reqnroll project from Visual Studio

If you have installed the [](../ide-integrations/visual-studio/index), you can easily create a new Reqnroll project using the *Add new project* wizard, by performing the following steps:

1. From the context menu of your solution in the *Solution Explorer* window select *Add / New Project...*
2. In the *Add a new project* dialog enter `Reqnroll` to the *Search for templates* text box.
3. Choose *Reqnroll Project" from the list and click on *Next* button.
4. Follow the wizard by choosing the name, the target framework and the test framework for the project.

As a result, a new Reqnroll project is created with a sample [feature file](../gherkin/feature-files) and [step definition class](../automation/step-definitions).

Build your project and [execute the sample scenarios](../execution/executing-reqnroll-scenarios.md#executing-scenarios-from-visual-studio).

{#new-project-console}
### Creating a new Reqnroll project from console

Reqnroll projects can also be installed using the .NET template infrastructure and the `dotnet new` command.

First, you need to make sure that the Reqnroll templates are installed on your computer by running the `dotnet new install Reqnroll.Templates.DotNet`:

```{code-block} pwsh
:caption: .NET CLI
dotnet new install Reqnroll.Templates.DotNet
```

Once the templates have been installed, you can create a new project using the `dotnet new reqnroll-project` command in a new directory.

```{code-block} pwsh
:caption: Terminal
> mkdir MyReqnrollProject
> cd MyReqnrollProject
> dotnet new reqnroll-project
The template "Reqnroll Project Template" has been created successfully.
```

This command creates a Reqnroll project with NUnit for the latest .NET framework. In order to use other test execution framework or .NET version, you can use the `-t` and the `-f` option. For the possible values and all options, you can invoke `dotnet new reqnroll-project --help`.

```{code-block} pwsh
:caption: .NET CLI
dotnet new reqnroll-project --help
```

The following command creates a Reqnroll project with MsTest using .NET 8.0

```{code-block} pwsh
:caption: .NET CLI
dotnet new reqnroll-project -t mstest -f net8.0
```

As a result, a new Reqnroll project is created with a sample [feature file](../gherkin/feature-files) and [step definition class](../automation/step-definitions).

You can go ahead and [execute the sample scenarios](../execution/executing-reqnroll-scenarios.md#executing-scenarios-from-console) from console.

To add further feature file or a [Reqnroll configuration file](configuration.md), you can also use the `reqnroll-feature` and `reqnroll-config` item templates. The following command adds a new feature file to the project named `MyFeature.feature`.

```{code-block} pwsh
:caption: .NET CLI
dotnet new reqnroll-feature -n MyFeature
```

{#setup-existing-project}
### Setup an existing test project

Reqnroll can also be configured for an existing [test project](https://learn.microsoft.com/en-us/visualstudio/test/create-a-unit-test-project?view=vs-2022). For that you need to add the NuGet package of your chosen test execution framework to your project. (Check the NuGet package names [above](#choosing-your-test-execution-framework)). The chosen test execution framework has to match the framework used in your existing test project.

The following example adds the Reqnroll NuGet package for an MsTest project.

```{code-block} pwsh
:caption: .NET CLI
dotnet add package Reqnroll.MsTest
```

Although the Reqnroll tests can be mixed with normal unit tests in the same .NET project, for the sake of clarity it is recommended to create a separate project for your Reqnroll BDD scenarios.
