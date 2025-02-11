using Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class DocStringSyntax : SyntaxNode
{
    internal DocStringSyntax(RawNode node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }
}
