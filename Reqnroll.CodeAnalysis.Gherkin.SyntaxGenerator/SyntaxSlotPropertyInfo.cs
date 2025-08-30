namespace Reqnroll.CodeAnalysis.Gherkin.SyntaxGenerator;

internal record SyntaxSlotPropertyInfo(
    SyntaxNodeType NodeType,
    string Name,
    int Index,
    string TypeName,
    ComparableArray<SyntaxKindInfo> SyntaxKinds,
    string? Description,
    bool IsRequired,
    bool IsInherited,
    bool IsAbstract)
{
    public bool IsInternalNodeNullable => NodeType != SyntaxNodeType.SyntaxToken;
}
