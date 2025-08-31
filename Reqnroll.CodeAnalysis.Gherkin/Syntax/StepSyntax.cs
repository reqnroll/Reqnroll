namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a step in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Step)]
public partial class StepSyntax : SyntaxNode
{
    [SyntaxSlot([
        SyntaxKind.ContextStepKeyword,
        SyntaxKind.ActionStepKeyword,
        SyntaxKind.OutcomeStepKeyword,
        SyntaxKind.ConjunctionStepKeyword,
        SyntaxKind.ConjunctionStepKeyword,
        SyntaxKind.WildcardStepKeyword],
        "The token that represents the keyword of the step.")]
    public partial SyntaxToken StepKeyword { get; }

    [SyntaxSlot(SyntaxKind.StepTextToken, "The text of the step following the keyword.")]
    public partial SyntaxToken Text { get; }

    [SyntaxSlot([SyntaxKind.StepTable, SyntaxKind.StepDocString], "The optional data associated with the step.")]
    public partial StepDataSyntax? Data { get; }
}
