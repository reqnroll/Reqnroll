using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

internal partial class RawSyntaxToken : RawNode
{
    private readonly string _text;
    private readonly RawNode? _leading;
    private readonly RawNode? _trailing;

    public RawSyntaxToken(SyntaxKind kind, string text) : base(kind, text.Length)
    {
        _text = text;

        SetFlag(NodeFlags.IsNotMissing);
    }

    public RawSyntaxToken(
        SyntaxKind kind,
        string text,
        RawNode? leading,
        RawNode? trailing) : base(kind, text.Length)
    {
        _text = text;
        _leading = leading;
        _trailing = trailing;

        SetFlag(NodeFlags.IsNotMissing);

        if (leading != null)
        {
            IncludeChild(leading);
        }

        if (trailing != null)
        {
            IncludeChild(trailing);
        }
    }

    public RawSyntaxToken(
        SyntaxKind kind,
        string text,
        RawNode? leading,
        RawNode? trailing,
        ImmutableArray<RawDiagnostic> diagnostics,
        ImmutableArray<SyntaxAnnotation> annotations) : base(kind, text.Length, diagnostics, annotations)
    {
        _text = text;
        _leading = leading;
        _trailing = trailing;

        SetFlag(NodeFlags.IsNotMissing);

        if (leading != null)
        {
            IncludeChild(leading);
        }

        if (trailing != null)
        {
            IncludeChild(trailing);
        }
    }

    public override int SlotCount => 0;

    public override RawNode? GetSlot(int index) => throw new InvalidOperationException();

    public override bool IsToken => true;

    public override int Width => _text.Length;

    public override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new InvalidOperationException();
    }

    public override string ToString() => _text;

    internal static RawSyntaxToken CreateMissing(
        SyntaxKind kind,
        RawNode? leadingTrivia,
        RawNode? trailingTrivia)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        var token = new RawSyntaxToken(
            kind,
            GetText(kind),
            leadingTrivia,
            trailingTrivia,
            ImmutableArray<RawDiagnostic>.Empty,
            ImmutableArray<SyntaxAnnotation>.Empty);

        token.ClearFlag(NodeFlags.IsNotMissing);

        return token;
    }

    internal static RawSyntaxToken Create(
        SyntaxKind kind,
        RawNode? leading,
        RawNode? trailing)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        if (!kind.IsTextlessToken())
        {
            return CreateMissing(kind, leading, trailing);
        }

        return new RawSyntaxToken(
            kind,
            GetText(kind),
            leading,
            trailing,
            ImmutableArray<RawDiagnostic>.Empty,
            ImmutableArray<SyntaxAnnotation>.Empty);
    }

    internal static RawSyntaxToken Create(
        SyntaxKind kind,
        string text,
        RawNode? leading,
        RawNode? trailing)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        return new RawSyntaxToken(
            kind,
            text,
            leading,
            trailing,
            ImmutableArray<RawDiagnostic>.Empty,
            ImmutableArray<SyntaxAnnotation>.Empty);
    }

    private static string GetText(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.ColonToken => ":",
            _ => string.Empty
        };
    }

    public override bool HasLeadingTrivia => _leading != null;

    public override RawNode? GetLeadingTrivia() => _leading;

    public override int GetLeadingTriviaWidth() => _leading?.FullWidth ?? 0;

    public override bool HasTrailingTrivia => _trailing != null;

    public override RawNode? GetTrailingTrivia() => _trailing;

    public override int GetTrailingTriviaWidth() => _trailing?.FullWidth ?? 0;

    internal override void WriteTo(TextWriter writer, bool leading, bool trailing)
    {
        if (leading && _leading != null)
        {
            _leading.WriteTo(writer, true, true);
        }

        writer.Write(_text);

        if (trailing && _trailing != null)
        {
            _trailing.WriteTo(writer, true, true);
        }
    }

    public override RawNode WithLeadingTrivia(RawNode? trivia)
    {
        return new RawSyntaxToken(Kind, _text, trivia, _trailing, GetAttachedDiagnostics(), GetAnnotations());
    }

    public override RawNode WithTrailingTrivia(RawNode? trivia)
    {
        return new RawSyntaxToken(Kind, _text, _leading, trivia, GetAttachedDiagnostics(), GetAnnotations());
    }

    public override RawNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new RawSyntaxToken(Kind, _text, _leading, _trailing, GetAttachedDiagnostics(), annotations);
    }

    public override RawNode WithDiagnostics(ImmutableArray<RawDiagnostic> diagnostics)
    {
        return new RawSyntaxToken(Kind, _text, _leading, _trailing, diagnostics, GetAnnotations());
    }

    public static implicit operator SyntaxToken(RawSyntaxToken token) => new(null, token, 0);
}
