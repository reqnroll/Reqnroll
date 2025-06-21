namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a block of text that is delimited by specific tokens.
/// </summary>
[SyntaxNode(SyntaxKind.DocString)]
public sealed partial class DocStringSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.DocStringDelimiterToken, "The delimiter that marks the start of the Doc String.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken StartDelimeter { get; }

    [SyntaxSlot(SyntaxKind.DocStringContentType, "The optional content type of the syntax.")]
    public partial DocStringContentTypeSyntax? ContentType { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The text tokens which make up the content of the Doc String.")]
    [ParameterGroup("Common")]
    public partial PlainTextSyntax Content { get; }

    [SyntaxSlot(SyntaxKind.DocStringDelimiterToken, "The delimiter that marks the start of the Doc String.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken EndDelimeter { get; }
}
