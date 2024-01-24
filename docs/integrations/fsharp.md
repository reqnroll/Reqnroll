# F# Support

[Bindings](../automation/bindings) for Reqnroll can be written also in F#. Doing so you can take the advantages of the F# language for writing step definitions: you can define regex-named F# functions for your steps. Simply put the regex between double backticks.

```F#
let [<Given>] ``I have entered (.*) into the calculator``(number:int) = 
    Calculator.Push(number)
```

Although the regex method names are only important for [step definitions](../automation/step-definitions) you can also define [hooks](../automation/hooks) and [step argument conversions](../automation/step-argument-conversions) in the F# binding projects.

Note: You need to create a C# or VB project for hosting the feature files and configure your F# project(s) as [external binding assemblies](../automation/bindings-from-external-assemblies):

```{code-block} json
:caption: reqnroll.json
{
  "$schema": "https://schemas.reqnroll.net/reqnroll-config-latest.json",

  "bindingAssemblies": [
    { 
      "assembly": "MyFSharpBindings"
    }
  ]
}
```
