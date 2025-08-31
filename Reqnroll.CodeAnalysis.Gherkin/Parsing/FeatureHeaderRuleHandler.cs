using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal class FeatureHeaderRuleHandler() : BaseRuleHandler(RuleType.FeatureHeader)
{
    private DescriptionRuleHandler? _descriptionRuleHandler;

    private TagsRuleHandler? _tagsRuleHandler;

    private readonly DeclarationHelper _declarationHelper = new(SyntaxKind.FeatureKeyword, true);

    public InternalNode? Keyword => _declarationHelper.Keyword;

    public InternalNode? Colon => _declarationHelper.Colon;

    public InternalNode? Name => _declarationHelper.Name;

    public DescriptionSyntax.Internal? Description => _descriptionRuleHandler?.CreateDescriptionSyntax();

    public InternalNode? Tags => _tagsRuleHandler?.Tags;

    protected override void AppendFeatureLine(Token token, TextLine line, ParsingContext context)
    {
        CodeAnalysisDebug.Assert(_declarationHelper.Keyword == null, "Duplicate feature line from parser.");

        _declarationHelper.DeconstructDeclarationToken(token, line, context);
    }

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        switch (ruleType)
        {
            case RuleType.Tags:
                CodeAnalysisDebug.Assert(_tagsRuleHandler == null, "Duplicate tags from parser.");
                return _tagsRuleHandler = new();

            case RuleType.Description:
                CodeAnalysisDebug.Assert(_descriptionRuleHandler == null, "Duplicate description from parser.");
                return _descriptionRuleHandler = new();
        }

        return base.StartChildRule(ruleType);
    }
}
