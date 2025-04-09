namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal abstract class InternalStructuredTriviaSyntax : InternalNode
{
    protected InternalStructuredTriviaSyntax(SyntaxKind kind) : base(kind)
    {
        SetFlag(NodeFlags.ContainsStructuredTrivia);
    }

    public override bool IsStructuredTrivia => true;
}
