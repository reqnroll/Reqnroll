using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
    public readonly RawNode? tokens;

    public SkippedTokensTriviaSyntax(RawNode? tokens) : base(SyntaxKind.SkippedTokensTrivia)
    {
        if (tokens != null)
        {
            this.tokens = tokens;
            IncludeChild(tokens);
        }

        SetFlag(NodeFlags.ContainsSkippedText);
    }

    public override Syntax.StructuredTriviaSyntax? CreateStructuredTriviaSyntaxNode(SyntaxTrivia parent) =>
        new Syntax.SkippedTokensTriviaSyntax(this, parent.Token.Parent, parent.Position);

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position) => 
        new Syntax.SkippedTokensTriviaSyntax(this, parent, position);

    public override int SlotCount => 1;

    public override RawNode? GetSlot(int index)
    {
        return index switch
        {
            0 => tokens,
            _ => null
        };
    }

    public override RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        throw new NotImplementedException();
    }

    public override RawNode WithDiagnostics(ImmutableArray<RawDiagnostic> diagnostics)
    {
        throw new NotImplementedException();
    }
}
