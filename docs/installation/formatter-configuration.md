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

The settings discussed above can be overridden by setting an environment variable. When an environment variable is set, it takes precedence over the same configuration setting in the configuration file. If a setting is not overridden by an environment variable, the value will be taken from the configuration file (if set), otherwise a default (as shown above) will be used. The formatter specific environment variables override the general `REQNROLL_FORMATTERS` environment variable settings. 

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

#### REQNROLL_FORMATTERS_*formatter*

**Description:** Overrides the configuration of a specific formatter using key-value pair settings. For example the `REQNROLL_FORMATTERS_HTML` environment variable can be used to configure the [html formatter](../reporting/reqnroll-formatters.md#html-formatter). 

**Default Value:** Not set (uses configuration file settings)

**Behavior:** 

* When set to `true` it enables the formatter with default settings
* When set to `false` it disables the formatter (if it was configured in the configuration file)
* When set to `setting1=value1;setting2=value2` it configures the formatter with the specified settings and values. For example the value `outputFilePath=result.html` sets the output file to `result.html`.

#### REQNROLL_FORMATTERS

**Description:** Overrides the `formatters` section of the `reqnroll.json` configuration file using JSON format.

**Default Value:** Not set (uses configuration file settings)

**Behavior:** When set, replaces the named `formatters` sub-section(s) from the configuration file.

```{note}
When using an environment variable to override a `formatters` section, the value of the environment variable must be properly escaped (appropriate to your shell) to remain a valid json representation of the configuration setting.
```

### Environment Variable Configuration Examples

#### Enable HTML formatter with default settings:

```bash
export REQNROLL_FORMATTERS_HTML='true'
```

#### Enable HTML formatter with custom output path:

```bash
export REQNROLL_FORMATTERS_HTML='outputFilePath=result.html'
```

#### Enable HTML formatter with custom directory only:

```bash
export REQNROLL_FORMATTERS_HTML='outputFilePath=test-results/'
```

This setting will generate the HTML report in the specified folder with the default file name (`test-results/reqnroll_report.html`).

#### Enable Message formatter with default settings:

```bash
export REQNROLL_FORMATTERS_MESSAGE='true'
```

#### Enable both formatters with custom output paths using JSON:

```bash
export REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "reports/test_report.html"}, "message": {"outputFilePath": "reports/test_messages.ndjson"}}}'
```

#### Set JSON value in different shells with correct escaping:

```bash
# Windows Command Prompt
set REQNROLL_FORMATTERS={"formatters": {"html": {"outputFilePath": "output\\report.html"}, "message": {"outputFilePath": "output\\messages.ndjson"}}}

# PowerShell
$env:REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "output/report.html"}, "message": {"outputFilePath": "output/messages.ndjson"}}}'

# Linux/macOS Bash
export REQNROLL_FORMATTERS='{"formatters": {"html": {"outputFilePath": "output/report.html"}, "message": {"outputFilePath": "output/messages.ndjson"}}}'
```
