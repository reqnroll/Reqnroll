using Gherkin;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal class RootHandler() : ParsingRuleHandler(RuleType.None)
{
    private GherkinDocumentRuleHandler? _gherkinDocumentHandler;

    public override ParsingRuleHandler StartChildRule(RuleType ruleType)
    {
        if (ruleType == RuleType.GherkinDocument)
        {
            CodeAnalysisDebug.Assert(_gherkinDocumentHandler == null, "Duplicate document rule from parser.");
            return _gherkinDocumentHandler = new GherkinDocumentRuleHandler();
        }

        return base.StartChildRule(ruleType);
    }

    public SyntaxNode BuildFeatureFileSyntax()
    {
        CodeAnalysisDebug.Assert(_gherkinDocumentHandler != null, "No nodes recieved from parser.");
        return _gherkinDocumentHandler!.BuildFeatureFileSyntax().CreateSyntaxNode();
    }
}
