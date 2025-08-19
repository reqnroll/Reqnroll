using Reqnroll.CodeAnalysis.Gherkin.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal class TableLiteralEscapingStyle : LiteralEscapingStyle
{
    protected override bool RequiresEscaping(char c)
    {
        // In the context of a table, we only escape:
        // - The pipe character '|', which would otherwise separate columns.
        // - The newline character '\n' which would otherwise start a new row.
        // - The backslash character '\' to separate it from other escape sequences.
        return c is '|' or '\n' or '\\';
    }

    protected override bool TryEncode(char c, [NotNullWhen(true)] out string? encoded)
    {
        encoded = null;

        switch (c)
        {
            case '\\':
                encoded = "\\\\";
                break;
            case '\n':
                encoded = "\\n";
                break;
            case '|':
                encoded = "\\|";
                break;
        }

        return encoded is not null;
    }

    public override string Unescape(SourceTextSpan value)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '\\' && i + 1 < value.Length && TryUnescape(value[i + 1], sb))
            {
                // Read the escaped sequence into the buffer.
                i++;
            }
            else
            {
                // Copy the value literally.
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static bool TryUnescape(char c, StringBuilder sb)
    {
        switch (c)
        {
            case '\\':
                sb.Append('\\');
                return true;

            case 'n':
                sb.Append('\n');
                return true;

            case '|':
                sb.Append('|');
                return true;

            default:
                // If it's not a recognized escape sequence, we don't unescape it.
                return false;
        }
    }
}
