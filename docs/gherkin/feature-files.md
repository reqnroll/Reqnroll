# Feature Files

The feature files are the files that contain the BDD executable specification. 

The feature files are plain text files with the `.feature` extension. You can put feature files in any folders within the Reqnroll project, but the convention is to have a `Features` folder in your project and put the feature files in that folder, optionally in sub-folders.
```{admonition} Feature Files Should Be Saved in UTF-8
:class: warning

For proper support of non-ASCII characters in feature files (such as currency symbols and accented characters, feature files must be encoded in UTF-8 (with or without BOM signature). You may wish to add a [*.feature] section to your .editorconfig file with a charset setting to enforce this.
```

The format of the feature files is called *Gherkin* that is specified and maintained by the [Cucumber](https://cucumber.io/) project. For a full language reference please check the [Cucumber documentation](https://cucumber.io/docs/gherkin/).

The following example shows a feature file that describes the addition functionality of a calculator.

```{code-block} gherkin
:caption: Calculator.feature
Feature: Calculator

Simple calculator for adding two numbers

Rule: Add should calculate the sum of the entered numbers

@mytag
Scenario: Add two numbers
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120
```

Please also check the [](gherkin-reference) section of the Reqnroll documentation for the details of the feature file syntax.
