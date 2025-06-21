namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents an example in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Example)]
public sealed partial class ExampleSyntax : BehaviorDeclarationSyntax
{
    [SyntaxSlot(
        SyntaxKind.ExampleKeyword,
        "The token that represents the \"Example\" keyword.",
        LocatedAfter = nameof(Tags))]
    [ParameterGroup("Minimal")]
    [ParameterGroup("Untagged")]
    public partial SyntaxToken ExampleKeyword { get; }

    [SyntaxSlot(SyntaxKind.Examples, "The examples accompanying the example.")]
    [ParameterGroup("Minimal")]
    [ParameterGroup("Untagged")]
    public partial SyntaxList<ExamplesSyntax> Examples { get; }

    public override SyntaxToken GetKeywordToken() => ExampleKeyword;
}
