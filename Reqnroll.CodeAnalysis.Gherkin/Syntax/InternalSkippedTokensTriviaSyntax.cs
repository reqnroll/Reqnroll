using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// Represents the internal state of a skipped tokens trivia syntax.
/// </summary>
internal class InternalSkippedTokensTriviaSyntax : InternalStructuredTriviaSyntax
{
    public readonly InternalNode? tokens;

    public InternalSkippedTokensTriviaSyntax(InternalNode? tokens) : base(SyntaxKind.SkippedTokensTrivia)
    {
        if (tokens != null)
        {
            this.tokens = tokens;
            IncludeChild(tokens);
        }

        SetFlag(NodeFlags.ContainsSkippedText);
    }

    public InternalSkippedTokensTriviaSyntax(
        InternalNode? tokens,
        ImmutableArray<InternalDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : 
        base(SyntaxKind.SkippedTokensTrivia, diagnostics, annotations)
    {
        if (tokens != null)
        {
            this.tokens = tokens;
            IncludeChild(tokens);
        }

        SetFlag(NodeFlags.ContainsSkippedText);
    }

    internal override StructuredTriviaSyntax? CreateStructuredTriviaSyntaxNode(SyntaxTrivia trivia)
    {
        return new SkippedTokensTriviaSyntax(this, trivia, trivia.Position);
    }

    internal override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        CodeAnalysisDebug.Assert(
            parent == null,
            "Cannot specify a parent when creating a syntax node from a structured trivia. " +
            "To specify a parent, instead use CreateStructuredTriviaSyntaxNode(SyntaxTrivia)");

        return new SkippedTokensTriviaSyntax(this, new SyntaxTrivia(default, this, position), position);
    }

    public override int SlotCount => 1;

    public override InternalNode? GetSlot(int index)
    {
        return index switch
        {
            0 => tokens,
            _ => null
        };
    }

    /// <inheritdoc />
    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        return new InternalSkippedTokensTriviaSyntax(
            tokens,
            diagnostics,
            GetAnnotations());
    }

    /// <inheritdoc />
    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new InternalSkippedTokensTriviaSyntax(
            tokens,
            GetAttachedDiagnostics(),
            annotations);
    }
}
