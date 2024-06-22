using System.Text.RegularExpressions;

namespace Reqnroll.Bindings.CucumberExpressions;

public class CucumberExpressionDetector : ICucumberExpressionDetector
{
    private static readonly Regex ParameterPlaceholder = new(@"{\w*}");
    private static readonly Regex CommonRegexStepDefPatterns = new(@"(\([^\)]+[\*\+]\)|\.\*)");
    private static readonly Regex ExtendedRegexStepDefPatterns = new(@"(\\\.|\\d\+)"); // \. \d+

    public virtual bool IsCucumberExpression(string cucumberExpressionCandidate)
    {
        if (cucumberExpressionCandidate.StartsWith("^") || cucumberExpressionCandidate.EndsWith("$"))
            return false;

        if (ParameterPlaceholder.IsMatch(cucumberExpressionCandidate))
            return true;

        if (CommonRegexStepDefPatterns.IsMatch(cucumberExpressionCandidate))
            return false;

        // These are special constructs that usually happen in regex, but not valid
        // in Cucumber Expressions => If they exist, we treat the expression as regex.
        // - \d+
        // - \.
        if (ExtendedRegexStepDefPatterns.IsMatch(cucumberExpressionCandidate))
            return false;

        return true;
    }
}
