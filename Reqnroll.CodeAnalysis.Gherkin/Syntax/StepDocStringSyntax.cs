namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents a Doc String argument of a step.
/// </summary>
[SyntaxNode(SyntaxKind.StepDocString)]
public partial class StepDocStringSyntax : StepDataSyntax
{
    [SyntaxSlot(SyntaxKind.DocString, "The Doc String argument of the step data.")]
    public partial DocStringSyntax DocString { get; }
}
