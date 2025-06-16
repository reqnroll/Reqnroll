namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Scenario)]
public partial class ScenarioSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.ScenarioKeyword, "The token that represents the \"Scenario\" keyword.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxToken ScenarioKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot([SyntaxKind.LiteralText, SyntaxKind.InterpolatedText], "The name of the scenario.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial PlainTextSyntax? Name { get; }

    [SyntaxSlot([SyntaxKind.LiteralText, SyntaxKind.InterpolatedText], "The optional description of the scenario.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial PlainTextSyntax? Description { get; }

    [SyntaxSlot(SyntaxKind.Step, "The steps of the scenario.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxList<StepSyntax> Steps { get; }

    [SyntaxSlot(SyntaxKind.Examples, "The examples accompanying the scenario.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxList<ExamplesSyntax> Examples { get; }

    public override SyntaxToken GetKeywordToken() => ScenarioKeyword;
}
