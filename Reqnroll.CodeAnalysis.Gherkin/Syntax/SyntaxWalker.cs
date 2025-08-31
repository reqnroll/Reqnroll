namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class SyntaxWalker
{
    public virtual void Visit(SyntaxNode node)
    {
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (child.IsNode)
            {
                Visit(child.AsNode()!);
            }
            else
            {
                Visit(child.AsToken());
            }
        }
    }

    public virtual void Visit(SyntaxToken token)
    {
        foreach (var trivia in token.LeadingTrivia)
        {
            Visit(trivia);
        }

        foreach (var trivia in token.TrailingTrivia)
        {
            Visit(trivia);
        }
    }

    public virtual void Visit(SyntaxTrivia trivia)
    {
        if (trivia.HasStructure)
        {
            Visit(trivia.Structure!);
        }
    }
}
