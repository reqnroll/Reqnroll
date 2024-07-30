# Setup an IDE for Reqnroll

```{tip}
Reqnroll can be used without any IDE integration as well, so setting up the IDE is optional.
```

Setting up the Integrated Development Environment (IDE) integration for Reqnroll can add convenience and productivity features like:

* Adding new project elements, like feature files based on templates
* Syntax coloring of feature files
* Showing suggestions (completions) for Gherkin syntax keywords
* Navigating between steps and step definitions
* Adding step definition snippets to your codebase for undefined steps

This guide describes the setup steps for the following IDEs:

* [](#setup-vs)
* [](#setup-vscode)
* [](#setup-rider)

{#setup-vs}
## Setup Visual Studio 2022

In order to use Reqnroll with Visual Studio 2022, you need to install the [Reqnroll for Visual Studio 2022](https://go.reqnroll.net/vs2022-extension) extension.

```{warning}
The *Reqnroll with Visual Studio 2022* extension cannot work together with the *SpecFlow for Visual Studio 2022* extension, as they both process feature files. As the Reqnroll extension also supports SpecFlow projects, you can remove the SpecFlow extension if you install the Reqnroll extension. Alternatively, you can disable the SpecFlow extension for the time you work with Reqnroll. 
```

1. Open Visual Studio 2022
2. From the *Extensions* menu, choose the *Manage Extensions...* command.
3. On the dialog, make sure that *Online* is selected from the list on the left and type `Reqnroll` to the *Search* text box on the right top corner.
4. Choose the *Reqnroll for Visual Studio 2022* from the list and click on the *Download* button.
5. Restart Visual Studio 2022.

For more details about the Reqnroll with Visual Studio extension, please check the [](../ide-integrations/visual-studio/index) page.

```{hint}
The Reqnroll Visual Studio extension cannot be used for Visual Studio for Mac. On macOS we recommend using [Visual Studio Code](#setup-vscode).
```

{#setup-vscode}
## Setup Visual Studio Code

For using Reqnroll with Visual Studio Code, you can choose from multiple available extensions. We recommend using the [Cucumber](https://marketplace.visualstudio.com/items?itemName=CucumberOpen.cucumber-official) extension.

In order to use the navigation features of the extension, you should configure the location of your feature files and step definition classes within your repository.

The following Visual Studio configuration shows a typical configuration.

```{code-block} json
:caption: .vscode/settings.json
{
  "explorer.fileNesting.patterns": {  // shows *.feature.cs files as nested items
    "*.feature": "${capture}.feature.cs"
  },
  "files.exclude": { // excludes compilation result
    "**/obj/": true,
    "**/bin/": true,
  },
  "cucumber.glue": [ // sets the location of the step definition classes
    "MyReqnrollProject/**/*.cs",
  ],
  "cucumber.features": [ // sets the location of the feature files
    "MyReqnrollProject/**/*.feature",
  ]
}
```

{#setup-rider}
## Setup Rider

In order to use Reqnroll with Rider, you need to install the [Reqnroll for Rider](https://plugins.jetbrains.com/plugin/24012-reqnroll-for-rider) extension.

```{warning}
The *Reqnroll with Rider* extension cannot work together with the *SpecFlow for Rider* extension, as they both process feature files. As the Reqnroll extension also supports SpecFlow projects, you can remove the SpecFlow extension if you install the Reqnroll extension. Alternatively, you can disable the SpecFlow extension for the time you work with Reqnroll. 
```

1. Open Rider and verify that you are using a compatible version. The following versions are verified to work with Reqnroll:
    - `2024.1.4`
    - `2024.2 EAP9 build 242.20224.162`
2. Top right of Rider click the gear icon and press plugins.
3. Click `Marketplace`. 
3. Enter `Reqnroll` in the search box and install.
4. Enter `Gherkin` in the search box and install.
5. Open csproj and verify your project contains `<Contents include = "**/*.feature*"/>`
5. Restart Rider.



% TODO
```{admonition} Documentation is in progress
:class: warning

This documentation page is in progress. Please, come back later or help contribute to it in our [open-source GitHub project](https://github.com/reqnroll/Reqnroll).
```
