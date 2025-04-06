namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
    internal SkippedTokensTriviaSyntax(Internal.SkippedTokensTriviaSyntax node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
    }

    private new Internal.SkippedTokensTriviaSyntax InternalNode => (Internal.SkippedTokensTriviaSyntax)base.InternalNode;

    public SyntaxTokenList Tokens => new(this, InternalNode.tokens, InternalNode.GetSlotOffset(0));

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index) => null;
}
