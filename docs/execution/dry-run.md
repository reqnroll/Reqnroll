# Dry Run

```{note}
Introduced in Reqnroll v3
```

The Runtime of Reqnroll supports running tests in a "dry run" mode. This means that when each test is executed, the runtime will skip executing the code in your step handlers.

This can be useful for Pull Request/CI scenarios where you want to ensure all steps declared in the feature files match to a step handler in your C# code, but executing the test suite as normal is lengthy, expensive, or not possible. This feature is usually paired with the [runtime configuration](../installation/configuration.md#runtime) option `"missingOrPendingStepsOutcome": "Error"` to ensure any unbound steps are reported as errors.

## Enabling Dry Run
To enable dry run mode, set the environment variable `REQNROLL_DRY_RUN=true` when executing your tests.

```{note}
It's usually not enough to simply set the environment variable in your session before invoking `dotnet test`. The .NET test runtime creates its own environment which may not inherit variables from the parent process. (May be different depending on the shell, test framework, OS, etc.)

To ensure the environment variable is set correctly, use one of the options shown below.
```

### Example with .NET CLI
```{code-block} pwsh
:caption: .NET CLI
dotnet test -e "REQNROLL_DRY_RUN=true" MyReqnrollProject.csproj
```

### Example with `.runsettings` file
```{code-block} xml
:caption: MyRunSettings.runsettings
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <REQNROLL_DRY_RUN>true</REQNROLL_DRY_RUN>
    </EnvironmentVariables>
  </RunConfiguration>
</RunSettings>
```

```{code-block} pwsh
:caption: Consuming the .runsettings file
dotnet test --settings MyRunSettings.runsettings
```
