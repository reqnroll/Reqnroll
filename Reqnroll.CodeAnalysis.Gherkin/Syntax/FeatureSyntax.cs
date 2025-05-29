namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Feature)]
public sealed partial class FeatureSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.FeatureKeyword, "The token that represents the \"Feature\" keyword.", LocatedAfter = nameof(Tags))]
    [ParameterGroup("Common")]
    public partial SyntaxToken FeatureKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The name of the feature.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken Name { get; }

    [SyntaxSlot(SyntaxKind.DescriptionLiteralToken, "The optional description of the feature.")]
    public partial SyntaxTokenList Description { get; }

    [SyntaxSlot(SyntaxKind.Background, "The optional background of the feature.")]
    public partial BackgroundSyntax? Background { get; }

    [SyntaxSlot([SyntaxKind.Scenario, SyntaxKind.ScenarioOutline], "The scenarios which form the feature.")]
    public partial SyntaxList<ScenarioSyntax> Scenarios { get; }

    [SyntaxSlot(SyntaxKind.Rule, "The rules which form the feature.")]
    public partial SyntaxList<RuleSyntax> Rules { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => FeatureKeyword;
}
