using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class FeatureRuleHandler() : BaseRuleHandler(RuleType.Feature)
{
    private TagsRuleHandler? _featureTagsRuleHandler;
    private FeatureHeaderRuleHandler? _featureHeaderRuleHandler;
    private BackgroundRuleHandler? _backgroundRuleHandler;
    private readonly List<ISyntaxBuilder> _memberHandlers = new();

    public FeatureSyntax.Internal? CreateFeatureDeclarationSyntax()
    {
        if (_featureHeaderRuleHandler == null)
        {
            return null;
        }

        var members = _memberHandlers
            .Select(handler => handler.CreateSyntax()!)
            .Where(syntax => syntax != null)
            .ToList();

        return Feature(
            _featureTagsRuleHandler?.Tags,
            _featureHeaderRuleHandler.Keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
            _featureHeaderRuleHandler.Colon ?? MissingToken(SyntaxKind.ColonToken),
            _featureHeaderRuleHandler.Name ?? LiteralText(MissingToken(SyntaxKind.LiteralToken)),
            _featureHeaderRuleHandler.Description,
            _backgroundRuleHandler?.CreateBackgroundSyntax(),
            members.Count == 0 ? null : InternalSyntaxList.Create(members),
            null);
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.Tags:
                CodeAnalysisDebug.Assert(_featureTagsRuleHandler == null, "Duplicate tags from parser.");
                return _featureTagsRuleHandler = new TagsRuleHandler();

            case RuleType.FeatureHeader:
                CodeAnalysisDebug.Assert(_featureHeaderRuleHandler == null, "Duplicate feature header from parser.");
                return _featureHeaderRuleHandler = new FeatureHeaderRuleHandler();

            case RuleType.Background:
                CodeAnalysisDebug.Assert(_backgroundRuleHandler == null, "Duplicate background from parser.");
                return _backgroundRuleHandler = new BackgroundRuleHandler();

            case RuleType.ScenarioDefinition:
                var scenarioHandler = new ScenarioDefinitionRuleHandler();
                _memberHandlers.Add(scenarioHandler);
                return scenarioHandler;
        }

        return base.StartChildRule(ruleType);
    }
}
