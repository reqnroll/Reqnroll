using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal class RawSyntaxTrivia : RawNode
{
    public RawSyntaxTrivia(SyntaxKind kind, string text) : base(kind, text.Length)
    {
        Text = text;
    }

    public RawSyntaxTrivia(
        SyntaxKind kind,
        string text,
        ImmutableArray<RawDiagnostic> diagnostics,
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

    public override RawNode? GetLeadingTrivia() => null;

    public override RawNode? GetTrailingTrivia() => null;

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new InvalidOperationException();
    }

    public override RawNode? GetSlot(int index)
    {
        throw new InvalidOperationException();
    }

    internal override void WriteTo(TextWriter writer, bool leading, bool trailing)
    {
        writer.Write(Text);
    }

    public override string ToString() => Text;

    public override string ToFullString() => Text;

    public static implicit operator SyntaxTrivia(RawSyntaxTrivia trivia) => new(new SyntaxToken(), trivia, 0);

    public override RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations) =>
        new RawSyntaxTrivia(Kind, Text, GetAttachedDiagnostics(), annotations);

    public override RawNode WithDiagnostics(ImmutableArray<RawDiagnostic> diagnostics) => 
        new RawSyntaxTrivia(Kind, Text, diagnostics, GetAnnotations());
}
