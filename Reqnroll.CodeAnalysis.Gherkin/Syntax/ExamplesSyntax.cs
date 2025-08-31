namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Examples)]
[SyntaxConstructor(nameof(ExamplesKeyword), nameof(ColonToken), nameof(Name), nameof(Description), nameof(Table))]
public sealed partial class ExamplesSyntax : DeclarationSyntax
{
    [SyntaxSlot(
        SyntaxKind.ExamplesKeyword,
        "The token that represents the \"Examples\" keyword.",
        LocatedAfter = nameof(Tags))]
    public partial SyntaxToken ExamplesKeyword { get; }

    [SyntaxSlot(SyntaxKind.Table, "The table which forms the examples.", LocatedAfter = nameof(Description))]
    public partial TableSyntax? Table { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => ExamplesKeyword;
}
