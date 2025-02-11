namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
    internal SkippedTokensTriviaSyntax(Internal.SkippedTokensTriviaSyntax node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
    }

    private new Internal.SkippedTokensTriviaSyntax RawNode => (Internal.SkippedTokensTriviaSyntax)base.RawNode;

    public SyntaxTokenList Tokens => new(this, RawNode.tokens, RawNode.GetSlotOffset(0));

    internal override SyntaxNode? GetNodeSlot(int index) => null;
}
