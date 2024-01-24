using System.ComponentModel;

namespace Reqnroll.BindingSkeletons
{
    public enum StepDefinitionSkeletonStyle
    {
        [Description("Regular expressions in attributes")]
        RegexAttribute = 0,
        // legacy config
        [Description("Method name - underscores")]
        MethodNameUnderscores = 1,
        // legacy config
        [Description("Method name - pascal case")]
        MethodNamePascalCase = 2,
        // legacy config
        [Description("Method name as regulare expression (F#)")]
        MethodNameRegex = 3,
        [Description("Cucumber expressions in attributes")]
        CucumberExpressionAttribute = 4,
    }
}