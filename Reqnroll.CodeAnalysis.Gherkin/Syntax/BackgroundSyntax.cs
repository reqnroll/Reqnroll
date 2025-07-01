namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Background declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Background)]
public sealed partial class BackgroundSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.BackgroundKeyword, "The token that represents the \"Background\" keyword.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxToken BackgroundKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot([SyntaxKind.LiteralText, SyntaxKind.InterpolatedText], "The optional name of the background.")]
    [ParameterGroup("Common")]
    public partial PlainTextSyntax? Name { get; }

    [SyntaxSlot([SyntaxKind.LiteralText, SyntaxKind.InterpolatedText], "The optional description of the background.")]
    [ParameterGroup("Common")]
    public partial PlainTextSyntax? Description { get; }

    [SyntaxSlot(SyntaxKind.Step, "The steps which form the background.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxList<StepSyntax> Steps { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => BackgroundKeyword;
}
