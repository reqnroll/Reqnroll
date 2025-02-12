using Gherkin;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;
internal partial class ParsedSyntaxTreeBuilder
{
    class RootHandler() : RuleHandler(RuleType.None)
    {
        private GherkinDocumentRuleHandler? _gherkinDocumentHandler;

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            if (ruleType == RuleType.GherkinDocument)
            {
                Debug.Assert(_gherkinDocumentHandler == null, "Duplicate document rule from parser.");
                return _gherkinDocumentHandler = new GherkinDocumentRuleHandler();
            }

            throw new NotSupportedException($"{nameof(RootHandler)} does not support having child rules of type {ruleType}");
        }

        public SyntaxNode BuildFeatureFileSyntax()
        {
            Debug.Assert(_gherkinDocumentHandler != null, "No nodes recieved from parser.");
            return _gherkinDocumentHandler!.BuildFeatureFileSyntax().CreateSyntaxNode();
        }
    }
}
