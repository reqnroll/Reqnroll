namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// The base type for behavior declarations in a Gherkin document.
/// </summary>
[SyntaxNode]
public abstract partial class BehaviorDeclarationSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.Step, "The steps which define the behavior.", LocatedAfter = nameof(Description))]
    [ParameterGroup("Untagged")]
    [ParameterGroup("Minimal")]
    public partial SyntaxList<StepSyntax> Steps { get; }
}
