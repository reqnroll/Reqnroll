using System.Text.RegularExpressions;

namespace Reqnroll.StepBindingSourceGenerator;

internal static class CucumberExpressionDetector
{
    private static readonly Regex ParameterPlaceholder = new(@"{\w*}");
    private static readonly Regex CommonRegexStepDefPatterns = new(@"(\([^\)]+[\*\+]\)|\.\*)");
    private static readonly Regex ExtendedRegexStepDefPatterns = new(@"(\\\.|\\d\+)"); // \. \d+

    public static bool IsCucumberExpression(string text)
    {
        // If the user specifies regular expression anchors,
        // assume the text is intended as a regular expression.
        if (text.StartsWith("^") || text.EndsWith("$"))
        {
            return false;
        }

        // If the text includes a cucumber-expression-style placeholder,
        // assume the text is intended as a cucumber expression.
        if (ParameterPlaceholder.IsMatch(text))
        {
            return true;
        }

        // If the text contains a commonly-encountered encountered regular expression pattern,
        // assume the text is intended as a regular expression.
        if (CommonRegexStepDefPatterns.IsMatch(text))
        {
            return false;
        }

        // These are special constructs that usually happen in regex, but not valid
        // in Cucumber Expressions => If they exist, we treat the expression as regex.
        // - \d+
        // - \.
        if (ExtendedRegexStepDefPatterns.IsMatch(text))
        {
            return false;
        }

        return true;
    }
}
