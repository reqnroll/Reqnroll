namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class DocStringSyntax : SyntaxNode
{
    internal DocStringSyntax(InternalNode node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index)
    {
        return null;
    }
}
