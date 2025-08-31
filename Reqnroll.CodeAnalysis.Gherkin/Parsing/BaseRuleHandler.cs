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

    private static readonly string[] TriviaTokenTypes =
        [
            "#Language",
            "#Comment",
            "#Empty",
            "#EOF"
        ];

    private static readonly HashSet<string> FeatureTagTokenTypes = ["#FeatureLine", "#TagLine"];

    protected override void AppendComment(Token token, TextLine line, ParsingContext context)
    {
        // Comments are always added to leading trivia, but they can be surrounded by whitespace.
        var leading = context.SourceText.ConsumeWhitespace(line.Start, line.End - 1);
        var trailing = context.SourceText.ReverseConsumeWhitespace(line.End - 1, line.Start);

        var commentStart = line.Start + (leading?.Width ?? 0);
        var commentEnd = line.End - (trailing?.Width ?? 0);

        var span = TextSpan.FromBounds(commentStart, commentEnd);
        var comment = Comment(line.Text!, span);

        context.AddLeadingTrivia(leading);
        context.AddLeadingTrivia(comment);
        context.AddLeadingTrivia(trailing);

        // Include the end-of-line trivia (if present).
        var endOfLineTrivia = line.GetEndOfLineTrivia();
        if (endOfLineTrivia != null)
        {
            context.AddLeadingTrivia(endOfLineTrivia);
        }
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
        var name = Literal(
            null,
            SyntaxKind.NameToken,
            LiteralEscapingStyle.Default.Escape(token.MatchedText),
            token.MatchedText,
            nameWhitespace + line.GetEndOfLineTrivia());

        // TODO: Refactor this to generalise the process of reading tokens and trivia from a line.

        context.AddLeadingTrivia(
            DirectiveCommentTrivia(
                hash,
                keyword,
                colon,
                name));
    }

    public override void AppendUnexpectedToken(
        Token token,
        TextLine line,
        UnexpectedTokenException exception,
        ParsingContext context)
    {
        // The default implementation handles unexpected tokens by skipping them - they're added to the
        // trivia of the next token as a literal. Other handlers may override this to handle other cases.
        var contentSpan = TextSpan.FromBounds(line.Start + token.MatchedIndent, line.End);

        InternalNode? leadingTrivia = Whitespace(context.SourceText, line.Start, token.MatchedIndent);
        var leadingWhitespace = context.SourceText.ConsumeWhitespace(contentSpan);

        leadingTrivia += leadingWhitespace;

        var trailingWhitespace = context.SourceText.ReverseConsumeWhitespace(contentSpan);
        InternalNode? trailingTrivia = trailingWhitespace + line.GetEndOfLineTrivia();

        // All text remaining between any whitespace we'll treat as a literal.
        var literalSpan = TextSpan.FromBounds(
            contentSpan.Start + (leadingWhitespace?.Width ?? 0),
            contentSpan.End - (trailingWhitespace?.Width ?? 0));

        var text = context.SourceText.ToString(literalSpan);

        var literal = Literal(
            leadingTrivia,
            SyntaxKind.NameToken,
            LiteralEscapingStyle.Default.Escape(text),
            text,
            trailingTrivia);

        // Add the skipped tokens to be included as leading trivia in the next token.
        // Inkeeping with the CS compiler error codes, we'll create a unique error for each combination of expected tokens.
        var diagnostic = GetUnexpectedTokenDiagnostic(exception.ExpectedTokenTypes);

        context.AddSkippedToken(literal, diagnostic);
    }

    private static InternalDiagnostic GetUnexpectedTokenDiagnostic(string[] expectedTokenTypes)
    {
        var expectedTokenSet = expectedTokenTypes.Except(TriviaTokenTypes).ToArray();

        if (FeatureTagTokenTypes.SetEquals(expectedTokenSet))
        {
            return InternalDiagnostic.Create(DiagnosticDescriptors.ErrorExpectedFeatureOrTag);
        }

        // TODO: Add exception message from resources.
        throw new NotImplementedException(
            "No error-code exists to handle when an unexpected token was encountered whilst expecting " +
            $"tokens {string.Join(", ", expectedTokenSet)}");
    }
}
