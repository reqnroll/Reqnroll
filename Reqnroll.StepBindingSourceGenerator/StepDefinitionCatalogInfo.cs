namespace Reqnroll.StepBindingSourceGenerator;

internal record StepDefinitionCatalogInfo(
    string HintName,
    QualifiedTypeName ClassName,
    ComparableArray<StepDefinitionInfo> StepDefinitions);
