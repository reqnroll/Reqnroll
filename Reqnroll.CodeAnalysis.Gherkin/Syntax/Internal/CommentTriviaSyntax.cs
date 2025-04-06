namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal abstract class CommentTriviaSyntax : StructuredTriviaSyntax
{
    public readonly RawSyntaxToken hashToken;

    protected CommentTriviaSyntax(RawSyntaxToken hashToken, SyntaxKind kind) : base(kind)
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
