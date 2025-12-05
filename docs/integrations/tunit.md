# TUnit

Reqnroll supports TUnit 1.3.25 or later.

Documentation for TUnit can be found [here](https://tunit.dev/).

## Supported .NET Versions

TUnit with Reqnroll supports the following .NET versions:

- .NET 8.0 or later
- .NET Framework 4.6.2 or later

## Needed NuGet Packages

For Reqnroll: [Reqnroll.TUnit](https://www.nuget.org/packages/Reqnroll.TUnit/)

For TUnit: [TUnit](https://www.nuget.org/packages/TUnit/)

## Access TestContext

The TUnit test context (`TUnit.Core.TestContext`) is registered in the scenario dependency scope. You can get access to it via [Context-Injection](../automation/context-injection.md) when needed.

## Parallel Execution

TUnit supports test-level (scenario-level) parallel test execution by default. The parallel execution can be disabled for the entire test project using the `[assembly: TUnit.Core.NotInParallel]` attribute or use [](../execution/parallel-execution.md#excluding-reqnroll-features-from-parallel-execution).

## .NET 10 SDK Compatibility

TUnit uses Microsoft.Testing.Platform which dropped VSTest support in .NET 10 SDK. If you encounter the error "Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later", you need to enable the new dotnet test experience by adding the following property to your project file:

```xml
<PropertyGroup>
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>
```

For more information, see [Microsoft's documentation on the Microsoft.Testing.Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform).
