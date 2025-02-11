namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

public abstract class DeclarationSyntax : SyntaxNode
{
    internal DeclarationSyntax(Internal.DeclarationSyntax node) : base(node)
    {
    }

    internal DeclarationSyntax(Internal.DeclarationSyntax node, SyntaxNode? parent, int position) : base(node, parent, position)
    {
    }

    /// <summary>
    /// Gets the tags which apply to this declaration.
    /// </summary>
    public abstract SyntaxTokenList Tags { get; }
}
