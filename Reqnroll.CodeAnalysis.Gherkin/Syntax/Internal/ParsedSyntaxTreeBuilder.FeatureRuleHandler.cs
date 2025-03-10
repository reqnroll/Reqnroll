using Gherkin;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    class FeatureRuleHandler() : RuleHandler(RuleType.Feature)
    {
        private FeatureHeaderRuleHandler? _featureHeaderRuleHandler;

        public FeatureDeclarationSyntax? CreateFeatureDeclarationSyntax()
        {
            if (_featureHeaderRuleHandler == null)
            {
                return null;
            }

            return FeatureDeclaration(
                _featureHeaderRuleHandler.keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
                _featureHeaderRuleHandler.colon ?? MissingToken(SyntaxKind.ColonToken),
                _featureHeaderRuleHandler.name ?? MissingToken(SyntaxKind.IdentifierToken),
                _featureHeaderRuleHandler.CreateDescriptionSyntax());
        }

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            if (ruleType == RuleType.FeatureHeader)
            {
                Debug.Assert(_featureHeaderRuleHandler == null, "Duplicate feature header from parser.");
                return _featureHeaderRuleHandler = new FeatureHeaderRuleHandler();
            }

            return base.StartChildRule(ruleType);
        }
    }
}
