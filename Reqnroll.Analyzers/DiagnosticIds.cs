namespace Reqnroll.Analyzers;

internal static class DiagnosticIds
{
    internal const string Prefix = "Reqnroll";

    // Step Definition Expressions
    internal const string StepDefinitionExpressionCannotBeNull = Prefix + "1001";
    internal const string StepDefinitionExpressionCannotBeEmptyOrWhitespace = Prefix + "1002";
    internal const string StepDefinitionExpressionShouldNotHaveLeadingOrTrailingWhitespace = Prefix + "1003";

    // Step Definition Method Signatures
    internal const string StepDefinitionMethodShouldReturnVoidOrTaskOrValueTask = Prefix + "1021";
    internal const string AsyncStepDefinitionMethodMustReturnTask = Prefix + "1022";
}
