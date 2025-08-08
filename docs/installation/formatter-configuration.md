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
  - `true`
  - Controls whether Reqnroll formatters will be enabled during test execution.
* - HTML Formatter outputFilePath
  - `reqnroll_report.html`
  - Default output file path for the HTML formatter (relative to project output folder)
* - Message Formatter outputFilePath
  - `reqnroll_report.ndjson`
  - Default output file path for the Cucumber Messages formatter (relative to project output folder)
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

### Built-in Formatter Configuration Examples

#### Enabling Formatters with Default Settings

To enable a formatter with its default settings, simply include it in the `formatters` section without any configuration:

```{code-block} json
:caption: reqnroll.json - Enable HTML formatter with defaults
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "html": {}
  }
}
```

```{code-block} json
:caption: reqnroll.json - Enable Message formatter with defaults
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "message": {}
  }
}
```

```{code-block} json
:caption: reqnroll.json - Enable both formatters with defaults
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "html": {},
    "message": {}
  }
}
```

#### Overriding Output File Path Components

You can override different parts of the output file path:

**Override directory only (keep default filename):**
```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "html": { "outputFilePath": "reports\\" },
    "message": { "outputFilePath": "reports\\" }
  }
}
```

**Override filename only (use current directory):**
```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "html": { "outputFilePath": "my_report.html" },
    "message": { "outputFilePath": "my_messages.ndjson" }
  }
}
```

**Override both directory and filename:**
```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",
  "formatters": {
    "html": { "outputFilePath": "output\\detailed_report.html" },
    "message": { "outputFilePath": "output\\test_messages.ndjson" }
  }
}
```

## Environment Variables

The settings discussed above can be overridden by setting an environment variable. When an environment variable is set, it takes precedence over the same configuration setting in the configuration file. If a setting is not overridden by an environment variable, the value will be taken from the configuration file (if set), otherwise a default (as shown above) will be used.

### Available Environment Variables

#### REQNROLL_FORMATTERS_DISABLED

**Description:** Disables the entire formatter subsystem when set to `true`. When disabled, no formatters will run regardless of configuration file settings.

**Default Value:** `false` (formatters are enabled by default)

**Behavior:** 
- When set to `true`: All formatters are disabled, no report files will be generated
- When set to `false` or not set: Formatters operate according to configuration file settings

**Usage Examples:**
```bash
# Disable all formatters
export REQNROLL_FORMATTERS_DISABLED=true

# Enable formatters (default behavior)
export REQNROLL_FORMATTERS_DISABLED=false
```

#### REQNROLL_FORMATTERS

**Description:** Allows overriding the entire `formatters` section of the `reqnroll.json` configuration file using JSON format.

**Default Value:** Not set (uses configuration file settings)

**Behavior:** When set, completely replaces the `formatters` section from the configuration file.

```{note}
When using an environment variable to overwrite the `formatters` section of the `reqnroll.json` configuration file, the value of the environment variable replaces the `formatters` element in its entirety.
```

### Environment Variable Configuration Examples

#### Enable HTML formatter with default settings:
```bash
export REQNROLL_FORMATTERS='{"formatters": {"html": {}}}'
```

#### Enable Message formatter with default settings:
```bash
export REQNROLL_FORMATTERS='{"formatters": {"message": {}}}'
```

#### Enable both formatters with custom output paths:
```bash
export REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "reports/test_report.html"}, "message": {"outputFilePath": "reports/test_messages.ndjson"}}}'
```

#### Enable HTML formatter with custom directory only:
```bash
export REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "test-results/"}}}'
```

#### Enable formatters with different configurations:
```bash
# Windows Command Prompt
set REQNROLL_FORMATTERS={"formatters": {"html": {"outputFilePath": "output\\report.html"}, "message": {"outputFilePath": "output\\messages.ndjson"}}}

# PowerShell
$env:REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "output/report.html"}, "message": {"outputFilePath": "output/messages.ndjson"}}}'

# Linux/macOS Bash
export REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "output/report.html"}, "message": {"outputFilePath": "output/messages.ndjson"}}}'
```
