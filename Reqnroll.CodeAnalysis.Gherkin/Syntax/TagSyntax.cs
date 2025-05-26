namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a tag in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Tag)]
public partial class TagSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.AtToken, "The token that represents the \"@\" symbol prefix of the tag.")]
    public SyntaxToken AtSymbolToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The token that represents the name of the tag.")]
    public SyntaxToken Text { get; }
}
