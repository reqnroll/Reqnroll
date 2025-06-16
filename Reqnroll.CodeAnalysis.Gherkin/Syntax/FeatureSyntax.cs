namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Feature)]
public sealed partial class FeatureSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.FeatureKeyword, "The token that represents the \"Feature\" keyword.", LocatedAfter = nameof(Tags))]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial SyntaxToken FeatureKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The name of the feature.")]
    [ParameterGroup("Common")]
    [ParameterGroup("Minimal")]
    public partial LiteralTextSyntax? Name { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The optional description of the feature.")]
    [ParameterGroup("Common")]
    public partial LiteralTextSyntax? Description { get; }

    [SyntaxSlot(SyntaxKind.Background, "The optional background of the feature.")]
    [ParameterGroup("Common")]
    public partial BackgroundSyntax? Background { get; }

    [SyntaxSlot([SyntaxKind.Scenario, SyntaxKind.ScenarioOutline], "The scenarios which form the feature.")]
    [ParameterGroup("Common")]
    public partial SyntaxList<ScenarioSyntax> Scenarios { get; }

    [SyntaxSlot(SyntaxKind.Rule, "The rules which form the feature.")]
    [ParameterGroup("Common")]
    public partial SyntaxList<RuleSyntax> Rules { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => FeatureKeyword;
}
