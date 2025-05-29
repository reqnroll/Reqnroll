namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Rule)]
public partial class RuleSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.FeatureKeyword, "The token that represents the \"Feature\" keyword.", LocatedAfter = nameof(Tags))]
    [ParameterGroup("Common")]
    public partial SyntaxToken RuleKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The name of the rule.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken Name { get; }

    [SyntaxSlot(SyntaxKind.DescriptionLiteralToken, "The optional description of the rule.")]
    public partial SyntaxTokenList Description { get; }

    [SyntaxSlot(SyntaxKind.Background, "The optional background of the rule.")]
    public partial BackgroundSyntax? Background { get; }

    [SyntaxSlot([SyntaxKind.Scenario, SyntaxKind.ScenarioOutline], "The scenarios which describe the rule.")]
    public partial SyntaxList<ScenarioSyntax> Scenarios { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => RuleKeyword;
}
