namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents the syntax of a Gherkin document.
/// </summary>
[SyntaxNode(SyntaxKind.GherkinDocument)]
[SyntaxConstructor(nameof(FeatureDeclaration))]
public sealed partial class GherkinDocumentSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.Feature, "The feature declaration syntax contained by the file structure.")]
    public partial FeatureSyntax? FeatureDeclaration { get; }

    [SyntaxSlot(SyntaxKind.EndOfFileToken, "The token which represents the end of the source file.")]
    public partial SyntaxToken EndOfFileToken { get; }
}
