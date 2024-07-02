using Gherkin;

namespace Reqnroll.FeatureSourceGenerator.SourceModel;

public record ScenarioStep(StepKeywordType KeywordType, string Keyword, string Text, int LineNumber);
