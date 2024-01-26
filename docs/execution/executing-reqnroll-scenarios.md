# Executing Reqnroll Scenarios

Reqnroll generates executable tests from the scenarios defined in [feature files](../gherkin/feature-files.md). In order to execute these tests you can use your usual test execution tools.

{#executing-scenarios-from-console}
## Executing scenarios from console

From the console, you can use the `dotnet test` command.

1. Open a console
2. Change the current directory to the folder of your Reqnroll project (where the `.csproj` file is located)
3. Invoke `dotnet test`

```{code-block} pwsh
:caption: Terminal
> dotnet test
  Determining projects to restore...
  All projects are up-to-date for restore.
[...]
Starting test execution, please wait...
[...]
Passed! - Failed: 0, Passed: 1, Skipped: 0, Total: 1, Duration: 76 ms - MyReqnrollProject.dll
```

```{note}

Running the `dotnet test` command automatically restores the dependencies and builds your project by default.
```

On Windows the test execution is also possible using the [`vstest.console.exe`](https://learn.microsoft.com/en-us/visualstudio/test/vstest-console-options?view=vs-2022) tool. For that, make sure you use a [Developer Command Prompt](https://learn.microsoft.com/en-us/visualstudio/ide/reference/command-prompt-powershell?view=vs-2022).

```{code-block} pwsh
:caption: Developer Command Prompt
> vstest.console.exe .\bin\Debug\net8.0\MyReqnrollProject.dll
```

{#executing-scenarios-from-visual-studio}
## Executing scenarios from Visual Studio

Visual Studio contains a built-in test execution feature that can also be used for executing Reqnroll scenarios as well. In addition to that, other test execution tools, like [ReSharper](https://www.jetbrains.com/resharper/) or [NCrunch](https://www.ncrunch.net/) can also be used.

1. From the *Test* menu, choose the *Test Explorer* command. The *Test Explorer* tool window will open.
2. Wait until the tests are listed in the *Test Explorer* window. You might need to build your project first.
3. Locate the scenario you would like execute and invoke *Run* from the context menu. You can also use the *Run All Tests In View* button from the *Test Explorer* toolbar. 

```{note}

Running the tests from the *Test Explorer* window will automatically save your files and build your project before executing the tests.
```
