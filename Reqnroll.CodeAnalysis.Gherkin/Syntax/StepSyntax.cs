namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Step specification syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Step)]
public partial class StepSyntax : SyntaxNode
{
    [SyntaxSlot(
        [SyntaxKind.GivenKeyword, SyntaxKind.WhenKeyword, SyntaxKind.ThenKeyword, SyntaxKind.AndKeyword],
        "The token that represents the keyword of the step.")]
    public partial SyntaxToken StepKeyword { get; }

    [SyntaxSlot([SyntaxKind.TextLiteralToken], "The text of the step following the keyword.")]
    public partial SyntaxTokenList Text { get; }
}
