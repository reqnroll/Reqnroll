namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
    internal SkippedTokensTriviaSyntax(InternalSkippedTokensTriviaSyntax node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
    }

    private new InternalSkippedTokensTriviaSyntax InternalNode => (InternalSkippedTokensTriviaSyntax)base.InternalNode;

    public SyntaxTokenList Tokens => new(this, InternalNode.tokens, InternalNode.GetSlotOffset(0));

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index) => null;
}
