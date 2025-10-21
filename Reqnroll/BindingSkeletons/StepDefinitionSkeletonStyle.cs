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

        // Async variants

        [Description("Regular expressions in attributes, Async binding")]
        AsyncRegexAttribute = 128,
        // legacy config
        [Description("Method name - underscores, Async binding")]
        AsyncMethodNameUnderscores = 129,
        // legacy config
        [Description("Method name - pascal case, Async binding")]
        AsyncMethodNamePascalCase = 130,
        // legacy config
        [Description("Method name as regulare expression (F#), Async binding")]
        AsyncMethodNameRegex = 131,
        [Description("Cucumber expressions in attributes, Async binding")]
        AsyncCucumberExpressionAttribute = 132,
    }
}