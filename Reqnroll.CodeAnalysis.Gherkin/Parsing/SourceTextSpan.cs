using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal readonly ref struct SourceTextSpan
{
    private readonly SourceText _text;

    private readonly TextSpan _span;

    public SourceTextSpan(SourceText text, TextSpan span)
    {
        _text = text;
        _span = span;

        CodeAnalysisDebug.Assert(_span.End < _text.Length + 1, "Span exceeds text");
    }

    public int Length => _span.Length;

    public char this[int index]
    {
        get
        {
            CodeAnalysisDebug.Assert(index < _span.Length, "Index out of range");

            return _text[_span.Start + index];
        }
    }

    public Enumerator GetEnumerator() => new(_text, _span);

    public override string ToString() => _text.ToString(_span);

    public SourceTextSpan Slice(int start) => new (_text, TextSpan.FromBounds(_span.Start + start, _span.End));

    public string Substring(int start, int length)
    {
        var sb = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            sb.Append(this[start + i]);
        }

        return sb.ToString();
    }

    public struct Enumerator(SourceText text, TextSpan span)
    {
        private readonly SourceText _text = text;
        private readonly int _end = span.End;

        private int _current = span.Start - 1;

        public readonly char Current => _text[_current];

        public bool MoveNext()
        {
            if (_current + 1 < _end)
            {
                _current++;
                return true;
            }

            return false;
        }
    }
}
