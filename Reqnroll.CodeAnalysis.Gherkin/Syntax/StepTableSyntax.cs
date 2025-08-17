namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a table argument of a step.
/// </summary>
[SyntaxNode(SyntaxKind.StepTable)]
public partial class StepTableSyntax : StepDataSyntax
{
    [SyntaxSlot(SyntaxKind.Table, "The table argument of the step data.")]
    public partial TableSyntax Table { get; }
}
