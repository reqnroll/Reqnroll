﻿namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

[SyntaxNode(SyntaxKind.GherkinDocument)]
public partial class GherkinDocumentSyntax : SyntaxNode
{
    [SyntaxSlot(SyntaxKind.Feature, "The feature declaration syntax contained by the file structure.")]
    [ParameterGroup("Common")]
    public partial FeatureSyntax? FeatureDeclaration { get; }

    [SyntaxSlot(SyntaxKind.EndOfFileToken, "The token which represents the end of the source file.")]
    public partial SyntaxToken EndOfFileToken { get; }
}
