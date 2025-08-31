namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a rule declaration in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Rule)]
[SyntaxConstructor(nameof(RuleKeyword), nameof(ColonToken), nameof(Name), nameof(Description), nameof(Background), nameof(Examples))]
public sealed partial class RuleSyntax : BehaviorGroupSyntax
{
    [SyntaxSlot(SyntaxKind.RuleKeyword, "The token that represents the \"Rule\" keyword.", LocatedAfter = nameof(Tags))]
    public partial SyntaxToken RuleKeyword { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => RuleKeyword;
}
