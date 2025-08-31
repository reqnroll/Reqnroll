using Gherkin;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal class ExamplesDefinitionRuleHandler() : BaseRuleHandler(RuleType.ExamplesDefinition)
{
    private readonly List<ExamplesRuleHandler> _examples = [];

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.Examples:
                var examplesHandler = new ExamplesRuleHandler();
                _examples.Add(examplesHandler);
                return examplesHandler;

            default:
                return base.StartChildRule(ruleType);
        }
    }
}

internal class ExamplesRuleHandler() : BaseRuleHandler(RuleType.Examples)
{
}
