using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class LanguageCommentTriviaSyntax : CommentTriviaSyntax
{
    public LanguageCommentTriviaSyntax(
        InternalSyntaxToken hashToken,
        InternalSyntaxToken languageKeyword,
        InternalSyntaxToken colon,
        InternalSyntaxToken identifier) : base(hashToken, SyntaxKind.LanguageCommentTrivia)
    {
        this.languageKeyword = languageKeyword;
        IncludeChild(languageKeyword);

        this.colon = colon;
        IncludeChild(colon);

        this.identifier = identifier;
        IncludeChild(identifier);
    }

    public readonly InternalSyntaxToken languageKeyword;
    public readonly InternalSyntaxToken colon;
    public readonly InternalSyntaxToken identifier;

    public override int SlotCount => 4;

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => hashToken,
            1 => languageKeyword,
            2 => colon,
            3 => identifier,
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
