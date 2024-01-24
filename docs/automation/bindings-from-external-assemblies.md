# Bindings from External Assemblies

[Bindings](bindings) can be defined in the main Reqnroll project or in other assemblies (_external binding assemblies_). If the bindings are used from external binding assemblies, the following notes have to be considered:

- The external binding assembly can be another project in the solution or a compiled library (dll).
- The external binding assembly can also use a different .NET language, e.g. you can write bindings for your C# Reqnroll project also in F# (As an extreme case, you can use your Reqnroll project with the feature files only and with all the bindings defined in external binding assemblies).
- The external binding assembly has to be referenced from the Reqnroll project to ensure it is copied to the target folder and listed in the `reqnroll.json` of the Reqnroll project (see below).
- The external binding assemblies can contain all kind of bindings: [step definition](step-definitions), [hooks](hooks) and also [step argument transformations](step-argument-conversions).

## Configuration

In order to use bindings from an external binding assembly, you have to list it (with the assembly name) in the `reqnroll.json` (the Reqnroll project is always included implicitly). See [Use bindings from external projects](../installation/configuration.md#use-bindings-from-external-projects) section of the documentation for details. 

The following example registers the project `SharedStepDefinitions` as an external binding assembly.

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "bindingAssemblies": [
    { 
      "assembly": "SharedStepDefinitions"
    }
  ]
}
```
