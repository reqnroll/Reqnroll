namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record SyntaxSlotPropertyInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    SyntaxKindInfo SyntaxKind,
    string? Description,
    bool IsRequired,
    ComparableArray<string> ParameterGroups)
{
    public bool IsInternalNodeNullable => NodeType == SyntaxNodeType.SyntaxNode || NodeType == SyntaxNodeType.SyntaxTokenList;
}
