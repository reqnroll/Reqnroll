namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Feature)]
public partial class FeatureSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.FeatureKeyword, "The token that represents the \"Feature\" keyword.")]
    public partial SyntaxToken FeatureKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The name of the feature.")]
    public partial SyntaxToken Name { get; }

    [SyntaxSlot(SyntaxKind.Description, "The optional description of the feature.")]
    public partial DescriptionSyntax? Description { get; }
}
