using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class DescriptionSyntax : InternalNode
{
    public readonly InternalNode? textTokens;

    public DescriptionSyntax(InternalNode? textTokens) : base(SyntaxKind.Description)
    {
        this.textTokens = textTokens;
        if (textTokens != null)
        {
            IncludeChild(textTokens);
        }
    }

    public DescriptionSyntax(
        InternalNode? textTokens,
        ImmutableArray<InternalDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(SyntaxKind.Description, diagnostics, annotations)
    {
        this.textTokens = textTokens;
        if (textTokens != null)
        {
            IncludeChild(textTokens);
        }
    }

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        return new Syntax.DescriptionSyntax(this, parent, position);
    }

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => textTokens,
            _ => throw new IndexOutOfRangeException(),
        };
    }

    public override int SlotCount => 1;

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new DescriptionSyntax(
            textTokens,
            GetAttachedDiagnostics(),
            annotations);
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        return new DescriptionSyntax(
            textTokens,
            diagnostics,
            GetAnnotations());
    }
}
