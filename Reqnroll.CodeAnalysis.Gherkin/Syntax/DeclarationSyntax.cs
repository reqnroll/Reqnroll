namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// The base type for declarations in a Gherkin document.
/// </summary>
[SyntaxNode]
public abstract partial class DeclarationSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.List, "The tokens which make up the tags for this declaration.")]
    public partial SyntaxList<TagSyntax> Tags { get; }

    /// <summary>
    /// Gets the keyword token for this declaration.
    /// </summary>
    /// <returns>The token which is the keyword for this declaration.</returns>
    public abstract SyntaxToken GetKeywordToken();
}
