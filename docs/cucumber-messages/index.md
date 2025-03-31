# Reporting
Reqnroll uses reporter plugins to produce reports that contain information about what scenarios have passed or failed.

Some plugins are built-in, others have to be installed separately. You can also build your own.

This page documents Reqnroll's built-in formatter plugins.

Report generation in Reqnroll is 'opt-in'. By default, Reqnroll will not produce Messages.

The most basic way of turning ON Cucumber Message support is to set an Environment Variable.

```{code-block} pwsh
:caption: Powershell
$env:REQNROLL__CUCUMBER_MESSAGES__ENABLED=true
```
## Built-in Reporter Plugins

Reqnroll has two Reporting plugins built-in:
- messages
- html

### Messages

Reqnroll produces test reports as Cucumber Messages. Cucumber Messages are a standardized way for Cucumber-based test tooling (like Reqnroll) to feed test results to Reporting systems.

```{note}
For more information about Cucumber Messages and the tooling that consumes them, please see the [Cucumber Messages](https://github.com/cucumber/messages) page on Github.
```

The default name for this file is: reqnroll_report.ndjson. By default the output file is located in the execution directory of your test project, e.g., typically 
 `Your-Project/bin/Release/net8.0/` 
The file name and storage location can be changed via configuration options (see [Configuration(./configuration.html)]).

### HTML

This formatter plugin generates stand-alone HTML files from test results.
The default name for this file is: reqnroll_report.html. By default the output file is located in the execution directory of your test project, e.g., typically 
 `Your-Project/bin/Release/net8.0/` 
The file name and storage location can be changed via configuration options (see [Configuration(./configuration.html)]).