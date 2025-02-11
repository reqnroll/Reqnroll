namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class LanguageCommentTriviaSyntax : CommentTriviaSyntax
{
    internal LanguageCommentTriviaSyntax(Internal.LanguageCommentTriviaSyntax node, SyntaxNode parent, SyntaxTrivia parentTrivia, int position) 
        : base(node, parent, parentTrivia, position)
    {
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }
}
