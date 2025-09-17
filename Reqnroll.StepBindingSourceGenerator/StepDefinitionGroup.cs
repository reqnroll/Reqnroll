namespace Reqnroll.StepBindingSourceGenerator;

internal record StepDefinitionGroup(
    string GroupName,
    ComparableArray<StepDefinitionInfo> StepDefinitions);
