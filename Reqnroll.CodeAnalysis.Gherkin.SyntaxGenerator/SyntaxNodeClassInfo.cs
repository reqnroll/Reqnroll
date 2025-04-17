namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record SyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    SyntaxKindInfo SyntaxKind,
    ComparableArray<SyntaxSlotPropertyInfo> SlotProperties);
