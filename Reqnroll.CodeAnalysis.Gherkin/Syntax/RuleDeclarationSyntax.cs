namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class RuleDeclarationSyntax : DeclarationSyntax
{
    internal RuleDeclarationSyntax(Internal.DeclarationSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    public override SyntaxTokenList Tags => new();

    internal override SyntaxNode? GetSlotAsSyntaxNode(int index)
    {
        return null;
    }
}
