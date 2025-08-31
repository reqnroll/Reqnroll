namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a comment which specifies a Gherkin directive.
/// </summary>
[SyntaxNode(SyntaxKind.DirectiveCommentTrivia)]
public sealed partial class DirectiveCommentTriviaSyntax : StructuredTriviaSyntax
{
    [SyntaxSlot(SyntaxKind.HashToken, "The token that represents the leading hash symbol (`#`).")]
    public partial SyntaxToken HashToken { get; }

    [SyntaxSlot(SyntaxKind.DirectiveIdentifierToken, "The token that represents the name of the directive.")]
    public partial SyntaxToken Name { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.DirectiveValueToken, "The token containing the value of the directive.")]
    public partial SyntaxToken Value { get; }
}
