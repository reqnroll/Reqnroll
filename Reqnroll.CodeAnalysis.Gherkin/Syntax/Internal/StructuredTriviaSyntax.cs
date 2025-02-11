namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal abstract class StructuredTriviaSyntax : RawNode
{
    protected StructuredTriviaSyntax(SyntaxKind kind) : base(kind)
    {
        SetFlag(NodeFlags.ContainsStructuredTrivia);
    }

    public override bool IsStructuredTrivia => true;
}
