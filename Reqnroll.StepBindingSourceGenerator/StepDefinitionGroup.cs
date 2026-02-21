namespace Reqnroll.StepBindingSourceGenerator;

internal record StepDefinitionGroup(
    QualifiedTypeName DeclaringClassName,
    ComparableArray<StepDefinitionInfo> StepDefinitions);
