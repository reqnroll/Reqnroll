using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class DescriptionSyntax : RawNode
{
    public readonly RawNode? textTokens;

    public DescriptionSyntax(RawNode? textTokens) : base(SyntaxKind.Description)
    {
        this.textTokens = textTokens;
        if (textTokens != null)
        {
            IncludeChild(textTokens);
        }
    }

    public DescriptionSyntax(
        RawNode? textTokens,
        ImmutableArray<RawDiagnostic> diagnostics,
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

    public override RawNode? GetSlot(int index)
    {
        return index switch
        {
            0 => textTokens,
            _ => throw new IndexOutOfRangeException(),
        };
    }

    public override int SlotCount => 1;

    public override RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new DescriptionSyntax(
            textTokens,
            GetAttachedDiagnostics(),
            annotations);
    }

    public override RawNode WithDiagnostics(ImmutableArray<RawDiagnostic> diagnostics)
    {
        return new DescriptionSyntax(
            textTokens,
            diagnostics,
            GetAnnotations());
    }
}
