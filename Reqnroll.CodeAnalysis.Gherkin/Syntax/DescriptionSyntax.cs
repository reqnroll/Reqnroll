namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a block of descriptive text.
/// </summary>
[SyntaxNode(SyntaxKind.Description)]
public partial class DescriptionSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.List, "The text tokens which form the descriptive text.")]
    public partial SyntaxTokenList Text { get; }
}
