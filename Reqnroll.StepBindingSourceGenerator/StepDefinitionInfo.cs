namespace Reqnroll.StepBindingSourceGenerator;

internal record StepDefinitionInfo(
    string DisplayName,
    MethodInfo Method,
    StepKeywordMatch MatchesKeywords,
    BindingMethod BindingMethod,
    string? TextPattern,
    MethodInvocationStyle InvocationStyle);
