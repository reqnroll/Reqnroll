# Feature Language

To avoid communication errors introduced by translations, it is recommended to keep the specification and the acceptance test descriptions in the language of the business. The Gherkin format supports many natural languages besides English, like German, Spanish or French. More details on the supported natural languages are available in the [Cucumber documentation](https://cucumber.io/docs/gherkin/languages/).  

The language of the feature files can either be specified globally in your configuration (see [Set the default feature file language](../installation/configuration.md#set-the-default-feature-file-language), or in the feature file's header using the `#language` syntax. Specify the language using the ISO language names used by the `CultureInfo` class of the .NET Framework (e.g. `en-US`).  

```{code-block} gherkin
:caption: Feature File
#language: de-DE
Funktionalität: Addition
...
```

Reqnroll uses the feature file language to determine the set of keywords used to parse the file, but the language setting is also used as the default setting for converting parameters by the Reqnroll runtime. The culture for binding execution and parameter conversion can be specified explicitly, see [language element](../installation/configuration.md#language).

As data conversion can only be done using a specific culture in the .NET Framework, we recommend using the specific culture name (e.g. `en-US`) instead of the neutral culture name (e.g. `en`). If a neutral culture is used, Reqnroll uses a specific default culture to convert data (e.g. `en-US` is used to convert data if the `en` language was used).
