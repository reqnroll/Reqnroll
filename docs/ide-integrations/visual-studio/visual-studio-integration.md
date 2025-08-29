# Feature Overview

The Visual Studio integration includes a number of features that make it easier to edit Gherkin files and navigate to and from step definitions in Visual Studio. You can also generate code snippets for step definition methods from feature files.

You can install the integration from the Visual Studio Gallery (Marketplace) or directly in Visual Studio. Detailed instructions can be found [here](../../installation/setup-ide.md#setup-visual-studio-2022).

The integration provides the following features:

* [Editing Feature Files](visual-studio-integration-editing-features)
  * [Gherkin syntax highlighting](visual-studio-integration-editing-features.md#gherkin-syntax-highlighting) in feature files, highlighting unbound steps and parameters
  * [IntelliSense](visual-studio-integration-editing-features.md#intellisense-auto-completion-for-keywords-and-steps) (auto-completion) for keywords and steps
  * [Outlining](visual-studio-integration-editing-features.md#outlining-and-comments-in-feature-files) (folding) sections of the feature file
  * [Comment/uncomment](visual-studio-integration-editing-features.md#outlining-and-comments-in-feature-files) feature file lines
  * Automatic Gherkin [table formatting](visual-studio-integration-editing-features.md#table-formatting)
  * [Document formatting](visual-studio-integration-editing-features.md#document-formatting)
  * [Renaming steps](visual-studio-integration-editing-features.md#renaming-steps)
* [Navigation](visual-studio-integration-navigation-features)
  * Navigate from [steps in scenarios to step definitions and vice versa](visual-studio-integration-navigation-features.md#navigating-from-a-scenario-step-to-a-step-definition)
  * Navigate from a [scenario to hook methods](visual-studio-integration-navigation-features.md#navigating-from-a-scenario-to-a-hook)
  * Find Unused step definitions (those not yet bound to a step)
  * Detects step definitions within the Reqnroll project to enable navigation features and indication of defined status of a step
* Other
  * [Generate code snippets for step definition methods](define-steps) from feature files
  * [Configurable settings](settings-options.md)
  * [Configurable Gherkin formatting settings with EditorConfig](editorconfig.md)
