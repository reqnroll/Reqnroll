using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class PlainTextParser(LiteralEscapingStyle escapeStyle)
{
    private readonly record struct Segment(InternalNode? Leading, string Text, InternalNode? Trailing);

    private readonly List<Segment> _segments = [];

    public void AppendText(InternalNode? leading, string text, InternalNode? trailing)
    {
        _segments.Add(new Segment(leading, text, trailing));
    }

    public PlainTextSyntax.Internal? ParseText()
    {
        if (_segments.Count == 0)
        {
            return null;
        }

        return ParseTextAsLiteral();
    }

    private LiteralTextSyntax.Internal ParseTextAsLiteral()
    {
        var tokens = _segments.Select(segment =>
            Literal(segment.Leading, escapeStyle.Escape(segment.Text), segment.Text, segment.Trailing));

        return LiteralText(InternalNode.CreateList(tokens));
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var segment in _segments)
        {
            if (segment.Leading != null)
            {
                sb.Append(segment.Leading.ToString());
            }

            sb.Append(segment.Text);

            if (segment.Trailing != null)
            {
                sb.Append(segment.Trailing.ToString());
            }
        }

        return sb.ToString();
    }
}
