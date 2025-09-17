# Reqnroll

[![NuGet Version](https://img.shields.io/nuget/v/Reqnroll)](https://www.nuget.org/packages/Reqnroll)

[![CI](https://github.com/reqnroll/Reqnroll/actions/workflows/ci.yml/badge.svg)](https://github.com/reqnroll/Reqnroll/actions/workflows/ci.yml)

Reqnroll is an open-source .NET test automation tool to practice [Behavior Driven Development (BDD)](https://cucumber.io/docs/bdd/).

Reqnroll is a .NET port of [Cucumber](https://cucumber.io/) and it is based on the SpecFlow framework and code base. You can find more information about the goal of the Reqnroll project and the motivations to create it on the [Reqnroll website](https://reqnroll.net/).

Reqnroll enables writing executable specifications for BDD using [Gherkin](https://cucumber.io/docs/gherkin/), the widely-accepted *feature file* specification format. With that you can define the requirements using *Given-When-Then* style *scenarios* and turn them to automated tests in order to verify their implementation.

Reqnroll works on all major operating systems (Windows, Linux, macOS), on all commonly used .NET implementations (including .NET Framework 4.6.2+ and up to .NET 9.0). For executing the automated scenarios, Reqnroll can use [MsTest](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest), [NUnit](https://nunit.org/), [xUnit](https://xunit.net/) or [TUnit](https://tunit.dev/). On Reqnroll projects you can work using Visual Studio 2022, Visual Studio Core and Rider, but you can also use Reqnroll without any IDE.

## Useful links

* [Quickstart guide](https://go.reqnroll.net/quickstart)
* [Reqnroll website](https://reqnroll.net/)
* [Reqnroll documentation](https://docs.reqnroll.net/)
* [Migrating from SpecFlow](https://docs.reqnroll.net/latest/guides/migrating-from-specflow.html)
* [Release notes](https://go.reqnroll.net/release-notes)
* [IDE setup instructions for Reqnroll](https://go.reqnroll.net/doc-setup-ide)

## Installation

The extension can be installed via NuGet packages from nuget.org. The main package you need to install depends on the test execution framework: [`Reqnroll.NUnit`](https://www.nuget.org/packages/Reqnroll.NUnit), [`Reqnroll.MsTest`](https://www.nuget.org/packages/Reqnroll.MsTest), [`Reqnroll.xUnit`](https://www.nuget.org/packages/Reqnroll.xUnit) or [`Reqnroll.TUnit`](https://www.nuget.org/packages/Reqnroll.TUnit). See detailed instructions on the [project setup documentation page](https://go.reqnroll.net/doc-setup-project).

## Contributing

All contributors are welcome! For more information see the [Contribution guidelines](CONTRIBUTING.md)

## Sponsors

* [Spec Solutions](https://www.specsolutions.eu/)
* [Info Support](https://www.infosupport.com/)
* [LambdaTest](https://www.lambdatest.com/)

See all sponsors and information about sponsorship on the [Sponsorship page](https://reqnroll.net/sponsorship) of our website.

## License

Reqnroll for VisualStudio is licensed under the [BSD 3-Clause License](LICENSE).

Copyright (c) 2024-2025 Reqnroll

This project is based on the SpecFlow framework.
