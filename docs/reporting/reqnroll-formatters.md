# Reqnroll formatters

Reqnroll provides a *formatter* infrastructure, similar to [Cucumber formatters](https://cucumber.io/docs/cucumber/reporting/#built-in-reporter-plugins). The formatters can be used to generate reports of the test execution. Reqnroll provides built-in formatters ([HTML](#html-formatter), [Cucumber Messages](#cucumber-messages-formatter)) and can be extended with custom formatters.

In order to generate a report with a formatter, you need to enable it. You can enable multiple formatters as well. The easiest way to enable a formatter is to add a `formatters` section to the `reqnroll.json` configuration file.

The following example enables the HTML formatter and configures the output file as `reqnroll_report.html`.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

    "formatters": {
        "html" : { "outputFilePath" : "reqnroll_report.html" }
    }
}
```

See [](../installation/formatter-configuration.md) for further details about formatter configuration.

## HTML formatter

The *HTML formatter* generates a stand-alone HTML file with all executed feature files and the test execution results. 

The result can also be used as a [Living Documentation](https://johnfergusonsmart.com/living-documentation-not-just-test-reports/) and it is the default replacement of the `SpecFlow+ LivingDoc Generator`.

The HTML generator uses the [Cucumber React Components](https://github.com/cucumber/react-components) and the [Cucumber HTML Formatter](https://github.com/cucumber/html-formatter) to render the feature files and the test results to HTML.

```{note}
Currently the HTML formatter does not allow customizations or templating. We plan to add customization options in future releases. 

With this release, customizations can be done either by a custom formatter (based on the [HTML formatter implementation](https://github.com/reqnroll/Reqnroll/blob/main/Reqnroll/Formatters/Html/HtmlFormatter.cs)) or by using the [Cucumber Message formatter](#cucumber-messages-formatter) to generate a Cucumber Messages report and use custom tooling to generate a HTML report from it (see [this sample project](https://github.com/gasparnagy/cucumber-html-formatter-cli) as an example).
```


The HTML formatter can be enabled with a `html` section in the configuration file within `formatters`. By default, it generates a `reqnroll_report.html` file to the project output folder (e.g., `bin/Debug/net8.0/`), but this can be configured using the `outputFilePath` setting.

The following configuration file enables the HTML formatter and generates the report to the `report/living_doc.html` file within the project output folder.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

    "formatters": {
        "html" : { "outputFilePath" : "report/living_doc.html" }
    }
}
```

## Cucumber Messages formatter

The *Cucumber Messages formatter* generates a Cucumber Messages (`.ndjson` or `.jsonl`) file. A Cucumber Messages file is a standardized file format for Cucumber-based test tooling (like Reqnroll) to feed test results to reporting systems.

```{note}
For more information about Cucumber Messages and the tooling that consumes them, please see the [Cucumber Messages](https://github.com/cucumber/messages) page on Github.
```

The Cucumber Messages formatter can be enabled with a `message` section in the configuration file within `formatters`. By default, it generates a `reqnroll_report.ndjson` file to the project output folder (e.g., `bin/Debug/net8.0/`), but this can be configured using the `outputFilePath` setting.

The following configuration file enables the Cucumber Messages formatter and generates the report to the `report/cucumber_messages.ndjson` file within the project output folder.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

    "formatters": {
        "message" : { "outputFilePath" : "report/cucumber_messages.ndjson" }
    }
}
```

## Creating a custom formatter

```{warning}
The formatter infrastructure is new, therefore the interface details and the implementation rules might change significantly even in minor version changes. 
```

To create a custom formatter, you need to create a Reqnroll [Runtime plugin](../extend/plugins.md#runtime-plugins), create a class that implements the `ICucumberMessageFormatter` interface and register it to the test run (global) DI container with a new name. This name must be the same as returned by the `ICucumberMessageFormatter.Name` property of the formatter implementation.

The following example registers the `CustomFormatter` with the name `custom`.

```{code-block} c#
using Reqnroll.Formatters;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using ReqnrollFormatters.Custom;

[assembly: RuntimePlugin(typeof(CustomFormatterPlugin))]

namespace ReqnrollFormatters.Custom;

public class CustomFormatterPlugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.RegisterGlobalDependencies += (_, args) =>
        {
            args.ObjectContainer.RegisterTypeAs<CustomFormatter, ICucumberMessageFormatter>("custom");
        };
    }
}
```

The created formatter can be enabled with a `custom` section in the configuration file within `formatters`.


```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

    "formatters": {
        "custom" : { "outputFilePath" : "custom_report.txt" }
    }
}
```

For a complete example that contains a custom formatter, please check our [Custom Formatter Test Project](https://github.com/reqnroll/Reqnroll.ExploratoryTestProjects/tree/main/ReqnrollFormatters/ReqnrollFormatters.Custom).
