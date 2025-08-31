namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a tag in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Tag)]
public sealed partial class TagSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.AtToken, "The token that represents the \"@\" symbol prefix of the tag.")]
    public partial SyntaxToken AtSymbolToken { get; }

    [SyntaxSlot(SyntaxKind.NameToken, "The token that represents the name of the tag.")]
    public partial SyntaxToken Text { get; }
}
