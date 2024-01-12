using Gherkin.Ast;
using Reqnroll.Parser;

namespace Reqnroll.Generator.Generation;

public class ScenarioDefinitionInFeatureFile
{
    public StepsContainer ScenarioDefinition { get; }
    public Rule Rule { get; }
    public ReqnrollFeature Feature { get; }

    public ScenarioOutline ScenarioOutline => ScenarioDefinition as ScenarioOutline;
    public Scenario Scenario => ScenarioDefinition as Scenario;

    public bool IsScenarioOutline => ScenarioOutline != null;

    public ScenarioDefinitionInFeatureFile(StepsContainer stepsContainer, ReqnrollFeature feature, Rule rule = null)
    {
        ScenarioDefinition = stepsContainer;
        Feature = feature;
        Rule = rule;
    }
}
