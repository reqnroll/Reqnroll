namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record BareSyntaxSlotPropertyInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    ushort SyntaxKind,
    string? Description,
    bool IsOptional,
    ComparableArray<string> ParameterGroups);
