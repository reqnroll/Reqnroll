using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
}
