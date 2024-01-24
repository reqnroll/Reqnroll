# Bindings

The [Gherkin feature files](../gherkin/feature-files) are closer to free-text than to code – they cannot be executed as they are. The automation that connects the specification to the application interface has to be developed first. The automation that connects the Gherkin specifications to source code is called a *binding*. The binding classes and methods can be defined in the Reqnroll project or in [external binding assemblies](bindings-from-external-assemblies).

```{note}
Bindings (step definitions, hooks, step argument transformations) are global for the entire Reqnroll project.
```

There are several kinds of bindings in Reqnroll. 

## Step Definitions

This is the most important one. The [step definition](step-definitions) that automates the scenario at the step level. This means that instead of providing automation for the entire scenario, it has to be done for each separate step. The benefit of this model is that the step definitions can be reused in other scenarios, making it possible to (partly) construct further scenarios from existing steps with less (or no) automation effort.  

It is required to add the `[Binding]` attribute to the classes where you define your step definitions.

See more details about step definitions in the [](step-definitions) page.

## Hooks

[Hooks](hooks) can be used to perform additional automation logic on specific events, e.g. before executing a scenario. See more details about hooks in the [](hooks) page.

## Step Argument Transformations

[Step Argument Transformations](step-argument-conversions) can be used to extend the step definition parameter conversion system of Reqnroll. See more details about step argument conversions in the [](step-argument-conversions) page.