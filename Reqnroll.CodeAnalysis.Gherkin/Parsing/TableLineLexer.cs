using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class TableLineLexer(TextLine line)
{
    private readonly LineReader _reader = new(line);

    private SourceText Source => line.Text!;

    public static IEnumerable<InternalNode> EnumerateTokens(TextLine line) => new TableLineLexer(line).EnumerateTokens();

    public IEnumerable<InternalNode> EnumerateTokens()
    {                
        while (_reader.MoveNext())
        {
            foreach (var token in ReadTokens())
            { 
                yield return token;
            }
        }
    }

    private IEnumerable<InternalNode> ReadTokens()
    {
        // This read method assumes we always start "after" the previous token, never in the middle
        // of a token being consumed. We check the first character to decide how to treat the block of text
        // and consume the block as a token until a condition is met.
        if (char.IsWhiteSpace(_reader.Current))
        {
            yield return ReadWhitespace();
        }
        else if (_reader.Current == '|')
        {
            // Consume as a single vertical bar token.
            yield return Token(SyntaxKind.VerticalBarToken);
        }
        else if (_reader.Current == '#')
        {
            foreach (var token in ReadComment())
            {
                yield return token;
            }
        }
        else
        {
            foreach (var token in ReadTableCell())
            {
                yield return token;
            }
        }
    }

    private IEnumerable<InternalNode> ReadTableCell()
    {
        // The first step in reading a table cell is to determine the boundaries of the cell.
        var start = _reader.Position;

        // A cell ends when we reach any of the following
        // - a bar ('|')
        // - a comment's start ('#')
        // - the end of the line

        // We must read the sequence of characters to handle escaped characters:
        // For example \\| translates to \|, a backslash followed by an end-of-cell bar,
        // however \| translates to an escaped | character, part of the cell content and not the end of the cell.

        Func<char, bool> consumeCurrent = ConsumeCharacter;

        bool ConsumeCharacter(char current)
        {
            switch (current)
            {
                // If this could be an escape sequence, consume the next character as an escape sequence.
                case '\\':
                    consumeCurrent = ConsumeEscapedCharacter;
                    return true;

                case '#':
                case '|':
                    return false;

                default:
                    return true;
            }
        }

        bool ConsumeEscapedCharacter(char current)
        {
            // Always switch back toe standard consume.
            consumeCurrent = ConsumeCharacter;

            switch (current)
            {
                case '#':
                    return false; // Comment ends the literal.

                default:
                    return true; // Any other character is part of the literal.
            }
        }

        do
        {
            // Stops when the current character is a control character.
            if (!consumeCurrent(_reader.Current))
            {
                break;
            }
        }
        while (_reader.MoveNext()); // Stops when the end of the line is reached.

        var literalEnd = _reader.Position - 1;

        // Consume the text in the section, excluding trailing whitespace.
        var trailing = Source.ReverseConsumeWhitespace(literalEnd, start);
        var end = literalEnd - (trailing?.Width ?? 0);
        var span = TextSpan.FromBounds(start, end + 1);

        var text = Source.ToString(span);
        var value = LiteralEscapingStyle.Table.Unescape(new SourceTextSpan(Source, span));

        _reader.Seek(literalEnd);

        yield return TableLiteral(null, text, value, null);

        if (trailing != null)
        {
            yield return trailing;
        }
    }

    private IEnumerable<InternalSyntaxTrivia> ReadComment()
    {
        var start = _reader.Position;

        // Consume all remaining text on the line as a comment, excluding trailing whitespace.
        var trailing = Source.ReverseConsumeWhitespace(line.End - 1, start);
        var end = line.End - (trailing?.Width ?? 0);
        var span = TextSpan.FromBounds(start, end);

        // Push the reader past the comment and any trailing whitespace.
        _reader.Seek(line.End);

        var comment = Comment(Source, span);

        yield return comment!;

        if (trailing != null)
        {
            yield return trailing;
        }
    }

    private InternalSyntaxTrivia ReadWhitespace()
    {
        var start = _reader.Position;

        // Consume whitespace until non-whitespace is reached and yield whitespace token.
        _reader.SkipUntil(c => !char.IsWhiteSpace(c));
        var span = TextSpan.FromBounds(start, _reader.Position);
        _reader.Seek(_reader.Position - 1);
        return Whitespace(Source, span)!;
    }
}
