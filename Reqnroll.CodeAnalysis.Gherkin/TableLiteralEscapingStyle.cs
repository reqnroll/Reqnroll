using Reqnroll.CodeAnalysis.Gherkin.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal class TableLiteralEscapingStyle : DefaultLiteralEscapingStyle
{
    protected override bool RequiresEscaping(char c)
    {
        // In the context of a table, we also need to escape the pipe character '|'
        return c == '|' || base.RequiresEscaping(c);
    }

    protected override bool TryEncode(char c, [NotNullWhen(true)] out string? encoded)
    {
        if (c == '|')
        {
            encoded = "\\|";
            return true;
        }

        return base.TryEncode(c, out encoded);
    }

    protected override bool TryUnescapeSequence(SourceTextSpan sourceTextSpan, StringBuilder stringBuilder, out int consumed)
    {
        if (sourceTextSpan[0] == '|')
        {
            stringBuilder.Append('|');
            consumed = 1;
            return true;
        }

        return base.TryUnescapeSequence(sourceTextSpan, stringBuilder, out consumed);
    }
}
