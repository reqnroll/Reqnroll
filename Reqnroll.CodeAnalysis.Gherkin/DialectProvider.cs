using Gherkin;
using Gherkin.Ast;

namespace Reqnroll.CodeAnalysis.Gherkin;

class DialectProvider(string defaultLanguage) : GherkinDialectProvider(defaultLanguage)
{
    public override GherkinDialect GetDialect(string language, Location location)
    {
        try
        {
            return base.GetDialect(language, location);
        }
        catch (NoSuchLanguageException)
        {
            var index = language.LastIndexOf('-');

            if (index < 0)
            {
                throw;
            }

            var fallback = language.Substring(0, index);

            return GetFallbackDialect(fallback, language, location);
        }
    }

    private GherkinDialect GetFallbackDialect(string fallback, string language, Location location)
    {
        var dialect = GetDialect(fallback, location);

        return new GherkinDialect(
            language,
            dialect.FeatureKeywords,
            dialect.RuleKeywords,
            dialect.BackgroundKeywords,
            dialect.ScenarioKeywords,
            dialect.ScenarioOutlineKeywords,
            dialect.ExamplesKeywords,
            dialect.GivenStepKeywords,
            dialect.WhenStepKeywords,
            dialect.ThenStepKeywords,
            dialect.AndStepKeywords,
            dialect.ButStepKeywords);
    }
}
