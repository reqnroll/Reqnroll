namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class CommentTriviaSyntax : StructuredTriviaSyntax
{
    internal CommentTriviaSyntax(Internal.CommentTriviaSyntax node, SyntaxNode parent, SyntaxTrivia parentTrivia, int position) 
        : base(parentTrivia, node, parent, position)
    {
    }

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index)
    {
        return null;
    }
}
