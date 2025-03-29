namespace Reqnroll.Bindings;

/// <summary>
/// Specifies the language of the syntax that is used by a step text pattern to define its parameters.
/// </summary>
public enum StepTextPatternSyntaxLanguage
{
    /// <summary>
    /// The step text pattern does not employ a syntax.
    /// </summary>
    None,

    /// <summary>
    /// The pattern is a Cucumber expression.
    /// </summary>
    CucumberExpression,

    /// <summary>
    /// The pattern is a regular expression.
    /// </summary>
    RegularExpression
};
