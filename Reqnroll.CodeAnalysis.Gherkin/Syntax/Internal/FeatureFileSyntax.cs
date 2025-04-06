using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class FeatureFileSyntax : InternalNode
{
    public readonly InternalNode? featureDeclaration;
    public readonly InternalNode endOfFile;

    public FeatureFileSyntax(
        InternalNode? featureDeclaration,
        InternalNode endOfFile) : base(SyntaxKind.FeatureFile)
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
        InternalNode? featureDeclaration,
        InternalNode endOfFile,
        ImmutableArray<InternalDiagnostic> diagnostics,
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

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => featureDeclaration,
            1 => endOfFile,
            _ => null
        };
    }

    public override int SlotCount => 2;

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new FeatureFileSyntax(
            featureDeclaration,
            endOfFile,
            GetAttachedDiagnostics(),
            annotations);
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        return new FeatureFileSyntax(
            featureDeclaration,
            endOfFile,
            diagnostics,
            GetAnnotations());
    }
}
