# Formatter Configuration

```{note}
Reqnroll formatters are only available in Reqnroll v3.0 or later.
```

There are two ways to configure [Reqnroll Formatters](../reporting/reqnroll-formatters.md).

* [](#configuration-file)
* [](#environment-variables)

## Defaults

Unless overwritten by using the Reqnroll configuration file and/or environment variable, Reqnroll will use the following defaults to configure formatters.

```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - Enabled
  - true/false
  - Controls whether Reqnroll formatters will be enabled during test execution. <br/> *Default:* `true`
```

## Configuration File

The `formatters` section of the [`reqnroll.json` configuration file](configuration.md) can be used to configure formatters. Each section within `formatters` enables and configures a built-in or custom formatter. You can enable multiple formatters.

The following example enables both the [HTML](../reporting/reqnroll-formatters.md#html-formatter) and the [Cucumber Message](../reporting/reqnroll-formatters.md#cucumber-messages-formatter) formatter with custom output file paths.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

    "bindingAssemblies": [
    ],
    "formatters": {
        "html" : { "outputFilePath" : "report\\living_doc.html" },
        "message" : { "outputFilePath" : "report\\cucumber_messages.ndjson" }
    }
}
```

If the formatter section is omitted, the report for that particular formatter is not enabled.

## Environment Variables

The settings discussed above can be overridden by setting an environment variable. When an environment variable is set, it takes precedence over the same configuration setting in the configuration file. If a setting is not overridden by an environment variable, the value will be taken from the configuration file (if set), otherwise a default (as shown above) will be used.

The available Environment Variables are:

* REQNROLL_FORMATTERS_DISABLED
* REQNROLL_FORMATTERS

```{note}
When using an environment variable to overwrite the `formatters` section of the `reqnroll.json` configuration file, the value of the environment variable replaces the `formatters` element in its entirety.
```
