# [vNext]

* Reqnroll.Verify: Support for Verify v24 (Verify.Xunit v24.2.0) for .NET 4.7.2+ and .NET 6.0+. For earlier versions of Verify or for .NET 4.6.2, use the latest 2.0.3 version of the plugin that is compatible with Reqnroll v2.*. (#151)

## Improvements:

* Update [versioning policy for plugins](https://docs.reqnroll.net/latest/installation/compatibility.html#versioning-policy) and set plugin dependencies accordingly (#160)
* Generate symbol packages, use deterministic build and update package metadata (#161)

# v2.0.2 - 2024-05-31

## Bug fixes:

* Fix: Building a Reqnroll project on macOS ARM64 architecture (eg MacBook M1) fails (#152)
* Fix: xUnit .NET framework Reqnroll projects might not run BeforeTestRun even with the fix for #146 (#152)

# v2.0.1 - 2024-05-29

## Bug fixes:

* Fix: BeforeTestRun not run in .NET462 up to .NET481 in multitarget test project (#146)

# v2.0.0 - 2024-05-22

## Breaking changes:

* The namespace of the `IObjectContainer` class has been changed from `BoDi` to `Reqnroll.BoDi`. You might need to update the namespace usages.

## Improvements:

* MsTest: Support for PriorityAttribute
* MsTest: Support for `[DataRow]` attribute for scenario outlines (default behavior)
* MsTest: Use ClassCleanupBehavior.EndOfClass instead of custom implementation (preparation for MsTest v4.0)
* SpecFlow.ExternalData: Support for loading data from JSON files
* Reqnroll.Microsoft.Extensions.DependencyInjection: Port [SolidToken.SpecFlow.DependencyInjection](https://github.com/solidtoken/SpecFlow.DependencyInjection) to Reqnroll. Thanks to @mbhoek (Solid Token) for the contribution! (#94)
* Plugins: Allow creating single target (netstandard2.0) plugins
* Dependencies: Include built-in dependency injection framework (BoDi) to the main repository as "Reqnroll.BoDi" based on v1.5 of [BoDi](https://github.com/SpecFlowOSS/BoDi/)

## Bug fixes:

* Fix: User code namespaces that included "Reqnroll" within them caused the code generation to fail (#44)
* Fix: Dependencies of [BeforeTestRun] / [AfterTestRun] hooks are wrongly resolved from the test thread context instead of the test run (global) context instead (#58)
* Fix: Cucumber Expressions fail when two enums or two custom types with the same short name (differing namespaces) are used as parameters (#81)
* Fix: Adding `@ignore` to an Examples block generates invalid code for NUnit v3+ (#103)
* Fix: `@ignore` attribute is not inherited to the scenarios from Rule (#111)
* Fix: Capture ExecutionContext after every binding invoke (#120)
* Fix: StackOverflowException when using `[StepArgumentTransformation]` with same input and output type, for example string (#71)
* Fix: Autofac without hook does not run GlobalDependencies (#127)
* Fix: Reqnroll.Autofac shows wrongly ambiguous step definition (#56)
* Fix: Dispose objects registered in test thread container at the end of test execution (#123)

# v1.0.1 - 2024-02-16

* Fix: Error when installing the Reqnroll template via command line (#22)

# v1.0.0 - 2024-02-05

* Support for .NET 8 projects
* Initial release based on v4.0.31-beta of [SpecFlow](https://github.com/SpecFlowOSS/SpecFlow/).
