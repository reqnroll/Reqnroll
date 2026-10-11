namespace Reqnroll.StepBindingSourceGenerator;

internal record MethodInfo(
    string Name,
    QualifiedTypeName DeclaringClassName,
    bool ReturnsVoid,
    bool ReturnsTask,
    bool IsAsync,
    ComparableArray<ParameterInfo> Parameters);
