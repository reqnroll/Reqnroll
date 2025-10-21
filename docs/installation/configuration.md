# Configuration

Reqnroll can be [setup](setup-project) simply by adding a NuGet package to your project and in the most of the cases there is no additional configuration required.

The default configuration can be altered by adding a `reqnroll.json` configuration file to your project. An empty configuration file can be added using the *Add / New Item...* command of Visual Studio 2022 or using the Reqnroll .NET item template. The following example downloads the Reqnroll templates and adds a configuration file to the project.

```{code-block} pwsh
:caption: .NET CLI
dotnet new install Reqnroll.Templates.DotNet
dotnet new reqnroll-config
```

You can also start by adding the following empty configuration file to your project.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json"
}
```

```{tip}
There is a JSON schema file available for `reqnroll.json`. By specifying the schema reference like in the example above, most IDE (including Visual Studio and Visual Studio Code) provides auto completion and documentation hints for the configuration file.
```

In this guide we show examples for the most common situations when you need to modify the config file followed by a full configuration reference.

* [](#use-bindings-from-external-projects)
* [](#set-the-default-feature-file-language)
* [](#config-reference)

{#use-bindings-from-external-projects}
## Use bindings from external projects

In order to use bindings (step definitions, hooks or step argument transformations) from other projects (called *external projects*) it is not enough to add a project reference to the Reqnroll project, but you need to also configure Reqnroll to search bindings in these projects. See [](../automation/bindings-from-external-assemblies) for further details.

This can be achieved by listing the *assembly name* of the external project to the `bindingAssemblies` section of the configuration file.

The following example registers the project `SharedStepDefinitions` as an external binding assembly.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "bindingAssemblies": [
    { 
      "assembly": "SharedStepDefinitions"
    }
  ]
}
```

{#set-the-default-feature-file-language}
## Set the default feature file language

The keywords in the [feature files](../gherkin/feature-files.md) are available in many many natural languages matching the language your business is using. 

In order to use the keywords in a language other than English, you can either use the [Gerkin `#language` directive](../gherkin/feature-language.md) in every feature file or specify a default language in the Reqnroll configuration.

The following example sets the default feature language to Hungarian:

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "language": {
    "feature": "hu-HU"
  }
}
```

{#config-reference}
## Configuration file reference

The following configuration sections are available for `reqnroll.json`.

{#language}
### `language`

Use this section to define the default language for feature files and other language-related settings. For more details on language settings, see [](../gherkin/feature-language).

```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - feature
  - culture name (`en-US`)
  - The default language of feature files added to the project. We recommend using specific culture names (e.g.: `en-US`) rather than generic (neutral) cultures (e.g.: `en`). <br/> *Default:* `en-US`
* - binding
  - culture name (`en-US`)
  - Specifies the culture to be used to execute binding methods and convert step arguments. If not specified, the feature language is used. <br/> *Default:* not specified
```

### `generator`

Use this section to define test generation options.

```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - allowDebugGeneratedFiles
  - `true`/`false`
  - By default, the debugger is configured to step through the generated code. This helps you debug your feature files and bindings (see [Debugging Tests](../execution/debugging)). Disabled this option by setting this attribute to `true`.<br/> *Default:* `false`
* - allowRowTests
  - `true`/`false`
  - Determines whether "row tests" should be generated for [scenario outlines](../gherkin/gherkin-reference.md#scenario-outline). This setting is ignored if the [test execution framework](setup-project.md#choosing-your-test-execution-framework) does not support row based testing. <br/> *Default:* `true`
* - addNonParallelizableMarkerForTags
  - List of tags
  - Defines a set of tags that mark tests as exclusive (non-parallelizable). If the tag appears on a feature, the whole generated test class is marked non-parallelizable. If the tag appears only on a scenario (and not on the feature), the generated test method is marked non-parallelizable on frameworks that support scenario/method level isolation (currently NUnit, MsTest V2 and TUnit). See [](../execution/parallel-execution).<br/> *Default:* empty
* - disableFriendlyTestNames
  - `true`/`false`
  - Option available in Reqnroll versions **greater than v3.0.3**. <br/> Determines whether generated tests will contain a `"DisplayName"` in their test attributes with the name of the Scenario. Friendly names are easier to read in test reports, but may cause issues with some test runners or CI systems. Friendly display names are currently only supported by MSTest and xUnit. <br/> *Default:* `false`
```

### `runtime`

Use this section to specify various test execution options.

```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - missingOrPendingStepsOutcome
  - `Pending` / `Inconclusive` / `Ignore` / `Error`
  - Determines how Reqnroll behaves if a step binding is not implemented or pending. See [](../execution/test-results).<br/> *Default:* `Pending`
* - obsoleteBehavior
  - `None` / `Warn` / `Pending` / `Error`
  - Determines how Reqnroll behaves if a step binding is marked with `[Obsolete]` attribute.<br/> *Default:* `Warn`
* - stopAtFirstError
  - `true`/`false`
  - Determines whether the execution of the scenario should stop when encountering the first error, or whether it should attempt to try and match subsequent steps (in order to detect missing steps). <br/> *Default:* `false`
```

### `trace`

Use this section to determine the Reqnroll trace output.

```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - stepDefinitionSkeletonStyle
  - `CucumberExpressionAttribute` / `RegexAttribute` / `AsyncCucumberExpressionAttribute` / `AsyncRegexAttribute`
  - Specifies the default [step definition style](step-matching-styles-rules).<br/> *Default:* `CucumberExpressionAttribute`
* - coloredOutput
  - `true`/`false`
  - Determine whether Reqnroll should color the test result output. See [](../execution/color-output) for more details. You can override this setting to disable color (e.g. on build servers), with the environment variable `NO_COLOR=1`<br/> *Default:* `false`
```

### `bindingAssemblies`

This section can be used to configure additional assemblies that contain bindings (step definitions, hooks or step argument transformations). See [](../automation/bindings-from-external-assemblies) for further details.

The assembly of the Reqnroll project (the project containing the feature files) is automatically included. The binding assemblies must be placed in the output folder (e.g. bin/Debug) of the Reqnroll project, for example by adding a reference to the assembly from the project.

The following example registers an additional binding assembly (`SharedStepDefinitions.dll`).

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "bindingAssemblies": [
    { 
      "assembly": "SharedStepDefinitions"
    }
  ]
}
```

The `bindingAssemblies` section can contain multiple JSON objects (one for each assembly), with the following settings.

```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - assembly
  - assembly name
  - The name of the assembly containing bindings (without `.dll`).
```

### `formatters`

This section can be used to configure [Reqnroll Formatters](../reporting/reqnroll-formatters.md). See [](formatter-configuration.md) for further details.


```{toctree}
:maxdepth: 1

formatter-configuration
configuring-build
