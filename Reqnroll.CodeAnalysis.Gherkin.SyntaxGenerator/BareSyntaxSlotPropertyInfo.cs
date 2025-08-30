namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record BareSyntaxSlotPropertyInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    ComparableArray<ushort> SyntaxKinds,
    string? Description,
    bool IsOptional,
    bool IsInherited,
    bool IsAbstract);
