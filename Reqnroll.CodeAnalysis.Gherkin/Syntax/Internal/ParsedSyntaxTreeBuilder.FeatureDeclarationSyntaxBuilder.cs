using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using Gherkin;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

using static InternalSyntaxFactory;

internal partial class ParsedSyntaxTreeBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureDeclarationBuilder"/> class.
    /// </summary>
    /// <param name="parent">The parent syntax builder.</param>
    class FeatureDeclarationBuilder(FeatureFileSyntaxBuilder parent) : SyntaxBuilder<FeatureDeclarationSyntax>
    {
        private RawNode? _keyword;
        private RawNode? _colon;
        private RawNode? _name;
        private readonly ImmutableArray<RawNode>.Builder _description = ImmutableArray.CreateBuilder<RawNode>();

        public override ISyntaxBuilder StartRule(RuleType ruleType)
        {
            throw new NotImplementedException();
        }

        public override ISyntaxBuilder EndRule(RuleType ruleType)
        {
            switch (ruleType)
            {
                // Once the feature is complete, return control back to the parent.
                case RuleType.Feature:
                    return parent;

                default:
                    throw new NotSupportedException();
            }
        }

        public override FeatureDeclarationSyntax? Build()
        {
            // If none of the feature declaration syntax are present, we should not include the node.
            if (_keyword == null && _colon == null && _name == null)
            {
                return null;
            }
            else
            {
                var descriptionText = RawNode.CreateList(_description);

                return new FeatureDeclarationSyntax(
                    _keyword ?? MissingToken(SyntaxKind.FeatureKeyword),
                    _colon ?? MissingToken(SyntaxKind.ColonToken),
                    _name ?? MissingToken(SyntaxKind.IdentifierToken),
                    descriptionText == null ? null : new DescriptionSyntax(descriptionText));
            }
        }

        protected override void AppendFeatureLine(Token token, TextLine line, Context context)
        {
            // Convert the line into tokens such that all characters are consumed.
            // Feature lines have the following layout:
            //
            // [keyword][colon] [name] [end-of-line]
            //
            // Leading whitespace characters are tracked by the Gherkin parser.
            // The parser also provides the keyword text (without the trailing colon) and position, and the name text.

            // Extract the whitespace between the colon and feature name.
            // Should just be a space, but we can read to be sure.
            var colonPosition = line.Start + token.Line.Indent + token.MatchedKeyword.Length;
            var colonWhitespace = context.SourceText.ConsumeWhitespace(colonPosition + 1, line.End);

            _keyword = Token(context.ConsumeLeadingTrivia(), SyntaxKind.FeatureKeyword, token.MatchedKeyword, null);
            _colon = Token(null, SyntaxKind.ColonToken, colonWhitespace);

            // Extract any whitespace between the end of the feature name and the end of the line.
            RawNode? nameWhitespace = context.SourceText.ConsumeWhitespace(
                colonPosition + (colonWhitespace?.Width ?? 0) + token.MatchedText.Length,
                line.End);

            AppendEndOfLineIfPresent(ref nameWhitespace, line);

            _name = Identifier(null, token.MatchedText, nameWhitespace);
        }

        private void AppendEndOfLineIfPresent(ref RawNode? trivia, TextLine line)
        {
            if (line.End != line.EndIncludingLineBreak)
            {
                trivia += EndOfLine(line.Text!, TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
            }
        }

        protected override void AppendOther(Token token, TextLine line, Context context)
        {
            // In the context of parsing a feature, "other" tokens are the plain-text lines that form the descriptive text
            // that follows the feature declaration and preceeds any rules or scenarios.
            // There can be multiple of these and they can be separated by all kinds of trivia, including comments.

            // Description lines have the following layout:
            //
            // [text-literal] [end-of-line]

            RawNode? trailing = null;

            // Blank lines will be matched as "other" in this context. These lines should be included as trivia, rather than tokens.
            if (token.MatchedText.Length == 0)
            {
                // The line is completely empty.
                AppendEndOfLineIfPresent(ref trailing, line);
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

            trailing = context.SourceText.ConsumeWhitespace(line.Start + token.Line.Indent + text.Length, line.End);

            AppendEndOfLineIfPresent(ref trailing, line);

            _description.Add(Literal(context.ConsumeLeadingTrivia(), text, trailing));
        }
    }
}
