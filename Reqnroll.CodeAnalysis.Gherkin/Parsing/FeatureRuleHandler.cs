using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class FeatureRuleHandler() : BaseRuleHandler(RuleType.Feature)
{
    private TagsRuleHandler? _featureTagsRuleHandler;
    private FeatureHeaderRuleHandler? _featureHeaderRuleHandler;

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
            _featureHeaderRuleHandler.Description,
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

    protected override void AppendScenarioLine(Token token, TextLine line, ParsingContext context)
    {
        // Scenario lines have the following layout:
        //
        // [keyword][colon] [name] [end-of-line]
        //
        // Leading whitespace characters are tracked by the Gherkin parser.
        // The parser also provides the keyword text (without the trailing colon) and position, and the name text.
    }

    protected override void AppendStepLine(Token token, TextLine line, ParsingContext context)
    {
    }
}
