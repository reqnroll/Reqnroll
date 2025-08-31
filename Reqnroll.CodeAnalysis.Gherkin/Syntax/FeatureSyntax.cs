using System.Xml.Linq;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Feature declaration syntax.
/// </summary>
[SyntaxNode(SyntaxKind.Feature)]
[SyntaxConstructor(nameof(FeatureKeyword), nameof(Name))]
[SyntaxConstructor(
    nameof(FeatureKeyword),
    nameof(ColonToken),
    nameof(Name),
    nameof(Description),
    nameof(Background),
    nameof(Examples),
    nameof(Rules))]
public sealed partial class FeatureSyntax : BehaviorGroupSyntax
{
    [SyntaxSlot(SyntaxKind.FeatureKeyword, "The token that represents the \"Feature\" keyword.", LocatedAfter = nameof(Tags))]
    public partial SyntaxToken FeatureKeyword { get; }

    [SyntaxSlot(SyntaxKind.Rule, "The rules which form the feature.", LocatedAfter = nameof(Examples))]
    public partial SyntaxList<RuleSyntax> Rules { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => FeatureKeyword;
}
