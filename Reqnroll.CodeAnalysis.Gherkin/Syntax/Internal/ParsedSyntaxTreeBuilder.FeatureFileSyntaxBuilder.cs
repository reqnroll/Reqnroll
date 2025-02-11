using Gherkin;
using System.Diagnostics;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    class FeatureFileSyntaxBuilder : SyntaxBuilder<FeatureFileSyntax>
    {
        private bool _fileHasStarted = false;
        private bool _fileHasEnded = false;

        private FeatureDeclarationBuilder? _featureDeclarationBuilder;

        private RawNode? _endOfFile;

        public override ISyntaxBuilder StartRule(RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.GherkinDocument:
                    Debug.Assert(!_fileHasStarted, "Recieved second document start.");
                    _fileHasStarted = true;
                    return this;

                case RuleType.Feature:
                    Debug.Assert(_fileHasStarted, "Recieved feature before document start.");
                    Debug.Assert(_featureDeclarationBuilder == null, "Recieved second feature start.");
                    return _featureDeclarationBuilder = new(this);

                default:
                    throw new NotSupportedException();
            }
        }

        public override ISyntaxBuilder EndRule(RuleType ruleType)
        {
            switch (ruleType)
            {
                case RuleType.GherkinDocument:
                    Debug.Assert(!_fileHasEnded, "Recieved second document end.");
                    _fileHasEnded = true;
                    return this;

                default:
                    throw new NotSupportedException();
            }
        }

        public override FeatureFileSyntax? Build()
        {
            return new FeatureFileSyntax(
                _featureDeclarationBuilder?.Build(),
                _endOfFile ?? MissingToken(SyntaxKind.EndOfFileToken));
        }

        public override void AppendEndOfFile(Context context)
        {
            // End of file tokens are zero-width and can have no trailing trivia.
            // Any whitespace on the line or blank lines preceeding the end of file are associated with the token.
            _endOfFile = Token(context.ConsumeLeadingTrivia(), SyntaxKind.EndOfFileToken, null);
        }
    }
}
