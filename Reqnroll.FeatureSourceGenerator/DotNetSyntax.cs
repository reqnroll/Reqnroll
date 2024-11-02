using System.Globalization;
using System.Text;

namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// Provides methods to support generating syntax following common .NET rules and conventions.
/// </summary>
internal static class DotNetSyntax
{
    /// <summary>
    /// Creates an identifier which follows common .NET naming rules and conventions.
    /// </summary>
    /// <param name="s">The input string to use as the basis of the identifier.</param>
    /// <returns>A string which is safe to use as an identifier across .NET languages.</returns>
    public static string CreateIdentifier(string s)
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
                sb.Append(char.ToUpper(c));
                newWord = false;
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
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
