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
* - OutputFilePath
  - Folder Path and File Name for storage of Cucumber Messages results files. <br/> *Default:* `` (none)
  - By convention, these files use the `.ndjson` extension. <br/> *Default:* `reqnroll_report.ndjson`
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
    "cucumberMessagesConfiguration": {
        "enabled" :  true,
        "outputFilePath": "C:\\Users\\dev\\source\\repos\\reqnroll_project\\CucumberMessages\\reqnroll_report.ndjson"
    }
}
```

In the above example, the configuration file instructs Reqnroll to turn Cucumber Message support ON, store the file in an absolute path (C:\\Users\\dev\\source\\repos\\reqnroll_project\\CucumberMessages\\reqnroll_report.ndjson). 


The json schema for this section of the configuration file is:
```{code-block} json
:caption: cucumbermessages.config-schema.json
{
    "description": "This class holds configuration information from a configuration source for Reqnroll Cucumber Message Generation.\n",
    "properties": {
        "Enabled": {
            "type": "boolean"
        },
        "OutputFilePath": {
            "type": "string"
        }
    }
}
```

{#environment-variable}
## Environment Variables

Any of the settings discussed above can be overriden by setting an Environment Variable. When an environment variable is set, it takes precedence over the same configuration setting in the configuration file. If a setting is not overridden by an evironment variable, the value will be taken from the configuration file (if set), otherwise a default (as shown above) will be used.

The available Environment Variables are:

* REQNROLL__CUCUMBER_MESSAGES__ENABLED
* REQNROLL__CUCUMBER_MESSAGES__OUTPUT_FILEPATH

