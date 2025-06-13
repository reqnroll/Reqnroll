namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a name of an entity in a Gherkin document, such as a feature or scenario.
/// </summary>
[SyntaxNode(SyntaxKind.Name)]
public partial class NameSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.IdentifierToken, "The token that represents the identifier.")]
    public SyntaxToken Identifier { get; }
}
