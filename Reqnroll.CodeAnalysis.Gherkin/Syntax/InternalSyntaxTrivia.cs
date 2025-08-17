using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

/// <summary>
/// The internal representation of syntax trivia.
/// </summary>
internal class InternalSyntaxTrivia : InternalNode
{
    public InternalSyntaxTrivia(SyntaxKind kind, string text) : base(kind, text.Length)
    {
        Text = text;
    }

    public InternalSyntaxTrivia(
        SyntaxKind kind,
        string text,
        ImmutableArray<InternalDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(kind, text.Length, diagnostics, annotations)
    {
        Text = text;
    }

    public string Text { get; }

    public override int Width => Text.Length;

    public override bool IsTrivia => true;

    public override bool HasLeadingTrivia => false;

    public override bool HasTrailingTrivia => false;

    public override int SlotCount => 0;

    public override int GetLeadingTriviaWidth() => 0;

    public override int GetTrailingTriviaWidth() => 0;

    public override InternalNode? GetLeadingTrivia() => null;

    public override InternalNode? GetTrailingTrivia() => null;

    internal override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new InvalidOperationException();
    }

    public override InternalNode? GetSlot(int index)
    {
        throw new InvalidOperationException();
    }

    internal override void WriteTo(TextWriter writer, bool leading, bool trailing)
    {
        writer.Write(Text);
    }

    public override string ToString() => Text;

    public override string ToFullString() => Text;

    public static implicit operator SyntaxTrivia(InternalSyntaxTrivia? trivia) => new(new SyntaxToken(), trivia, 0);

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations) =>
        new InternalSyntaxTrivia(Kind, Text, GetAttachedDiagnostics(), annotations);

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics) => 
        new InternalSyntaxTrivia(Kind, Text, diagnostics, GetAnnotations());

    public override InternalNode WithLeadingTrivia(InternalNode? trivia) => throw new InvalidOperationException();

    public override InternalNode WithTrailingTrivia(InternalNode? trivia) => throw new InvalidOperationException();
}
