# Configuration

Reqnroll's behavior can be configured extensively. How to configure Reqnroll depends on the version of Reqnroll you are using.

Note: bear in mind that, although this article is meant to address features default language configuration, you may define  language directly on the top of your feature. For further details, refer to [Gerkin's #language directive]( https://docs.reqnroll.net/projects/reqnroll/en/latest/Gherkin/Feature-Language.html)

## SpecFlow 3.x

Starting with SpecFlow 3, you can use the `reqnroll.json` file to configure it. It is mandatory for .NET Core projects and it is recommended for .NET Framework projects.  
When using the .NET Framework, you can still use the `app.config` file, as with earlier versions of Reqnroll.

If both the `reqnroll.json` and `app.config` files are available in a project, `reqnroll.json` takes precedence.

Please make sure that the **Copy to Output Directory property** of `reqnroll.json` is set to either **Copy always** or **Copy if newer**. Otherwise `reqnroll.json` might not get copied to the Output Directory, which results in the configuration specified in `reqnroll.json` taking no effect during test execution.

## SpecFlow 2.x

SpecFlow 2 is configured in your standard .NET configuration file, `app.config`, which is automatically added to your project. This method is not supported by .NET Core, and SpecFlow 2 does not include .NET Core support.

We recommend using `reqnroll.json` in new projects.

## Configuration Options

Both configuration methods use the same options and general structure. The only difference is that SpecFlow 2 only uses the `app.config` file (XML) and SpecFlow 3 requires the `reqnroll.json` file (JSON) for .NET Core projects.

### Configuration examples

The following 2 examples show the same option defined in the `reqnroll.json` and `app.config` in formats:

**reqnroll.json example:**

```json
{
  "$schema": "https://reqnroll.net/reqnroll-config.json",
  "language": {
    "feature": "de-AT"
  }
}
```

**app.config example:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="reqnroll" type="Reqnroll.Configuration.ConfigurationSectionHandler, Reqnroll" />
  </configSections>
  <reqnroll>
    <language feature="de-AT" />
  </reqnroll>
</configuration>
```

You can find more examples in the [sample projects](https://github.com/reqnroll/Reqnroll-Examples) for Reqnroll.

## Default Configuration

All Reqnroll configuration options have a default setting. Simple Reqnroll projects may not require any further configuration.

## Configuring Your Unit Test Provider

### SpecFlow 3

You can only configure your unit provider by adding the corresponding packages to your project. You will therefore need to add **one** of the following NuGet packages to your project to configure the unit test provider:

- SpecRun.Reqnroll
- Reqnroll.xUnit
- Reqnroll.MsTest
- Reqnroll.NUnit

**Note: Make sure you do not add more than one of the unit test plugins to your project. If you do, an error message will be displayed.**

### SpecFlow 2

SpecFlow 2 is configured using the `app.config` file (Full Framework only). Enter your unit test provider in the `unitTestProvider` element in the `reqnroll` section, e.g.:

```xml
  <reqnroll>
    <unitTestProvider name="MsTest" />
  </reqnroll>
```

## Configuration Elements

The same configuration elements are available in both the XML (`app.config`) and JSON (`reqnroll.json`) formats.

### `language`

Use this section to define the default language for feature files and other language-related settings. For more details on language settings, see [Feature Language](../Gherkin/Feature-Language.md).

| Attribute | Value                  | Description                                                                                                                                                                                                                                                             |
| --------- | ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| feature   | culture name (“en-US”) | The default language of feature files added to the project. We recommend using specific culture names (e.g.: “en-US”) rather than generic (neutral) cultures (e.g.: “en”). <br/> **Default:** en-US                                                                     |
| tool      | empty or culture name  | Specifies the language that Reqnroll uses for messages and tracing. Uses the default feature language if empty and that language is supported; otherwise messages are displayed in English. (<b>Note:</b> Only English is currently supported.)<br/> **Default:** empty |

### `bindingCulture`

Use this section to define the culture for executing binding methods and converting step arguments. For more details on language settings, see [Feature Language](../Gherkin/Feature-Language.md).

| Attribute | Value                  | Description                                                                                                                                                             |
| --------- | ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| name      | culture name (“en-US”) | Specifies the culture to be used to execute binding methods and convert step arguments. If not specified, the feature language is used.<br/> **Default:** not specified |

### `generator`

Use this section to define unit test generation options.

| Attribute                         | Value        | Description                                                                                                                                                                                                                                                                                                                                                                    |
| --------------------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| allowDebugGeneratedFiles          | true/false   | By default, the debugger is configured to step through the generated code. This helps you debug your feature files and bindings (see [Debugging Tests]()). Disabled this option by setting this attribute to “true”.<br/> **Default:** false                                                                                                                                   |
| allowRowTests                     | true/false   | Determines whether "row tests" should be generated for [scenario outlines](https://docs.reqnroll.net/projects/reqnroll/en/latest/Gherkin/Gherkin-Reference.html). This setting is ignored if the [unit test framework](https://docs.reqnroll.net/projects/reqnroll/en/latest/Installation/Unit-Test-Providers.html) does not support row based testing.<br/> **Default:** true |
| addNonParallelizableMarkerForTags | List of tags | Defines a set of tags, any of which specify that a feature should be excluded from running in parallel with any other feature. See [Parallel Execution](../Execution/Parallel-Execution.md).<br/> **Default:** empty                                                                                                                                                           |

### `runtime`

Use this section to specify various test execution options.

| Attribute                    | Value                             | Description                                                                                                                                                                                                      |
| ---------------------------- | --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| missingOrPendingStepsOutcome | Pending/Inconclusive/Ignore/Error | Determines how Reqnroll behaves if a step binding is not implemented or pending. See [Test Result](https://docs.reqnroll.net/projects/reqnroll/en/latest/Execution/Test-Results.html).<br/> **Default:** Pending |
| obsoleteBehavior             | None/Warn/Pending/Error           | Determines how Reqnroll behaves if a step binding is marked with [Obsolete] attribute.<br/> **Default:** Warn                                                                                                    |
| stopAtFirstError             | true/false                        | Determines whether the execution of the Scenario should stop when encountering the first error, or whether it should attempt to try and match subsequent steps (in order to detect missing steps).<br/> **Default:** false       |

### `trace`

Use this section to determine the Reqnroll trace output.

| Attribute                   | Value                                                                     | Description                                                                                                                                                                                                                                                                                                                                   |
|-----------------------------|---------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| traceSuccessfulSteps        | true/false                                                                | Determines whether Reqnroll should trace successful step binding executions. <br/>**Default:** true                                                                                                                                                                                                                                           |
| traceTimings                | true/false                                                                | Determines whether Reqnroll should trace execution time of the binding methods (only if the execution time is longer than the minTracedDuration value).<br/>**Default:** false                                                                                                                                                                |
| minTracedDuration           | TimeSpan (0:0:0.1)                                                        | Specifies a threshold for tracing the binding execution times.<br/>**Default:** 0:0:0.1 (100 ms)                                                                                                                                                                                                                                              |
| stepDefinitionSkeletonStyle | CucumberExpressionAttribute/RegexAttribute/MethodNameUnderscores/MethodNamePascalCase/MethodNameRegex | Specifies the default [step definition style](../Bindings/Step-Definitions.html#step-matching-styles-rules).<br/>**Default:** CucumberExpressionAttribute (from v4), RegexAttribute (in v3 or earlier)                                                                                                                                                                                                 |
| coloredOutput               | true/false                                                                | Determine whether Reqnroll should color the test result output. See [Color Test Result Output](../Execution/Color-Output.md) for more details<br/>**Default:** false<br/>When this setting is enable you can disable color, for example to run it on a build server that does not support colors, with the environment variable `NO_COLOR=1` |

### `stepAssemblies`

This section can be used to configure additional assemblies that contain [external binding assemblies](../Bindings/Use-Bindings-from-External-Assemblies.md). The assembly of the Reqnroll project (the project containing the feature files) is automatically included. The binding assemblies must be placed in the output folder (e.g. bin/Debug) of the Reqnroll project, for example by adding a reference to the assembly from the project.

The following example registers an additional binding assembly (MySharedBindings.dll).

**reqnroll.json example:**

```json
{
  "$schema": "https://reqnroll.net/reqnroll-config.json",
  "stepAssemblies": [
    {
      "assembly": "MySharedBindings"
    }
  ]
}
```

**app.config example:**

```xml
<reqnroll>
  <stepAssemblies>
    <stepAssembly assembly="MySharedBindings" />
  </stepAssemblies>
</reqnroll>
```

The `<stepAssemblies>` can contain multiple `<stepAssembly>` elements (one for each assembly), with the following attributes.

| Attribute | Value         | Description                                   |
| --------- | ------------- | --------------------------------------------- |
| assembly  | assembly name | The name of the assembly containing bindings. |
