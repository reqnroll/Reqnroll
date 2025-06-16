namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a comment which specifies the language of the Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.LanguageCommentTrivia)]
public partial class LanguageCommentTriviaSyntax : StructuredTriviaSyntax
{
    [SyntaxSlot(SyntaxKind.HashToken, "The token that represents the leading hash symbol (`#`).")]
    public partial SyntaxToken HashToken { get; }

    [SyntaxSlot(SyntaxKind.LanguageKeyword, "The token that represents the \"language\" keyword.")]
    public partial SyntaxToken LanguageKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The token containing the identifier of the language.")]
    public partial SyntaxToken Identifier { get; }
}
