using System.Globalization;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class CSharpSyntaxHelper
{
    public static bool IsValidInIdentifier(char c)
    {
        return char.GetUnicodeCategory(c) switch
        {
            UnicodeCategory.UppercaseLetter => true,        // Lu
            UnicodeCategory.LowercaseLetter => true,        // Ll
            UnicodeCategory.TitlecaseLetter => true,        // Lt
            UnicodeCategory.ModifierLetter => true,         // Lm
            UnicodeCategory.OtherLetter => true,            // Lo
            UnicodeCategory.LetterNumber => true,           // Nl

            UnicodeCategory.DecimalDigitNumber => true,     // Nd

            UnicodeCategory.ConnectorPunctuation => true,   // Pc (includes '_')

            UnicodeCategory.NonSpacingMark => true,         // Mn
            UnicodeCategory.SpacingCombiningMark => true,   // Mc

            UnicodeCategory.Format => true,                 // Cf

            _ => false
        };
    }
}
