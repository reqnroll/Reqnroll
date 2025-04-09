namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Feature)]
public partial class FeatureSyntax : SyntaxNode
{
    /// <summary>
    /// Gets the token that represents the <c>Feature:</c> keyword.
    /// </summary>
    [SyntaxSlot(SyntaxKind.FeatureKeyword)]
    public partial SyntaxToken FeatureKeyword { get; }

    /// <summary>
    /// Gets the token that represents the colon following the keyword.
    /// </summary>
    [SyntaxSlot(SyntaxKind.ColonToken)]
    public partial SyntaxToken ColonToken { get; }

    /// <summary>
    /// Gets the name of the feature.
    /// </summary>
    [SyntaxSlot(SyntaxKind.IdentifierToken)]
    public partial SyntaxToken Name { get; }

    /// <summary>
    /// Gets the description of the feature.
    /// </summary>
    [SyntaxSlot(SyntaxKind.Description)]
    public partial DescriptionSyntax? Description { get; }
}
