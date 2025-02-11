namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class ScenarioOutlineDeclarationSyntax : DeclarationSyntax
{
    internal ScenarioOutlineDeclarationSyntax(Internal.DeclarationSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    public override SyntaxTokenList Tags => new();

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }
}
