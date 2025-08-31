namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a block of text that is delimited by specific tokens.
/// </summary>
[SyntaxNode(SyntaxKind.DocString)]
[SyntaxConstructor(nameof(StartDelimeter), nameof(Content), nameof(EndDelimeter))]
public sealed partial class DocStringSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.DocStringDelimiterToken, "The delimiter that marks the start of the Doc String.")]
    public partial SyntaxToken StartDelimeter { get; }

    [SyntaxSlot(SyntaxKind.DocStringContentTypeIdentifierToken, "The optional content type of the syntax.")]
    public partial SyntaxToken ContentType { get; }

    [SyntaxSlot(SyntaxKind.DocStringContentToken, "The text token which makes up the content of the Doc String.")]
    public partial SyntaxToken Content { get; }

    [SyntaxSlot(SyntaxKind.DocStringDelimiterToken, "The delimiter that marks the start of the Doc String.")]
    public partial SyntaxToken EndDelimeter { get; }
}
