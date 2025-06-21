namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// The base type for declarations in a Gherkin document.
/// </summary>
[SyntaxNode]
public abstract partial class DeclarationSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.List, "The tokens which make up the tags for this declaration.")]
    public abstract SyntaxList<TagSyntax> Tags { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    [ParameterGroup("Untagged")]
    public abstract SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The name of the scenario.")]
    [ParameterGroup("Minimal")]
    [ParameterGroup("Untagged")]
    public abstract PlainTextSyntax? Name { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The optional description of the scenario.")]
    [ParameterGroup("Untagged")]
    public abstract PlainTextSyntax? Description { get; }

    /// <summary>
    /// Gets the keyword token for this declaration.
    /// </summary>
    /// <returns>The token which is the keyword for this declaration.</returns>
    public abstract SyntaxToken GetKeywordToken();
}
