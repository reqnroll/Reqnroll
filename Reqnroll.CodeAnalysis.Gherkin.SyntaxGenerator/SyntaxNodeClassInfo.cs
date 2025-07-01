namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record SyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    SyntaxKindInfo SyntaxKind,
    string BaseClassName,
    ComparableArray<SyntaxSlotPropertyInfo> SlotProperties);
