using Gherkin;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class FeatureRuleHandler() : ParsingRuleHandler(RuleType.Feature)
{
    private FeatureHeaderRuleHandler? _featureHeaderRuleHandler;
    private TagsRuleHandler? _featureTagsRuleHandler;

    public FeatureSyntax.Internal? CreateFeatureDeclarationSyntax()
    {
        if (_featureHeaderRuleHandler == null)
        {
            return null;
        }

        return Feature(
            _featureTagsRuleHandler?.Tags,
            _featureHeaderRuleHandler.Keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
            _featureHeaderRuleHandler.Colon ?? MissingToken(SyntaxKind.ColonToken),
            _featureHeaderRuleHandler.Name ?? LiteralText(MissingToken(SyntaxKind.LiteralToken)),
            null, //_featureHeaderRuleHandler.Description,
            null,
            null,
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

            default:
                return base.StartChildRule(ruleType);
        }
    }
}
