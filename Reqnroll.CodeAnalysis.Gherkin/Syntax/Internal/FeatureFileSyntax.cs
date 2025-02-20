using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class FeatureFileSyntax : RawNode
{
    public readonly RawNode? featureDeclaration;
    public readonly RawNode endOfFile;

    public FeatureFileSyntax(
        RawNode? featureDeclaration,
        RawNode endOfFile) : base(SyntaxKind.FeatureFile)
    {
        this.featureDeclaration = featureDeclaration;
        if (featureDeclaration != null)
        {
            IncludeChild(featureDeclaration);
        }

        this.endOfFile = endOfFile;
        IncludeChild(endOfFile);
    }

    public FeatureFileSyntax(
        RawNode? featureDeclaration,
        RawNode endOfFile,
        ImmutableArray<RawDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(SyntaxKind.FeatureFile, diagnostics, annotations)
    {
        this.featureDeclaration = featureDeclaration;
        if (featureDeclaration != null)
        {
            IncludeChild(featureDeclaration);
        }

        this.endOfFile = endOfFile;
        IncludeChild(endOfFile);
    }

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        return new Syntax.FeatureFileSyntax(this, parent, position);
    }

    public override RawNode? GetSlot(int index)
    {
        return index switch
        {
            0 => featureDeclaration,
            1 => endOfFile,
            _ => null
        };
    }

    public override int SlotCount => 2;

    public override RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new FeatureFileSyntax(
            featureDeclaration,
            endOfFile,
            GetAttachedDiagnostics(),
            annotations);
    }

    public override RawNode WithDiagnostics(ImmutableArray<RawDiagnostic> diagnostics)
    {
        return new FeatureFileSyntax(
            featureDeclaration,
            endOfFile,
            diagnostics,
            GetAnnotations());
    }
}
