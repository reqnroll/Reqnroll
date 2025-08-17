namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Base type for declarations that represent a group of behaviors in a Gherkin document.
/// </summary>
[SyntaxNode]
public abstract partial class BehaviorGroupSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.Background, "The optional background of the group.", LocatedAfter = nameof(Description))]
    [ParameterGroup("Untagged")]
    public abstract BackgroundSyntax? Background { get; }

    [SyntaxSlot(SyntaxKind.Example, "The examples declared by this group.")]
    [ParameterGroup("Untagged")]
    public abstract SyntaxList<ExampleSyntax> Examples { get; }
}
