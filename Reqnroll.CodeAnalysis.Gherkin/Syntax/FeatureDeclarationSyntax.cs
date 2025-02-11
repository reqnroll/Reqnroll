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

    private new Internal.FeatureDeclarationSyntax RawNode => (Internal.FeatureDeclarationSyntax)base.RawNode;

    public override SyntaxTokenList Tags => new();

    /// <summary>
    /// Gets the token that represents the <c>Feature:</c> keyword.
    /// </summary>
    public SyntaxToken FeatureKeyword => new(this, RawNode.keyword, Position + RawNode.GetSlotOffset(0));

    /// <summary>
    /// Gets the token that represents the colon following the keyword.
    /// </summary>
    public SyntaxToken ColonToken => new(this, RawNode.colon, Position + RawNode.GetSlotOffset(1));

    /// <summary>
    /// Gets the name of the feature.
    /// </summary>
    public SyntaxToken Name => new(this, RawNode.name, Position + RawNode.GetSlotOffset(2));

    /// <summary>
    /// Gets the description of the feature.
    /// </summary>
    public DescriptionSyntax? Description => RawNode.description == null ? null : new(RawNode.description, this, Position + RawNode.GetSlotOffset(3));

    internal override SyntaxNode? GetNodeSlot(int index) => null;
}
