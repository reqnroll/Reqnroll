namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal abstract class CommentTriviaSyntax : InternalStructuredTriviaSyntax
{
    public readonly InternalSyntaxToken hashToken;

    protected CommentTriviaSyntax(InternalSyntaxToken hashToken, SyntaxKind kind) : base(kind)
    {
        this.hashToken = hashToken;
        IncludeChild(hashToken);
    }

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => hashToken,
            _ => null
        };
    }
}
