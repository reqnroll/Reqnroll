using Reqnroll.CodeAnalysis.Gherkin.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin;

internal class DefaultLiteralEscapingStyle : LiteralEscapingStyle
{
    private static readonly HashSet<char> SpecialEscapedCharacters =
        ['\\', '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v'];

    protected override bool TryEncode(char c, [NotNullWhen(true)] out string? encoded)
    {
        encoded = null;
        switch (c)
        {
            case '\\':
                encoded = "\\\\";
                break;
            case '\0':
                encoded = "\\0";
                break;
            case '\a':
                encoded = "\\a";
                break;
            case '\b':
                encoded = "\\b";
                break;
            case '\f':
                encoded = "\\f";
                break;
            case '\n':
                encoded = "\\n";
                break;
            case '\r':
                encoded = "\\r";
                break;
            case '\t':
                encoded = "\\t";
                break;
            case '\v':
                encoded = "\\v";
                break;
        }

        if (encoded is not null)
        {
            return true;
        }

        if (RequiresEscaping(CharUnicodeInfo.GetUnicodeCategory(c)))
        {
            encoded = $"\\u{(int)c:X4}";
            return true;
        }

        return false;
    }

    protected override bool RequiresEscaping(char c) =>
        SpecialEscapedCharacters.Contains(c) || RequiresEscaping(CharUnicodeInfo.GetUnicodeCategory(c));

    protected static bool RequiresEscaping(UnicodeCategory category)
    {
        return category switch
        {
            UnicodeCategory.Control or
            UnicodeCategory.OtherNotAssigned or
            UnicodeCategory.ParagraphSeparator or
            UnicodeCategory.LineSeparator or
            UnicodeCategory.Surrogate => true,
            _ => false,
        };
    }

    public override string Unescape(SourceTextSpan value)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '\\' && i + 1 < value.Length && TryUnescapeSequence(value.Slice(i + 1), sb, out var consumed))
            {
                // Read the escaped sequence into the buffer.
                i += consumed;
            }
            else
            {
                // Copy the value literally.
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    protected virtual bool TryUnescapeSequence(SourceTextSpan sourceTextSpan, StringBuilder stringBuilder, out int consumed)
    {
        // Check the character after the escape character to determine how to decode.
        var first = sourceTextSpan[0];

        switch (first)
        {
            case '0':
                stringBuilder.Append('\0');
                consumed = 1;
                return true;

            case 'a':
                stringBuilder.Append('\a');
                consumed = 1;
                return true;

            case 'b':
                stringBuilder.Append('\b');
                consumed = 1;
                return true;

            case 'f':
                stringBuilder.Append('\f');
                consumed = 1;
                return true;

            case 'n':
                stringBuilder.Append('\n');
                consumed = 1;
                return true;

            case 'r':
                stringBuilder.Append('\r');
                consumed = 1;
                return true;

            case 't':
                stringBuilder.Append('\t');
                consumed = 1;
                return true;

            case 'v':
                stringBuilder.Append('\v');
                consumed = 1;
                return true;

            case '\\':
                stringBuilder.Append('\\');
                consumed = 1;
                return true;

            case 'u':
                
                // Attempt to decode a Unocde hex value.
                if (sourceTextSpan.Length < 5 || 
                    !ushort.TryParse(
                        sourceTextSpan.Substring(2, 4),
                        NumberStyles.HexNumber,
                        CultureInfo.InvariantCulture,
                        out var code))
                {
                    // Not a valid Unicode escape sequence.
                    stringBuilder.Append('\\');
                    stringBuilder.Append(first);
                    consumed = 1;
                    return true;
                }

                stringBuilder.Append((char)code);
                consumed = 1;
                return true;

            default:
                consumed = 0;
                return false;
        }
    }
}
