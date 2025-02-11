using Gherkin;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    class DescriptionRuleHandler() : RuleHandler(RuleType.Description)
    {
        private readonly ImmutableArray<RawNode>.Builder _description = ImmutableArray.CreateBuilder<RawNode>();

        protected override void AppendOther(Token token, TextLine line, Context context)
        {
            // In the context of parsing a feature, "other" tokens are the plain-text lines that form the descriptive text
            // that follows the feature declaration and preceeds any rules or scenarios.
            // There can be multiple of these and they can be separated by all kinds of trivia, including comments.

            // Description lines have the following layout:
            //
            // [text-literal] [end-of-line]

            RawNode? trailing;

            // Blank lines will be matched as "other" in this context. These lines should be included as trivia, rather than tokens.
            if (token.MatchedText.Length == 0)
            {
                // The line is completely empty.
                trailing = line.GetEndOfLineTrivia();
                context.AddLeadingTrivia(trailing);
                return;
            }
            else if (token.MatchedText.All(char.IsWhiteSpace))
            {
                // The whole line is whitespace characters.
                throw new NotImplementedException();
            }

            // For "other" tokens, the parser's `MatchedText` includes all the whitespace on the source line.
            // We need to trim the start and end of the string to yield the correct output.
            var text = token.MatchedText.AsSpan(token.Line.Indent).TrimEnd().ToString();

            trailing = context.SourceText.ConsumeWhitespace(line.Start + token.Line.Indent + text.Length, line.End) +
                line.GetEndOfLineTrivia();

            _description.Add(Literal(context.ConsumeLeadingTrivia(), text, trailing));
        }

        public DescriptionSyntax? CreateDescriptionSyntax()
        {
            var text = RawNode.CreateList(_description);

            if (text == null)
            {
                return null;
            }

            return Description(text);
        }
    }
}
