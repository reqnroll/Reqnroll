namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Examples)]
public sealed partial class ExamplesSyntax : DeclarationSyntax
{
    [SyntaxSlot(
        SyntaxKind.ExamplesKeyword,
        "The token that represents the \"Examples\" keyword.",
        LocatedAfter = nameof(Tags))]
    [ParameterGroup("Minimal")]
    [ParameterGroup("Untagged")]
    public partial SyntaxToken ExamplesKeyword { get; }

    [SyntaxSlot(SyntaxKind.Table, "The table which forms the examples.", LocatedAfter = nameof(Description))]
    [ParameterGroup("Untagged")]
    public partial TableSyntax? Table { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => ExamplesKeyword;
}
