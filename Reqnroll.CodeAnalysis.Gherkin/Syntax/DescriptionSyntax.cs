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

    internal new Internal.DescriptionSyntax RawNode => (Internal.DescriptionSyntax)base.RawNode;

    public SyntaxTokenList TextTokens => new(this, RawNode.textTokens, Position + RawNode.GetSlotOffset(0));

    internal override SyntaxNode? GetNodeSlot(int index) => null;
}
