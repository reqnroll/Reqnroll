---
hide-toc: true
---

# Reqnroll Documentation

Reqnroll is an open-source .NET test automation tool to practice [Behavior Driven Development (BDD)](https://cucumber.io/docs/bdd/).

Reqnroll is a .NET port of [Cucumber](https://cucumber.io/) and it is based on the [SpecFlow](https://www.specflow.org/) framework and code base. You can find more information about the goal of the Reqnroll project and the motivations to create it on the [Reqnroll website](https://reqnroll.net/).

Reqnroll enables writing executable specifications for BDD using [Gherkin](https://cucumber.io/docs/gherkin/), the widely-accepted *feature file* specification format. With that you can define the requirements using *Given-When-Then* style *scenarios* and turn them to automated tests in order to verify their implementation.

Reqnroll works on all major operating systems (Windows, Linux, macOS), on all commonly used .NET implementations (including .NET Framework 4.6.2+ and .NET 8.0). For executing the automated scenarios, Reqnroll can use [MsTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest), [NUnit](https://nunit.org/), [TUnit](https://tunit.dev/) or [xUnit](https://xunit.net/). On Reqnroll projects you can work using Visual Studio 2022, Visual Studio Code and Rider, but you can also use Reqnroll without any IDE.

Since Reqnroll has been based on SpecFlow, you can use your SpecFlow knowledge to work with Reqnroll and it is also very easy to port an existing SpecFlow project to Reqnroll. You can check out our detailed [migration guide](guides/migrating-from-specflow).

This documentation provides a comprehensive source of information about how to use Reqnroll. We also recommend you to follow the news and the blog on the [Reqnroll website](https://reqnroll.net/).

## How to use the documentation

```{admonition} Documentation is in progress
:class: warning

We are currently in progress of reviewing and restructuring the documentation ported from SpecFlow. If you find any glitches, please help with fixing it in our GitHub repository. Each page contains a small ✏️ (*edit*) icon to perform quick edits.
```

The documentation is structured in a way that you can find all relevant information quickly.

- In order to understand the concept of Reqnroll, we recommend checking out our [Quickstart Guide](quickstart/index).
- For setting up Reqnroll for your own project from scratch, you can find all details in the [Installation & Setup](installation/index) section.
- To migrate an existing SpecFlow project to Reqnroll, please follow our [SpecFlow Migration Guide](guides/migrating-from-specflow).
- The *FEATURES* block covers all the details of Reqnroll features. There are separate sections about 

  - the [Gherkin format](gherkin/index),
  - features for [writing automation code](automation/index),
  - features related to [test execution](execution/index), and
  - [extending the capabilities](extend/index) of Reqnroll.

```{toctree}
:hidden:

quickstart/index
installation/index
guides/index
```

```{toctree}
:caption: Features
:hidden:

gherkin/index
automation/index
execution/index
reporting/index
extend/index
```

```{toctree}
:caption: Integrations
:hidden:

integrations/index
ide-integrations/index
```

```{toctree}
:caption: Help
:hidden:

help/troubleshooting
help/faq
help/samples
help/support
```

