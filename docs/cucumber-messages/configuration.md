# Cucumber Messages Configuration

There are two ways to configure Cucumber Messages support within Reqnroll.

* [](#config-file)
* [](#environment-variable)

## Defaults
Unless overriden by the use of the Reqnroll configuration file and/or environment variable, Reqnroll will use the following defaults to configure support for Cucumber Messages.
```{list-table}
:header-rows: 1

* - Setting
  - Value
  - Description
* - Enabled
  - true/false
  - Controls whether Cucumber Messages will be created during the execution of the test. <br/> *Default:* `false`
```

{#config-file}
## Configuration File
Reqnroll will use values from the reqnroll.json configuration file to control the above settings. To use the configuration file, add a json object to your project's reqnroll.json file.
An example configuration file looks like this:

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

    "bindingAssemblies": [
    ],
    "formatters": {
        "messages" : { "outputFilePath" : "C:\\Users\\dev\\source\\repos\\reqnroll_project\\CucumberMessages\\reqnroll_report.ndjson" },
        "html" : { "outputFilePath" : "C:\\Users\\dev\\source\\repos\\reqnroll_project\\CucumberMessages\\reqnroll_report.html" }
    }
}
```

In the above example, the configuration file instructs Reqnroll to use both Reporting formatters. Each is given a location to store it's respective file in an absolute path (C:\\Users\\dev\\source\\repos\\reqnroll_project\\CucumberMessages\\reqnroll_report.ndjson). 

If the formatters object is omitted or empty, report generation is turned OFF.



{#environment-variable}
## Environment Variables

The settings discussed above can be overriden by setting an Environment Variable. When an environment variable is set, it takes precedence over the same configuration setting in the configuration file. If a setting is not overridden by an evironment variable, the value will be taken from the configuration file (if set), otherwise a default (as shown above) will be used.

The available Environment Variables are:

* REQNROLL_CUCUMBER_MESSAGES_ENABLED
* REQNROLL_CUCUMBER_MESSAGES_FORMATTERS

```{note}
When using an environment variable to override the Formatters section of the reqnroll.json configuration file, the value of the environment variable replaces the Formatters element in its entirety.
```
