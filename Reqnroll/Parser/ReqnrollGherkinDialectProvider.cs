using Gherkin;
using Gherkin.Ast;

namespace Reqnroll.Parser
{
    public class ReqnrollGherkinDialectProvider : GherkinDialectProvider
    {
        public ReqnrollGherkinDialectProvider(string defaultLanguage) : base(defaultLanguage)
        {
        }

        public override GherkinDialect GetDialect(string language, Location? location)
        {
            if (language.Contains("-"))
            {
                // Use TryGetDialect to avoid NoSuchLanguageException, if culture specific language is not present
                if (TryGetDialect(language, location, out var dialect))
                    return dialect;
                
                var languageBase = language.Split('-')[0];
                var languageBaseDialect = base.GetDialect(languageBase, location);
                return new GherkinDialect(language, languageBaseDialect.FeatureKeywords, languageBaseDialect.RuleKeywords, languageBaseDialect.BackgroundKeywords, languageBaseDialect.ScenarioKeywords, languageBaseDialect.ScenarioOutlineKeywords, languageBaseDialect.ExamplesKeywords, languageBaseDialect.GivenStepKeywords, languageBaseDialect.WhenStepKeywords, languageBaseDialect.ThenStepKeywords, languageBaseDialect.AndStepKeywords, languageBaseDialect.ButStepKeywords);
            }

            return base.GetDialect(language, location);
        }
    }
}