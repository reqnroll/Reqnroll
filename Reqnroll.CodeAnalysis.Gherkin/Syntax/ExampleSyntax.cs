using System.Xml.Linq;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents an example in a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.Example)]
[SyntaxConstructor(nameof(ExampleKeyword), nameof(Name), nameof(Steps))]
[SyntaxConstructor(nameof(ExampleKeyword), nameof(ColonToken), nameof(Name), nameof(Description), nameof(Steps), nameof(Examples))]
public sealed partial class ExampleSyntax : BehaviorDeclarationSyntax
{
    [SyntaxSlot(
        SyntaxKind.ExampleKeyword,
        "The token that represents the \"Example\" keyword.",
        LocatedAfter = nameof(Tags))]
    public partial SyntaxToken ExampleKeyword { get; }

    [SyntaxSlot(SyntaxKind.Examples, "The examples accompanying the example.")]
    public partial SyntaxList<ExamplesSyntax> Examples { get; }

    public override SyntaxToken GetKeywordToken() => ExampleKeyword;
}
