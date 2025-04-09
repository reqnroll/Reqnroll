using Gherkin;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    class GherkinDocumentRuleHandler() : RuleHandler(RuleType.GherkinDocument)
    {
        private FeatureRuleHandler? _featureRuleHandler;

        private InternalNode? _endOfFile;

        public GherkinDocumentSyntax.Internal BuildFeatureFileSyntax()
        {
            return new GherkinDocumentSyntax.Internal(
                _featureRuleHandler?.CreateFeatureDeclarationSyntax(),
                _endOfFile ?? MissingToken(SyntaxKind.EndOfFileToken));
        }

        public override RuleHandler StartChildRule(RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.Feature:
                    Debug.Assert(_featureRuleHandler == null, "Duplicate feature rule from parser.");
                    return _featureRuleHandler = new FeatureRuleHandler();
            }

            return base.StartChildRule(ruleType);
        }

        protected override void AppendEndOfFile(Context context)
        {
            // End of file tokens are zero-width and can have no trailing trivia.
            // Any whitespace on the line or blank lines preceeding the end of file are associated with the token.
            _endOfFile = Token(context.ConsumeLeadingTrivia(), SyntaxKind.EndOfFileToken, null);
        }
    }
}
