namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Description)]
public partial class DescriptionSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.DescriptionLiteralToken, "The text of the description.")]
    public partial SyntaxTokenList Text { get; }
}
