# Cucumber Messages Introduction
Reqnroll can produce test results as Cucumber Messages. Cucumber Messages are a standardized way for Cucumber-based test tooling (like Reqnroll) to feed test results to Reporting systems.

```{note}
For more information about Cucumber Messages and the tooling that consumes them, please see the [Cucumber Messages](https://github.com/cucumber/messages) page on Github.
```

Cucumber Message support in Reqnroll is 'opt-in'. By default, Reqnroll will not produce Messages.

The most basic way of turning ON Cucumber Message support is to set an Environment Variable.

```{code-block} pwsh
:caption: Powershell
$env:REQNROLL__CUCUMBER_MESSAGES__ENABLED=true
```

Once enabled, test output will be stored as an .ndjson file.

The default name for this file is: reqnroll_report.ndjson. By default the output file is located in the execution directory of your test project, e.g., typically 
 `Your-Project/bin/Release/net8.0/` 
The file name and storage location can be changed via configuration options (see [Configuration(./configuration.html)]).
