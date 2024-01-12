# Upgrade from SpecFlow 2.* to 3.*

This guide explains how to update your SpecFlow 2.* project to the latest SpecFlow 3.* version

## Make a Backup!

Before upgrading to the latest version, ensure you have a backup of your project (either locally or in a source control system).

## Visual Studio Integration

The Visual Studio integration for Reqnroll has been updated for SpecFlow 3. You will need to update the extension in order to upgrade. If you previously set the extension to not update automatically, please enable automatic upgrades once your projects have been migrated to SpecFlow 2.3.2 or higher.

## App.config Deprecated

### Changes to How Unit Test Providers are Configured

In previous versions of Reqnroll, the unit test provider used to execute tests was configured in your app.config file. As of SpecFlow 3, you need to configuring your unit test provider by installing one of the available packages (see below).

### reqnroll.json

Moving forward, we recommend using reqnroll.json to configure Reqnroll, rather than app.config. .NET Core projects require reqnroll.json (app.config is not supported). While using reqnroll.json is optional for Full Framework projects, we recommend migrating to the new format. For more details, see [Configuration](../Installation/Configuration.md) in the documentation.

## Updating Reqnroll

To upgrade a solution from SpecFlow 2.x to SpecFlow 3:

1. Open your solution, and check that it compiles, all tests are discovered and that all source files have been committed.
1. Right-click on your solution and select Manage NuGet Packages for Solution.
1. Switch to Updates in the list on the left and locate Reqnroll in the list of packages. Use the search box to restrict the listed packages if necessary.
![NuGet Dialog in Visual Studio](../_static/images/update-reqnroll2-nuget-packages.png)
1. Select the Reqnroll package in the list and click on Update.
1. Add one of the following packages to your specifications project (the one containing your tests) to select your unit test provider. You will receive an error if you add more than one of these packages to your project:

    - SpecRun.Reqnroll
    - Reqnroll.xUnit
    - Reqnroll.MsTest
    - Reqnroll.NUnit  

1. Remove “ReqnrollSingleFileGenerator” from the Custom Tool field in the Properties of your feature files.

## Updating SpecFlow+ Runner

If you want to update both Reqnroll and SpecFlow+ Runner to version 3, the easiest way to do this is to simply upgrade the SpecRun package. This automatically updates Reqnroll as well.

To update Reqnroll and SpecFlow+ Runner:

1. Open your solution, and check that it compiles, all tests are discovered and that all source files have been committed.
1. Right-click on your solution and select Manage NuGet Packages for Solution.
1. Uninstall any SpecRun.Reqnroll.*-*-* packages you have installed.
1. Install/update the following packages:
    - Reqnroll
    - SpecRun.Reqnroll
1. Remove “ReqnrollSingleFileGenerator” from the Custom Tool field in the Properties of your feature files.

### SpecFlow+ Runner Report Templates

If you have customized the SpecFlow+ runner templates, a small change needs to be made to the template for SpecFlow 3:

Open the CSHTML file in the editor of your choice.
Replace the first line with the following:

``` xml
@inherits Reqnroll.Plus.Runner.Reporting.CustomTemplateBase<TestRunResult>
```

