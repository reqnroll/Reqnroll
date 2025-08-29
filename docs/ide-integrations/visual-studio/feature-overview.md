# Feature Overview

The Visual Studio extension includes a number of features that make it easier to edit Gherkin files and navigate to and from step definitions in Visual Studio. You can also generate code snippets for step definition methods from feature files.

You can install the extension from the Visual Studio Gallery (Marketplace) or directly in Visual Studio. Detailed instructions can be found [here](../../installation/setup-ide.md#setup-visual-studio-2022).

The extension provides the following features:

* [Editing Feature Files](editing-features)
  * [Gherkin syntax highlighting](editing-features.md#gherkin-syntax-highlighting) in feature files, highlighting unbound steps and parameters
  * [IntelliSense](editing-features.md#intellisense-auto-completion-for-keywords-and-steps) (auto-completion) for keywords and steps
  * [Outlining](editing-features.md#outlining-and-comments-in-feature-files) (folding) sections of the feature file
  * [Comment/uncomment](editing-features.md#outlining-and-comments-in-feature-files) feature file lines
  * Automatic Gherkin [table formatting](editing-features.md#table-formatting)
  * [Document formatting](editing-features.md#document-formatting)
  * [Renaming steps](editing-features.md#renaming-steps)
* [Navigation](navigation-features)
  * Navigate from [steps in scenarios to step definitions and vice versa](navigation-features.md#navigating-from-a-scenario-step-to-a-step-definition)
  * Navigate from a [scenario to hook methods](navigation-features.md#navigating-from-a-scenario-to-a-hook)
  * Find Unused step definitions (those not yet bound to a step)
  * Detects step definitions within the Reqnroll project to enable navigation features and indication of defined status of a step
* Other
  * [Generate code snippets for step definition methods](defining-steps) from feature files
  * [Configurable settings](settings.md)
  * [Configurable Gherkin formatting settings with EditorConfig](editorconfig.md)
