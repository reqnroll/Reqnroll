using Gherkin;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class ScenarioDefinitionRuleHandler() : 
    BaseRuleHandler(RuleType.ScenarioDefinition), ISyntaxBuilder
{
    private TagsRuleHandler? _tagsRuleHandler;

    private ScenarioRuleHandler? _scenarioRuleHandler;

    public ExampleSyntax.Internal? CreateExampleDeclarationSyntax()
    {
        return _scenarioRuleHandler?.CreateSyntax();
    }

    InternalNode? ISyntaxBuilder.CreateSyntax() => CreateExampleDeclarationSyntax();

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.Scenario:
                CodeAnalysisDebug.Assert(_scenarioRuleHandler == null, "Duplicate scenario from parser.");
                return _scenarioRuleHandler = new();

            case RuleType.Tags:
                CodeAnalysisDebug.Assert(_tagsRuleHandler == null, "Duplicate tags from parser.");
                return _tagsRuleHandler = new();
        }

        return base.StartChildRule(ruleType);
    }
}
