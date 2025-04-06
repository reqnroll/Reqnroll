using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class GenericCommentTriviaSyntax : CommentTriviaSyntax
{
    public GenericCommentTriviaSyntax(
        RawSyntaxToken hashToken,
        RawSyntaxToken text) : base(hashToken, SyntaxKind.CommentTrivia)
    {
        this.text = text;
        IncludeChild(text);
    }

    public readonly RawSyntaxToken text;

    public override int SlotCount => 2;

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => hashToken,
            1 => text,
            _ => null
        };
    }

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new NotImplementedException();
    }

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        throw new NotImplementedException();
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        throw new NotImplementedException();
    }
}
