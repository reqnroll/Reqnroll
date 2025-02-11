namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public abstract class StructuredTriviaSyntax : SyntaxNode
{
    internal StructuredTriviaSyntax(Internal.StructuredTriviaSyntax node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
    }

    internal StructuredTriviaSyntax(SyntaxTrivia parentTrivia, Internal.StructuredTriviaSyntax node, SyntaxNode parent, int position)
        : base(node, parent, position)
    {
        ParentTrivia = parentTrivia;
    }

    public SyntaxTrivia ParentTrivia { get; }
}
