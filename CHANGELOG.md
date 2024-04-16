# [vNext]

* Fix for #44 in which user code namespaces that included "Reqnroll" within them caused the code generation to fail
* Include built-in dependency injection framework (BoDi) to the main repository as "Reqnroll.BoDi" based on v1.5 of [BoDi](https://github.com/SpecFlowOSS/BoDi/)
* Resolve dependencies of [BeforeTestRun] / [AfterTestRun] hooks from the 
  test run (global) context instead of the test thread context.
* Support for PriorityAttribute in MsTest adapter
* Support for Scenario Outline / DataRowAttribute in MsTest adapter
* Fix for #81 in which Cucumber Expressions fail when two enums with the same short name (differing namespaces) are used as parameters

# v1.0.1 - 2024-02-16

* Fix: Error when installing the Reqnroll template via command line (#22)

# v1.0.0 - 2024-02-05

* Support for .NET 8 projects
* Initial release based on v4.0.31-beta of [SpecFlow](https://github.com/SpecFlowOSS/SpecFlow/).
