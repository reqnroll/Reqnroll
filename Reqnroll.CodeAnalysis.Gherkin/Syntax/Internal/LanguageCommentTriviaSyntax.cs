using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class LanguageCommentTriviaSyntax : CommentTriviaSyntax
{
    public LanguageCommentTriviaSyntax(
        RawSyntaxToken hashToken,
        RawSyntaxToken languageKeyword,
        RawSyntaxToken colon,
        RawSyntaxToken identifier) : base(hashToken, SyntaxKind.LanguageCommentTrivia)
    {
        this.languageKeyword = languageKeyword;
        IncludeChild(languageKeyword);

        this.colon = colon;
        IncludeChild(colon);

        this.identifier = identifier;
        IncludeChild(identifier);
    }

    public readonly RawSyntaxToken languageKeyword;
    public readonly RawSyntaxToken colon;
    public readonly RawSyntaxToken identifier;

    public override int SlotCount => 4;

    public override RawNode? GetSlot(int index)
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

    public override RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        throw new NotImplementedException();
    }
}
