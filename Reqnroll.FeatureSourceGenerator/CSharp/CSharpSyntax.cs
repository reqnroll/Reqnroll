using System.Globalization;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator.CSharp;

internal static class CSharpSyntax
{
    public static readonly Dictionary<Type, string> TypeAliases = new()
    {
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(object), "object" },
        { typeof(bool), "bool" },
        { typeof(char), "char" },
        { typeof(string), "string" },
        { typeof(void), "void" }
    };

    public static IdentifierString GenerateTypeIdentifier(string s) => CreateIdentifier(s, capitalizeFirstWord: true);

    public static string CreateMethodIdentifier(string s) => CreateIdentifier(s, capitalizeFirstWord: true);

    public static IdentifierString GenerateParameterIdentifier(string s) => CreateIdentifier(s, capitalizeFirstWord: false);

    private static IdentifierString CreateIdentifier(string s, bool capitalizeFirstWord)
    {
        var sb = new StringBuilder();
        var newWord = true;

        foreach (char c in s)
        {
            if (char.IsWhiteSpace(c))
            {
                newWord = true;
                continue;
            }

            if (!IsValidInIdentifier(c))
            {
                continue;
            }

            if (sb.Length == 0 && !IsValidAsFirstCharacterInIdentifier(c))
            {
                sb.Append('_');
                sb.Append(c);
                continue;
            }

            if (newWord)
            {
                if (sb.Length == 0 && !capitalizeFirstWord)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(char.ToUpper(c));
                }

                newWord = false;
            }
            else
            {
                sb.Append(c);
            }
        }

        return new IdentifierString(sb.ToString());
    }

    private static bool IsValidAsFirstCharacterInIdentifier(char c)
    {
        if (c == '_')
        {
            return true;
        }

        var category = char.GetUnicodeCategory(c);

        return category == UnicodeCategory.UppercaseLetter
            || category == UnicodeCategory.LowercaseLetter
            || category == UnicodeCategory.TitlecaseLetter
            || category == UnicodeCategory.ModifierLetter
            || category == UnicodeCategory.OtherLetter;
    }

    private static bool IsValidInIdentifier(char c)
    {
        var category = char.GetUnicodeCategory(c);

        return category == UnicodeCategory.UppercaseLetter
            || category == UnicodeCategory.LowercaseLetter
            || category == UnicodeCategory.TitlecaseLetter
            || category == UnicodeCategory.ModifierLetter
            || category == UnicodeCategory.OtherLetter
            || category == UnicodeCategory.LetterNumber
            || category == UnicodeCategory.NonSpacingMark
            || category == UnicodeCategory.SpacingCombiningMark
            || category == UnicodeCategory.DecimalDigitNumber
            || category == UnicodeCategory.ConnectorPunctuation
            || category == UnicodeCategory.Format;
    }
}
