namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Feature)]
public sealed partial class FeatureSyntax : BehaviorGroupSyntax
{
    [SyntaxSlot(SyntaxKind.FeatureKeyword, "The token that represents the \"Feature\" keyword.", LocatedAfter = nameof(Tags))]
    [ParameterGroup("Untagged")]
    [ParameterGroup("Minimal")]
    public partial SyntaxToken FeatureKeyword { get; }

    [SyntaxSlot(SyntaxKind.Rule, "The rules which form the feature.", LocatedAfter = nameof(Examples))]
    [ParameterGroup("Untagged")]
    public partial SyntaxList<RuleSyntax> Rules { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => FeatureKeyword;
}
