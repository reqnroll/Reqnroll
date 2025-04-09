namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public abstract class StructuredTriviaSyntax : SyntaxNode
{    
    internal StructuredTriviaSyntax(InternalStructuredTriviaSyntax node, SyntaxNode? parent, int position)
        : base(node, parent, position)
    {
    }

    internal StructuredTriviaSyntax(SyntaxTrivia parentTrivia, InternalStructuredTriviaSyntax node, SyntaxNode parent, int position)
        : base(node, parent, position)
    {
        ParentTrivia = parentTrivia;
    }

    public SyntaxTrivia ParentTrivia { get; }
}
