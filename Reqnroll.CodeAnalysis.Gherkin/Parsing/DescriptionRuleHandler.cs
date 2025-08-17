using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal class DescriptionRuleHandler() : BaseRuleHandler(RuleType.Description)
{
    private readonly PlainTextParser _plainTextParser = new(LiteralEscapingStyle.Default);

    protected override void AppendOther(Token token, TextLine line, ParsingContext context)
    {
        // In the context of parsing a feature, "other" tokens are the plain-text lines that form the descriptive text
        // that follows the feature declaration and preceeds any rules or scenarios.
        // There can be multiple of these and they can be separated by all kinds of trivia, including comments.

        // Description lines have the following layout:
        //
        // [description-literal] [end-of-line]

        // Blank lines will be matched as "other" in this context.
        // If these lines occur between text lines, they are treated as part of the text.
        // If they occur at the start of the description, they are treated as leading trivia.
        // If they occur at the end of the description, they are leading trivia for the next element (standard rules).
        if (token.MatchedText.Length == 0)
        {
            // The line is completely empty.
            var trivia = line.GetEndOfLineTrivia();
            context.AddLeadingTrivia(trivia);
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

        var leading = context.ConsumeLeadingTriviaAndWhitespace(line, token);
        var trailing = context.SourceText.ConsumeWhitespace(line.Start + token.Line.Indent + text.Length, line.End) +
            line.GetEndOfLineTrivia();

        _plainTextParser.AppendText(leading, text, trailing);
    }

    public PlainTextSyntax.Internal? CreateDescriptionSyntax() => _plainTextParser.ParseText();
}
