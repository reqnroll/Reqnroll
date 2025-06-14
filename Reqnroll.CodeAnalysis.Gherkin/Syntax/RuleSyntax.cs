namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Rule)]
public partial class RuleSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.RuleKeyword, "The token that represents the \"Rule\" keyword.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxToken RuleKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The name of the rule.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial LiteralTextSyntax? Name { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The optional description of the rule.")]
    [ParameterGroup("Common")]
    public partial LiteralTextSyntax? Description { get; }

    [SyntaxSlot(SyntaxKind.Background, "The optional background of the rule.")]
    [ParameterGroup("Common")]
    public partial BackgroundSyntax? Background { get; }

    [SyntaxSlot([SyntaxKind.Scenario, SyntaxKind.ScenarioOutline], "The scenarios which describe the rule.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxList<ScenarioSyntax> Scenarios { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => RuleKeyword;
}
