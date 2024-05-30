# How to configure Cucumber Expression behavior for large legacy projects

Reqnroll uses Cucumber Expressions as the default method to connect step definitions to steps, while regular expressions are still supported. Cucumber expressions are provide a nice and convenient way to define which step should match to a step definition, but if you migrated a large project that uses regular expressions extensively, you might run into some issues.

The main problem is that Reqnroll needs to decide based on an expression whether it is a regular expression or a cucumber expression. As SpecFlow introduced regular expression support without forcing the users to use the regex start (`^`) and end (`$`) markers, this is not an easy task to do. Reqnroll uses some heuristics to make the decision:

* If the expression has the regex start (`^`) and end (`$`) markers, it is treated as regex
* If the expression contains a cucumber expression parameter placeholder (e.g. `{string}`), it is treated as cucumber expression
* If the expression contains common regex patterns (`.*`, `\.`, `\d+`) it is treated as regex
* In all other cases it is treated as cucumber expression

These heuristics work for the most of the regular expressions used in legacy projects, but in a large project there can be a big number of mis-classification that can only be detected during test execution time.

To avoid that, our primary recommendation is to add the regex start (`^`) and end (`$`) markers for all of the existing step definitions. This can be done in Visual Studio with a solution-level search-and-replace: 

* Invoke Visual Studio "Find and Replace in Files" (Ctrl+Alt+H) option
* Set search text to `\[(Given|When|Then)\((@?)"(.*?)"\)\]`
* Set replacement text to `[$1($2"^$3$$")]`
* Check "Use regular expressions" setting
* Click on "Replace All"

If for some reason this is not possible, you can also modify locally the heuristics that are used to decide whether an expression is a regular expression or a cucumber expression. Maybe there is a common regex pattern in your legacy project that you would like to handle or in an extreme case, you could override the logic (temporarily) to treat all expressions as regex.

```{admonition} Overriding Cucumber Expression detection strategy might be incompatible with IDE support
:class: warning

Changing the cucumber expression detection strategy might cause incompatibilities with different IDEs, resulting that the IDE will still mis-classify your expression and report a warning or an undefined step, although during test execution the scenarios can be executed without problems. (Visual Studio currently handles strategy overrides, but there might be features in the future that does not work correctly with overridden strategies.)

Because of that we recommend to apply this only temporarily or if the other solution (add the regex start and end markers) cannot be applied.
```

In order to override the detection strategy, you need to implement a simple Reqnroll runtime plugin. In the simplest case this is just adding a C# file to your Reqnroll project.

The plugin should look like the following (this is the file you need to add to your project):

```{code-block} csharp
:caption: ForceRegexPlugin.cs
using System.Text.RegularExpressions;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using MyProject;

[assembly:RuntimePlugin(typeof(ForceRegexPlugin))]

namespace MyProject;

public class ForceRegexPlugin : IRuntimePlugin
{
    public class ForceRegexDetector : ICucumberExpressionDetector
    {
        public bool IsCucumberExpression(string cucumberExpressionCandidate)
        {
            // TODO: If cucumberExpressionCandidate contains a specific regex pattern you use, 
            // treat it as regex
            if (Regex.IsMatch(cucumberExpressionCandidate, @"some-pattern"))
                return false;
            // Otherwise fall back to the default logic 
            // (you can also derive from CucumberExpressionDetector and use 'base')
            return new CucumberExpressionDetector().IsCucumberExpression(cucumberExpressionCandidate);
            // In order to force all expressions to be regex, just return false.
        }
    }

    public void Initialize(
        RuntimePluginEvents runtimePluginEvents, 
        RuntimePluginParameters runtimePluginParameters, 
        UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += (_, args) =>
        {
            // register our class as ICucumberExpressionDetector
            args.ObjectContainer.RegisterTypeAs<ForceRegexDetector, ICucumberExpressionDetector>();
        };
    }
}
```

You need to complete the logic marked with `TODO` and also correct the namespace `MyProject` when you apply this code.
