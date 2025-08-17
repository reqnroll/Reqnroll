using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

internal partial class InternalSyntaxToken : InternalNode
{
    private readonly string _text;
    private readonly InternalNode? _leading;
    private readonly InternalNode? _trailing;

    public InternalSyntaxToken(SyntaxKind kind, string text) : base(kind, text.Length)
    {
        _text = text;

        SetFlag(NodeFlags.IsNotMissing);
    }

    public InternalSyntaxToken(
        SyntaxKind kind,
        string text,
        InternalNode? leading,
        InternalNode? trailing) : base(kind, text.Length)
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

    public InternalSyntaxToken(
        SyntaxKind kind,
        string text,
        InternalNode? leading,
        InternalNode? trailing,
        ImmutableArray<InternalDiagnostic> diagnostics,
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

    public override InternalNode? GetSlot(int index) => throw new InvalidOperationException();

    public override bool IsToken => true;

    public override int Width => _text.Length;

    internal override SyntaxNode CreateSyntaxNode(SyntaxNode? parent, int position)
    {
        throw new InvalidOperationException(SyntaxExceptionMessages.CannotCreateSyntaxNodeFromSyntaxToken);
    }

    public override string ToString() => _text;

    internal static InternalSyntaxToken CreateMissing(
        SyntaxKind kind,
        InternalNode? leadingTrivia,
        InternalNode? trailingTrivia)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        var token = new InternalSyntaxToken(
            kind,
            GetText(kind),
            leadingTrivia,
            trailingTrivia,
            ImmutableArray<InternalDiagnostic>.Empty,
            ImmutableArray<SyntaxAnnotation>.Empty);

        token.ClearFlag(NodeFlags.IsNotMissing);

        return token;
    }

    internal static InternalSyntaxToken Create(
        SyntaxKind kind,
        InternalNode? leading,
        InternalNode? trailing)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        if (!kind.IsTextlessToken())
        {
            return CreateMissing(kind, leading, trailing);
        }

        return new InternalSyntaxToken(
            kind,
            GetText(kind),
            leading,
            trailing,
            ImmutableArray<InternalDiagnostic>.Empty,
            ImmutableArray<SyntaxAnnotation>.Empty);
    }

    internal static InternalSyntaxToken Create(
        SyntaxKind kind,
        string text,
        InternalNode? leading,
        InternalNode? trailing)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        return new InternalSyntaxToken(
            kind,
            text,
            leading,
            trailing,
            ImmutableArray<InternalDiagnostic>.Empty,
            ImmutableArray<SyntaxAnnotation>.Empty);
    }

    internal static InternalSyntaxToken Create(
        SyntaxKind kind,
        string text,
        string value,
        InternalNode? leading,
        InternalNode? trailing)
    {
        if (!kind.IsToken())
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "This method can only be used to create tokens.");
        }

        return new InternalSyntaxTokenWithValue<string>(kind, text, value, leading, trailing);
    }

    private static string GetText(SyntaxKind kind)
    {
        return kind switch
        {
            SyntaxKind.VerticalBarToken => "|",
            SyntaxKind.ColonToken => ":",
            SyntaxKind.AtToken => "@",
            SyntaxKind.HashToken => "#",
            SyntaxKind.LessThanToken => "<",
            SyntaxKind.GreaterThanToken => ">",
            _ => string.Empty
        };
    }

    public override bool HasLeadingTrivia => _leading != null;

    public override InternalNode? GetLeadingTrivia() => _leading;

    public override int GetLeadingTriviaWidth() => _leading?.FullWidth ?? 0;

    public override bool HasTrailingTrivia => _trailing != null;

    public override InternalNode? GetTrailingTrivia() => _trailing;

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

    public override InternalNode WithLeadingTrivia(InternalNode? trivia)
    {
        return new InternalSyntaxToken(Kind, _text, trivia, _trailing, GetAttachedDiagnostics(), GetAnnotations());
    }

    public override InternalNode WithTrailingTrivia(InternalNode? trivia)
    {
        return new InternalSyntaxToken(Kind, _text, _leading, trivia, GetAttachedDiagnostics(), GetAnnotations());
    }

    public override InternalNode WithAnnotations(ImmutableArray<SyntaxAnnotation> annotations)
    {
        return new InternalSyntaxToken(Kind, _text, _leading, _trailing, GetAttachedDiagnostics(), annotations);
    }

    public override InternalNode WithDiagnostics(ImmutableArray<InternalDiagnostic> diagnostics)
    {
        return new InternalSyntaxToken(Kind, _text, _leading, _trailing, diagnostics, GetAnnotations());
    }

    public static implicit operator SyntaxToken(InternalSyntaxToken token) => new(token, null, 0);
}
