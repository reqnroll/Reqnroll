# Bindings from External Assemblies

[Bindings](Bindings.md) can be defined in the main Reqnroll project or in other assemblies (_external binding assemblies_). If the bindings are used from external binding assemblies, the following notes have to be considered:

- The external binding assembly can be another project in the solution or a compiled library (dll).
- The external binding assembly can also use a different .NET language, e.g. you can write bindings for your C# Reqnroll project also in F# (As an extreme case, you can use your Reqnroll project with the feature files only and with all the bindings defined in external binding assemblies).
- The external binding assembly has to be referenced from the Reqnroll project to ensure it is copied to the target folder and listed in the `reqnroll.json` or `app.config` of the Reqnroll project (see below).
- The external binding assemblies can contain all kind of bindings: [step definition](Step-Definitions.md), [hooks](Hooks.md) and also [step argument transformations](Step-Argument-Conversions.md).
- The bindings from assembly references are not fully supported in the Visual Studio integration of Reqnroll v1.8 or earlier: the step definitions from these assemblies will not be listed in the autocompletion lists.
- The external binding file must be in the root of the project being referenced. If it is in a folder in the project, the bindings will not be found.

## Configuration

In order to use bindings from an external binding assembly, you have to list it (with the assembly name) in the `reqnroll.json` or `app.config` of the Reqnroll project. The Reqnroll project is always included implicitly. See more details on the configuration in the `<stepAssemblies>` section of [the configuration guide](../Installation/Configuration.md).

**reqnroll.json example:**

```json
{
  "stepAssemblies": [
    {
      "assembly": "MySharedBindings"
    }
  ]
}
```

**app.config example:**

```xml
<reqnroll>
  <stepAssemblies>
    <stepAssembly assembly="MySharedBindings" />
  </stepAssemblies>
</reqnroll>
```
