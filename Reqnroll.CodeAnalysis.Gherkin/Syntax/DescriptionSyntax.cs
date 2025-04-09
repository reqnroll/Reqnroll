namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a block of descriptive text.
/// </summary>
[SyntaxNode(SyntaxKind.Description)]
public partial class DescriptionSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.List)]
    public partial SyntaxTokenList TextTokens { get; }
}
