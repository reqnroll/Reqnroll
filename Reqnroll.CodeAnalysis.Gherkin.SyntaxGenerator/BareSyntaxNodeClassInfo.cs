namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record BareSyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    ushort SyntaxKind,
    ComparableArray<BareSyntaxSlotPropertyInfo> SlotProperties);
