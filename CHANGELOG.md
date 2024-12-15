# [vNext]

## Improvements:

* Enhance BoDi error handling to provide the name of the interface being registered when that interface has already been resolved (#324)
* Improve code-behind feature file compilation speed (#336)
* Improve parameter type naming for generic types (#343)

## Bug fixes:
* MsTest: Only use TestContext for output and not Console.WriteLine (#368) 

*Contributors of this release (in alphabetical order):* @clrudolphi, @obligaron, @olegKoshmeliuk

# v2.2.1 - 2024-11-08

## Improvements:

## Bug fixes:

* Fix: NUnit projects fail or provide warning as `TearDown : System.InvalidOperationException : Only static OneTimeSetUp and OneTimeTearDown are allowed for InstancePerTestCase mode.` (#320)

*Contributors of this release (in alphabetical order):* @gasparnagy

# v2.2.0 - 2024-11-07

## Improvements:

* Upgrade to Gherkin v30 from v29 (see [Gherkin changelog](https://github.com/cucumber/gherkin/blob/main/CHANGELOG.md)) (#305)
* Support scenario-level (method-level) parallel execution (#119, #277)

## Bug fixes:

* Fix: Visual Studio locks Reqnroll.Tools.MsBuild.Generation task files. Using `TaskHostFactory` for our tasks on Windows. (#293)
* Fix: Project dependencies transiently refer to System.Text.Json 8.0.4 that has security vulnerability. Microsoft.Extensions.DependencyModel updated to v8.0.2. (#291)
* Fix: Could not load System.CodeDom exception with xRetry.Reqnroll plugin (#310)
* Fix: Reqnroll.Microsoft.Extensions.DependencyInjection: `System.Collections.Generic.KeyNotFoundException: The given key 'Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceProviderEngineScope' was not present in the dictionary.` is thrown when a test execution dependent service is required during [BeforeTestRun]. We provide a better error message now. (#175)

*Contributors of this release (in alphabetical order):* @gasparnagy, @obligaron, @Romfos, @Tiberriver256

# v2.1.1 - 2024-10-08

## Bug fixes:

* Fix: Rule Backgounds cause External Data Plugin to fail (#271)
* Fix: VersionInfo class might provide the version of the runner instead of the version of Reqnroll (#248)
* Fix: Reqnroll.CustomPlugin NuGet package has a version mismatch for the System.CodeDom dependency (#244)
* Fix: Reqnroll.Verify fails to run parallel tests determinately (#254). See our [verify documentation](docs/integrations/verify.md) on how to set up your test code to enable parallel testing.
* Fix: Reqnroll generates invalid code for rule backgrounds in Visual Basic (#283)

*Contributors of this release (in alphabetical order):* @ajeckmans, @clrudolphi, @gasparnagy, @UL-ChrisGlew

# v2.1.0 - 2024-08-30

## Improvements:

* Reqnroll.Verify: Support for Verify v24 (Verify.Xunit v24.2.0) for .NET 4.7.2+ and .NET 6.0+. For earlier versions of Verify or for .NET 4.6.2, use the latest 2.0.3 version of the plugin that is compatible with Reqnroll v2.*. (#151)
* Reqnroll.Windsor: Support for Castle.Windsor v6.0.0. For earlier versions of Castle.Windsor, use the latest 2.0.3 version of the plugin that is compatible with Reqnroll v2.*. (#240)
* Optimize creation of test-thread context using test framework independent resource pooling (#144)
* Support DateTimeOffset in value comparer (#180)
* Support `Order` parameter for `StepArgumentTransformationAttribute` to prioritize execution (#185)
* Upgrade to Gherkin v29 from v19 (see [Gherkin changelog](https://github.com/cucumber/gherkin/blob/main/CHANGELOG.md)) (#205, #240)
* Rider: Avoid requirement to define `ReqnrollFeatureFiles` MsBuild item group to show feature files in Rider (#231)
* Added option to override regex group matching behavior (#243)

## Bug fixes:

* Fix: Reqnroll.Autofac: Objects registered in the global container cannot be relsolved in BeforeTestRun/AfterTestRun hooks (#183)
* Fix: Process cannot access the file when building a multi-target project (#197)
* Fix: Project dependencies transiently refer to System.Net.Http <= v4.3.0 that has high severity security vulnerability (#240)

*Contributors of this release (in alphabetical order):* @ajeckmans, @cimnine, @gasparnagy, @obligaron, @olegKoshmeliuk, @runnerok, @stbychkov

# v2.0.3 - 2024-06-10

## Improvements:

* Update [versioning policy for plugins](https://docs.reqnroll.net/latest/installation/compatibility.html#versioning-policy) and set plugin dependencies accordingly (#160)
* Generate symbol packages, use deterministic build and update package metadata (#161)

## Bug fixes:

* Fix: Project created with `dotnet new reqnroll-project` contains an invalid binding class (`[Binding]` attribute missing) (#169)

*Contributors of this release (in alphabetical order):* @gasparnagy, @mcraa

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
