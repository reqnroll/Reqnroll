using Gherkin;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class FeatureRuleHandler() : ParsingRuleHandler(RuleType.Feature)
{
    private FeatureHeaderRuleHandler? _featureHeaderRuleHandler;

    public FeatureSyntax.Internal? CreateFeatureDeclarationSyntax()
    {
        if (_featureHeaderRuleHandler == null)
        {
            return null;
        }

        return Feature(
            _featureHeaderRuleHandler.keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
            _featureHeaderRuleHandler.colon ?? MissingToken(SyntaxKind.ColonToken),
            _featureHeaderRuleHandler.name ?? MissingToken(SyntaxKind.IdentifierToken),
            _featureHeaderRuleHandler.CreateDescriptionSyntax());
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        if (ruleType == RuleType.FeatureHeader)
        {
            Debug.Assert(_featureHeaderRuleHandler == null, "Duplicate feature header from parser.");
            return _featureHeaderRuleHandler = new FeatureHeaderRuleHandler();
        }

        return base.StartChildRule(ruleType);
    }
}
