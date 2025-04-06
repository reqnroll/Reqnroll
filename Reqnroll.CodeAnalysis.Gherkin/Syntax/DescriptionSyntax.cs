namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a block of descriptive text.
/// </summary>
public class DescriptionSyntax : SyntaxNode
{
    internal DescriptionSyntax(Internal.DescriptionSyntax node) : base(node)
    {
    }

    internal DescriptionSyntax(Internal.DescriptionSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    internal new Internal.DescriptionSyntax InternalNode => (Internal.DescriptionSyntax)base.InternalNode;

    public SyntaxTokenList TextTokens => new(this, InternalNode.textTokens, Position + InternalNode.GetSlotOffset(0));

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index) => null;
}
