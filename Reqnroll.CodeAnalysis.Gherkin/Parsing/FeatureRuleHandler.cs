using Gherkin;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class FeatureRuleHandler() : BaseRuleHandler(RuleType.Feature)
{
    private TagsRuleHandler? _tags;
    private FeatureHeaderRuleHandler? _featureHeader;
    private BackgroundRuleHandler? _background;
    private readonly List<ISyntaxBuilder> _members = new();
    private readonly List<ISyntaxBuilder> _rules = new();

    public FeatureSyntax.Internal? CreateFeatureDeclarationSyntax()
    {
        if (_featureHeader == null)
        {
            return null;
        }

        var members = _members
            .Select(member => member.CreateSyntax()!)
            .Where(syntax => syntax != null)
            .ToList();

        var rules = _rules
            .Select(rule => rule.CreateSyntax()!)
            .Where(syntax => syntax != null)
            .ToList();

        return Feature(
            _tags?.Tags,
            _featureHeader.Keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
            _featureHeader.Colon ?? MissingToken(SyntaxKind.ColonToken),
            _featureHeader.Name,
            _featureHeader.Description,
            _background?.CreateBackgroundSyntax(),
            members.Count == 0 ? null : InternalSyntaxList.Create(members),
            rules.Count == 0 ? null : InternalSyntaxList.Create(rules));
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.Tags:
                CodeAnalysisDebug.Assert(_tags == null, "Duplicate tags from parser.");
                return _tags = new TagsRuleHandler();

            case RuleType.FeatureHeader:
                CodeAnalysisDebug.Assert(_featureHeader == null, "Duplicate feature header from parser.");
                return _featureHeader = new FeatureHeaderRuleHandler();

            case RuleType.Background:
                CodeAnalysisDebug.Assert(_background == null, "Duplicate background from parser.");
                return _background = new BackgroundRuleHandler();

            case RuleType.Rule:
                var ruleHandler = new RuleRuleHandler();
                _rules.Add(ruleHandler);
                return ruleHandler;

            case RuleType.ScenarioDefinition:
                var scenarioHandler = new ScenarioDefinitionRuleHandler();
                _members.Add(scenarioHandler);
                return scenarioHandler;
        }

        return base.StartChildRule(ruleType);
    }
}
