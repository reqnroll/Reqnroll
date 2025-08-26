# Visual Studio 2022 Integration

The Visual Studio integration includes a number of features that make it easier to edit Gherkin files and navigate to and from step definitions in Visual Studio. You can also generate code snippets for step definition methods from feature files. The Visual Studio integration also allows you to execute tests from Visual Studio's Test Explorer.

You can install the integration from the Visual Studio Gallery (Marketplace) or directly in Visual Studio. Detailed instructions can be found [here](../../installation/setup-ide.md#setup-visual-studio-2022).

The integration provides the following features:

* [Editing Feature Files](visual-studio-integration-editing-features)
  * [Gherkin syntax highlighting](visual-studio-integration-editing-features.md#gherkin-syntax-highlighting) in feature files, highlighting unbound steps and parameters
  * [IntelliSense](visual-studio-integration-editing-features.md#intellisense-auto-completion-for-keywords-and-steps) (auto-completion) for keywords and steps
  * [Outlining](visual-studio-integration-editing-features.md#outlining-and-comments-in-feature-files) (folding) sections of the feature file
  * [Comment/uncomment](visual-studio-integration-editing-features.md#outlining-and-comments-in-feature-files) feature file lines
  * Automatic Gherkin [table formatting](visual-studio-integration-editing-features.md#table-formatting)
  * [Document formatting](visual-studio-integration-editing-features.md#document-formatting)
* [Navigation](visual-studio-integration-navigation-features)
  * Navigate from [steps in scenarios to step definitions and vice versa](visual-studio-integration-navigation-features.md#navigating-from-a-step-definition-method-to-steps-in-gherkin-files)
  * Navigate from a scenario to Hook methods (** GO TO HOOKS **)
  * Find Unused step definitions (those not yet bound to a step)
  * Detects step definitions within the Reqnroll project to enable navigation features and indication of defined status of a step
* Other
  * [Generate step definition methods](define-steps) from feature files
  * Configurable options

