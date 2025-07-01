namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record BareSyntaxNodeClassInfo(
    string ClassNamespace,
    string ClassName,
    ushort SyntaxKind,
    string BaseClassName,
    ComparableArray<BareSyntaxSlotPropertyInfo> SlotProperties,
    bool IsAbstract);
