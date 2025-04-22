namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Background declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Background)]
public partial class BackgroundSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.BackgroundKeyword, "The token that represents the \"Background\" keyword.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken BackgroundKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The name of the background.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken Name { get; }

    [SyntaxSlot(SyntaxKind.Step, "The steps which form the background.")]
    [ParameterGroup("Common")]
    public partial SyntaxList<StepSyntax> Steps { get; }
}
