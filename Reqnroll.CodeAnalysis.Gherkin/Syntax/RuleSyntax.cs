namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a rule declaration in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Rule)]
public partial class RuleSyntax : BehaviorGroupSyntax
{
    [SyntaxSlot(SyntaxKind.RuleKeyword, "The token that represents the \"Rule\" keyword.", LocatedAfter = nameof(Tags))]
    [ParameterGroup("Untagged")]
    [ParameterGroup("Minimal")]
    public partial SyntaxToken RuleKeyword { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => RuleKeyword;
}
