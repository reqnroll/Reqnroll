# Reqnroll Bindings Provider

This folder contains services for the [Visual Studio extension](https://github.com/reqnroll/Reqnroll.VisualStudio/) in order to query the binding information from the Reqnroll project.

The Visual Studio extension invokes the `BindingProviderService.DiscoverBindings` method to retrieve the binding information as a JSON string. We use a string interface to minimize the binary compatibility errors across the different Reqnroll versions.

*Note:* Do not modify or remove these classes unless you are sure that the changes will not affect the Reqnroll Visual Studio extension. The extension relies on these classes to function correctly.
