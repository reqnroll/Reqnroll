using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class BaseRuleHandler : ParsingRuleHandler
{
    protected BaseRuleHandler(RuleType ruleType) : base(ruleType)
    {
    }

    protected override void AppendComment(Token token, TextLine line, ParsingContext context)
    {
        // Comments are always added to leading trivia, but they can be preceeded by whitespace.
        var leading = context.SourceText.ConsumeWhitespace(line.Start, line.Start + token.Line.Indent);

        context.AddLeadingTrivia(leading);
        context.AddLeadingTrivia(Comment(token.MatchedText));
    }

    protected override void AppendLanguage(Token token, TextLine line, ParsingContext context)
    {
        // Language tokens are comments that indicate the language of the Gherkin file.
        // Language lines have the following layout:
        //
        // [hash] [keyword][colon] [name] [end-of-line]
        //
        // Leading whitespace characters are tracked by the Gherkin parser.
        // The parser also provides the keyword text (without the trailing colon) and position, and the name text.

        // Language comments are always added to leading trivia, but they can be preceeded by whitespace.
        var leading = context.SourceText.ConsumeWhitespace(line.Start, line.Start + token.Line.Indent);

        // The first token on the line is always a hash.
        var hashIndex = line.Start + token.Line.Indent;
        var hashWhitespace = context.SourceText.ConsumeWhitespace(hashIndex + 1, line.End);
        var hash = Token(leading, SyntaxKind.HashToken, hashWhitespace);

        // After the hash, the next value is the keyword which is always "language".
        var keywordIndex = hashIndex + 1 + (hashWhitespace?.Width ?? 0);
        var keywordWhitespace = context.SourceText.ConsumeWhitespace(keywordIndex + "language".Length, line.End);
        var keyword = DirectiveIdentifier(null, "language", keywordWhitespace);

        // The next value is the colon token.
        var colonIndex = keywordIndex + "language".Length + (keywordWhitespace?.Width ?? 0);
        var colonWhitespace = context.SourceText.ConsumeWhitespace(colonIndex + 1, line.End);
        var colon = Token(null, SyntaxKind.ColonToken, colonWhitespace);

        // The final value is the language name, which is the rest of the line.
        var nameIndex = colonIndex + 1 + (colonWhitespace?.Width ?? 0);
        var nameWhitespace = context.SourceText.ConsumeWhitespace(nameIndex + token.MatchedText.Length, line.End);
        var name = DirectiveIdentifier(null, token.MatchedText, nameWhitespace + line.GetEndOfLineTrivia());

        // TODO: Refactor this to generalise the process of reading tokens and trivia from a line.

        context.AddLeadingTrivia(
            DirectiveCommentTrivia(
                hash,
                keyword,
                colon,
                name));
    }
}
