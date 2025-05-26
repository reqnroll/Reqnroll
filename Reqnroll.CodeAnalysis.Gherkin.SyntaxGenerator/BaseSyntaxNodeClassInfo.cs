namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record BaseSyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    string BaseClassName,
    ComparableArray<SyntaxSlotPropertyInfo> SlotProperties);
