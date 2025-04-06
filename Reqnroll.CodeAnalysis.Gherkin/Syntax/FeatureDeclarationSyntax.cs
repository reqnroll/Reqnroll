namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
public class FeatureDeclarationSyntax : DeclarationSyntax
{
    internal FeatureDeclarationSyntax(Internal.FeatureDeclarationSyntax node) : base(node)
    {
    }

    internal FeatureDeclarationSyntax(Internal.FeatureDeclarationSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    private new Internal.FeatureDeclarationSyntax InternalNode => (Internal.FeatureDeclarationSyntax)base.InternalNode;

    public override SyntaxTokenList Tags => new();

    /// <summary>
    /// Gets the token that represents the <c>Feature:</c> keyword.
    /// </summary>
    public SyntaxToken FeatureKeyword => new(this, InternalNode.keyword, Position + InternalNode.GetSlotOffset(0));

    /// <summary>
    /// Gets the token that represents the colon following the keyword.
    /// </summary>
    public SyntaxToken ColonToken => new(this, InternalNode.colon, Position + InternalNode.GetSlotOffset(1));

    /// <summary>
    /// Gets the name of the feature.
    /// </summary>
    public SyntaxToken Name => new(this, InternalNode.name, Position + InternalNode.GetSlotOffset(2));

    /// <summary>
    /// Gets the description of the feature.
    /// </summary>
    public DescriptionSyntax? Description => InternalNode.description == null ? null : new(InternalNode.description, this, Position + InternalNode.GetSlotOffset(3));

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index) => null;
}
