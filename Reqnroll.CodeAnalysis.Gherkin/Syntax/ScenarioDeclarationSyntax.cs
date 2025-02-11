namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public class ScenarioDeclarationSyntax : DeclarationSyntax
{
    internal ScenarioDeclarationSyntax(Internal.DeclarationSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    public override SyntaxTokenList Tags => new();

    internal override SyntaxNode? GetNodeSlot(int index)
    {
        return null;
    }
}
