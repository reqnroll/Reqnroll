namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents plain, non-structural text.
/// </summary>
[SyntaxNode(SyntaxKind.LiteralText)]
public partial class LiteralTextSyntax : PlainTextSyntax
{
    [SyntaxSlot(SyntaxKind.LiteralToken, "The text of the step following the keyword.")]
    public partial SyntaxTokenList Text { get; }
}
