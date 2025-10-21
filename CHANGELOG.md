# [vNext]

## Improvements:

## Bug fixes:

*Contributors of this release (in alphabetical order):* 

# v3.2.0 - 2025-10-21

## Improvements:

* Step definition snippets (aka, skeletons) can be generated as async methods by setting `treace/stepDefinitionSkeletonStyle` to `AsyncCucumberExpressionAttribute` or `AsyncRegexAttribute`. (#876)
* Added Support for MsTest v4 (#703, #889, #894, #896)

## Bug fixes:

* Address [Microsoft Security Advisory CVE-2025-55247 | .NET Denial of Service Vulnerability](https://github.com/advisories/GHSA-w3q9-fxm7-j8fq) (#893)

*Contributors of this release (in alphabetical order):*  @clrudolphi, @Code-Grump, @gasparnagy, @kant2002

# v3.1.2 - 2025-10-07

## Bug fixes:

* Fix: MsTest: Friendly names are generated for scenario outline examples, even if `generator/disableFriendlyTestNames` is set to true, causing a Reqnroll v2 compatiblility issue (#867)
* Fix: Performance issue caused by the fix #862 in v3.1.0 (#874)

*Contributors of this release (in alphabetical order):* @clrudolphi, @konarx

# v3.1.1 - 2025-09-29

## Bug fixes:

* Fix: Mismatch in Cucumber.HtmlFormatter version between package and project causing build warnings (#868)

*Contributors of this release (in alphabetical order):* @jdb0123

# v3.1.0 - 2025-09-26

## New features:

* Added xUnit 3 support. In order to use Reqnroll with xUnit v3, you need to use the `Reqnroll.xunit.v3` package. (#405, #538)
* Disabling parallel execution with the `addNonParallelizableMarkerForTags` efature now also applies to scenario-level tags for frameworks supporting method-level isolation (NUnit, MsTest V2, TUnit). (#826)
* Generating "friendly names" for generated test methods by default can be disabled by the `generator/disableFriendlyTestNames` setting in `reqnroll.json`. This can help to avoid compatiblity issues with tools like VsTest retry. For MsTest this setting restores the behavior of Reqnroll v2. (#854)

## Improvements:

* Reqnroll.Verify: Support for Verify v29+ (Verify.Xunit v29.0.0 or later). For earlier versions use 3.0.3 version of the plugin that is compatible with Reqnroll v3.*. The support for custom snapshot files with global VerifySettings has been removed, see [plugin documentation](https://docs.reqnroll.net/latest/integrations/verify.html) for details and workarounds. (#572)
* Dependencies: Updated to Cucumber Gherkin v35, Cucumber Messages v29 and Cucumber CompatibilityKit v23 (#841)
* Improved `Reqnroll.Tools.MsBuild.Generation` NuGet package: reduced size by optimizing included dependencies, consolidated MsBuild assets (#812)

## Bug fixes:

* Fix: Formatters: Test execution fails when formatters were disabled due to a generation error (#861)
* Fix: Exceptions from before scenario plugin hook events (RuntimePluginTestExecutionLifecycleEvents.BeforeScenario) are not shown (#856)
* Fix: Before scenario hooks are not raised immediately when `runtime/stopAtFirstError` is true, but handled delayed (#857)
* Fix: Dry Run mode doesn't work properly if the project contains hooks that register & inject custom dependencies (#862)

*Contributors of this release (in alphabetical order):* @chekkan, @Code-Grump, @DrEsteban, @gasparnagy, @konarx

# v3.0.3 - 2025-09-17

## Improvements:

* Formatters: Enchanced logging of exception information when file-based formatters throw exceptions (#842)

## Bug fixes:

* Fix: Step execution improperly handles error flows when `runtime/stopAtFirstError` is true, resulting in skipped step events and hence invalid reports (#819)

*Contributors of this release (in alphabetical order):* @clrudolplhi

# v3.0.2 - 2025-09-08

## Bug fixes:

* Fix: Unnecessary build output messages are shown during build (#815)

*Contributors of this release (in alphabetical order):* @gasparnagy

# v3.0.1 - 2025-09-05

## Bug fixes:

* Fix: Formatters: Formatters fail under .NET Framework when OutputFilePath is not configured (#797)
* Fix: Formatters: Build URL is invalid in HTML report for Azure DevOps builds (#795)
* Fix: Build fails with "error CS8103: Combined length of user strings used by the program exceeds allowed limit." when a project contains many long feature files. (#785)
* Fix: Embedded feature file resource generates different resource names in Windows/Linux (#799)
* Fix: Formatters: The Build Info banner of HTML reports shows UNKNOWN banner when the Build Server cannot be determined (#800)
* Fix: Formatters: Success flag in `TestRunFinished` Cucumber message is set to true when before/after test run hook fails (Cucumber/Cucumber-compatibility-Kit v21.0.0) (#801)

*Contributors of this release (in alphabetical order):* @gasparnagy, @clrudolphi

# v3.0.0 - 2025-08-21

## New features:

* Formatters: An infrastructure for implementing integrated reporting formatters has been introduced. The infrastructure allows writing custom formatters, but Reqnroll provides two built-in formatters as well: the HTML and Message formatters. (#233)
	* HTML Formatter: A formatter that can produce single page HTML reports using the [Cucumber React components](https://github.com/cucumber/react-components). This formatter can be used as a replacement for the "SpecFlow+ LivingDoc Generator"
	* Message Formatter: A formatter that can produce [Cucumber Messages](https://github.com/cucumber/messages/) `.ndjson` file. Cucumber messages contain all information about the test run, therefore this formatter can be used to integrate Reqnroll with other tools.
* Support for [TUnit test framework](https://tunit.dev/). Use the package `Reqnroll.TUnit` in a TUnit project to enable Reqnroll TUnit support. Can be used with TUnit v0.55.23 and later. (#442)
* Allow test execution without invoking binding (step definition, hook, step argument transformation) code by setting the `REQNROLL_DRY_RUN` environment variable to `true`. This is useful for quickly verifying if all steps have been defined. (#614)

## Improvements:

* Added `RuleInfo` to `ScenarioContext` to provide information about the current rule (#454)
* Assist: Added `VerifyCaseInsensitive` flag to `InstanceCreationOptions` that allows for case-insensitive member verification when object instances are created from tables (#577)
* Renamed `Regex` property and `regex` constructor parameter for all step definition attributes to `Expression` and `expression`, as it represents a cucumber expression or a regular expression (regex) that matches the step text. (#639)
* Added `ExpressionType` option (`CucumberExpression`/`RegularExpression`) to `[Given]`, `[When]` and `[Then]` attributes (#663)
* Use scenario names for generated MsTest tests instead of the method name (#588)
* Ensure that the runtime plugins are loaded in an alphabetic order based on file name on Unix-based platforms. (#519)
* Warnings can be logged during test generation (build) to indicate potential issues (#624)
* Upgrade to Gherkin v34 from v30 (see [Gherkin changelog](https://github.com/cucumber/gherkin/blob/main/CHANGELOG.md)) (#489)
* Improved test feature context and feature hook handling for non-parallel or class-parallel scenarios where the scenarios of the feature are not executed in sequence (#638)
* Introduced a new `BuildMetadata` class to encapsulate CI metadata properties such as `ProductName`, `BuildUrl`, `BuildNumber`, `Remote`, `Revision`, `Branch`, and `Tag`. These will be used to populate data in reports (e.g. html and message formatter). (#658)
* Updated Reqnroll project template to add TUnit test framework support, to remove EOL .NET versions (6.0, 7.0), and to add .NET 9.0 support (#701)
* Allow detecting skipped or pending execution status by the unit test providers, treat `NotImplementedException` as "pending" (#732)
* Allow `ScenarioContext`, `FeatureContext` and `TestThreadContext` to be resolved or injected through their interfaces, e.g. `IScenarioContext` (#761)
* Updated step definition template that is shown on test output when an undefined step is executed by using `PendingStepException` and `IReqnrollOutputHelper`. (#781)
* Improved performance of test execution by optimizing telemetry sending (#629)

## Improvements for plugins:

* Refactored `ExecutionEvent` classes and event publishing. Events carry more context information; publication is now async. (#621)
* Provide `AsyncEventHandler` in `RuntimePluginTestExecutionLifecycleEvents` (#634)

## Bug fixes:

* Fix: Exception in an `AfterFeature` hook causes the next first test failure in the next feature (#597)
* Fix: Disposed `ObjectContainer` can be accessed through `RegisterInstanceAs`/`RegisterFactoryAs`/`RegisterTypeAs` (#594)
* Fix: Namespace clash in generated files if no `RootNamespace` is defined in the project file (#633)
* Fix: Missing source link and deterministic compilation for `Reqnroll.CustomPlugin` package (#719)
* Fix: Rule tags are not generated as test categories (#731)

## Deprecations:

* The synchronous test runner API (`ISyncTestRunner`) has been removed. Please use `ITestRunner` instead.
* The synchronous code invocation API (`IBindingInvoker`) has been deprecated. Please use `IAsyncBindingInvoker` instead.
* Removed obsolete property `ScenarioInfo.ScenarioAndFeatureTags`. Please use `ScenarioInfo.CombinedTags` instead.
* Removed obsolete methods on `Reqnroll.Assist.Service` class: `RegisterValueComparer`, `UnregisterValueComparer`, `RegisterValueRetriever`, `UnregisterValueRetriever` use `ValueComparers.Register`, `ValueComparers.Unregister`, `ValueRetrievers.Register`, `ValueRetrievers.Unregister` instead.
* The methods `ScenarioContext.Pending` and `ScenarioContext.StepIsPending` have been deprecated and going to be removed in v4. Use `throw new PendingStepException()` instead.
* Removed unused `Culture` property from step definition attributes (`Given`, `When`, `Then`, `StepDefinition`) (#671)
* Removed `[Serializable]` from Reqnroll exceptions (#738)
* Removed support for end-of-life .NET frameworks (.NET 6, .NET 7, .NET Core) (#706)
* Removed deprecated `<summary>` from NuGet packages (#766)

## Breaking changes:

* Renamed `Regex` property and `regex` constructor parameter for all step definition attributes to `Expression` and `expression`, as it represents a cucumber expression or a regular expression (regex) that matches the step text. (#639)
* Removed unused `Culture` property from step definition attributes (`Given`, `When`, `Then`, `StepDefinition`) (#671)
* Use scenario names for generated MsTest tests instead of the method name (#588)

*Contributors of this release (in alphabetical order):* @304NotModified, @AdaskoTheBeAsT, @algirdasN, @clrudolphi, @DrEsteban, @gasparnagy, @loraderon, @obligaron

# v2.4.1 - 2025-04-29

## Bug fixes:

* Fix: xUnit async `[AfterTestRun]` hook might not execute fully (#530)
* Fix: Scenario, feature and test run finished event is not published when the related "after" hook fails (#560)
* Fix: Inconsistent hook execution (double execution, before/after hook skipped, infrastructure errors) when before or after hooks fail (#526)
* Fix: Namespace collisions in generated code when Reqnroll project namespace contains "System" (#583)
* Fix: InvalidOperationException when calling test teardown method after the Reqnroll test runner has been released (#387)

*Contributors of this release (in alphabetical order):* @304NotModified, @clrudolphi, @gasparnagy, @obligaron 

# v2.4.0 - 2025-03-06

## Improvements:

* Microsoft.Extensions.DependencyInjection.ReqnrollPlugin: Improved message when [ScenarioDependencies] can't be found or has an incorrect return type (#494)
* Include original exception for binding errors (`BindingException`) (#513)
* Map data table columns to constructor parameters without having a related property for data table "assist" helpers (e.g. `CreateInstance`). To use this feature you need to set the `InstanceCreationOptions.RequireTableToProvideAllConstructorParameters` flag. (#488)

## Bug fixes:

* Fix: Microsoft.Extensions.DependencyInjection.ReqnrollPlugin, the plugin was only searching for [ScenarioDependencies] in assemblies with step definitions (#477)
* Fix: xUnit Conservative Mode is not supported together with xUnit v2 (#473)
* Fix: Messages logged through `IReqnrollOutputHelper` are added to the output with a `->` prefix that should be reserved for output messages of Reqnroll itself (#504)

*Contributors of this release (in alphabetical order):* @304NotModified, @AroglDarthu, @DerAlbertCom, @gasparnagy, @obligaron, @Socolin

# v2.3.0 - 2025-02-11

## Improvements:

* Enhance BoDi error handling to provide the name of the interface being registered when that interface has already been resolved (#324)
* Improve code-behind feature file compilation speed (#336)
* Improve parameter type naming for generic types (#343)
* Reqnroll.Autofac: Add default registration for IReqnrollOutputHelper (#357)
* Reduced MsBuild log output and consistent use of [Reqnroll] prefix (#381)
* Update behavior of `ObjectContainer.IsRegistered()` to check base container for registrations, to match `Resolve()` behavior (#367)
* Replaced custom approach for avoiding namespace collisions with .net idiomatic approach
* Support loading plugin dependencies from .deps.json on .NET Framework and Visual Studio MSBuild (#408)
* Support for setting `ObjectContainer.DefaultConcurrentObjectResolutionTimeout` even after creation of the container (#435)
* Reqnroll.Microsoft.Extensions.DependencyInjection: Include `ReqnrollLogger` class to the Reqnroll MSDI plugin based on the work of @StefH at https://github.com/StefH/Stef.Extensions.SpecFlow.Logging (#321)
* Reqnroll.Assist.Dynamic: The SpecFlow.Assist.Dynamic plugin by @marcusoftnet has now been ported to Reqnroll. (#377)

## Bug fixes:

* Fix: MsTest: Output is written to Console.WriteLine additionally instead of using TestContext only (#368) 
* Fix: Deprecated dependency `Specflow.Internal.Json` is used. Relpaced with `System.Text.Json`. The dependency was used for laoding `reqnroll.json`, for Visual Studio integration and for telemetry. (#373)
* Fix: Error with NUnit 4: "Only static OneTimeSetUp and OneTimeTearDown are allowed for InstancePerTestCase mode" (#379)
* Fix: Reqnroll.Autofac: FeatureContext cannot be resolved in BeforeFeature/AfterFeature hooks (#340)
* Fix: Attempting to set the `ConcurrentObjectResolutionTimeout` value on the `ObjectContainer` to `TimeSpan.Zero` sometimes throws an exception if running multiple tests in parallel. (#440)
* Fix: Project and Package references of Reqnroll.Verify are inconsistent. (#446)

*Contributors of this release (in alphabetical order):* @Antwane, @clrudolphi, @DrEsteban, @gasparnagy, @obligaron, @olegKoshmeliuk, @SeanKilleen, @StefH

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
