namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a block of descriptive text in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Description)]
public sealed partial class DescriptionSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.DescriptionTextToken, "The lines which make up the description.")]
    public partial SyntaxTokenList Lines { get; }

    /// <summary>
    /// Gets the text of the description.
    /// </summary>
    /// <returns>The text nodes of the description combined into single text value.</returns>
    public string GetText()
    {
        throw new NotImplementedException();
    }
}
