namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.GherkinDocument)]
public partial class GherkinDocumentSyntax : SyntaxNode
{
    /// <summary>
    /// Gets the feature declaration syntax contained by the file structure.
    /// </summary>
    [SyntaxSlot(SyntaxKind.Feature)]
    public partial FeatureSyntax? FeatureDeclaration { get; }

    /// <summary>
    /// Gets the token which represents the end of the source file.
    /// </summary>
    [SyntaxSlot(SyntaxKind.EndOfFileToken)]
    public partial SyntaxToken EndOfFileToken { get; }
}
