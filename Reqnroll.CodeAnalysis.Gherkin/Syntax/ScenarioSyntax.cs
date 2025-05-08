namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Scenario)]
public partial class ScenarioSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.ScenarioKeyword, "The token that represents the \"Scenario\" keyword.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken ScenarioKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.IdentifierToken, "The name of the scenario.")]
    [ParameterGroup("Common")]
    public partial SyntaxToken Name { get; }
}
