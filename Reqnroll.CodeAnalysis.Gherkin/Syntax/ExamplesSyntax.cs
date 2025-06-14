namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.Examples)]
public partial class ExamplesSyntax : DeclarationSyntax
{
    [SyntaxSlot(SyntaxKind.ExamplesKeyword, "The token that represents the \"Examples\" keyword.")]
    [ParameterGroup("Minimal")]
    [ParameterGroup("Untagged")]
    public partial SyntaxToken ExamplesKeyword { get; }

    [SyntaxSlot(SyntaxKind.ColonToken, "The token that represents the colon following the keyword.")]
    [ParameterGroup("Minimal")]
    [ParameterGroup("Untagged")]
    public partial SyntaxToken ColonToken { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The optional name of the examples.")]
    [ParameterGroup("Untagged")]
    public partial LiteralTextSyntax? Name { get; }

    [SyntaxSlot(SyntaxKind.LiteralText, "The optional description of the examples.")]
    [ParameterGroup("Untagged")]
    public partial LiteralTextSyntax? Description { get; }

    [SyntaxSlot(SyntaxKind.Table, "The table which forms the examples.")]
    [ParameterGroup("Untagged")]
    public partial TableSyntax? Table { get; }

    /// <inheritdoc />
    public override SyntaxToken GetKeywordToken() => ExamplesKeyword;
}
