namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents the content type of a Doc String.
/// </summary>
[SyntaxNode(SyntaxKind.DocStringContentType)]
public sealed partial class DocStringContentTypeSyntax : SyntaxNode
{
    [SyntaxSlot(
        SyntaxKind.DocStringContentTypeIdentifierToken,
        "The token representing the content-type of the Doc String.")]
    public partial SyntaxToken ContentType { get; }
}
