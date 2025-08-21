using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

/// <summary>
/// Provides methods for working with syntax elements which are declarations in Gherkin syntax.
/// </summary>
/// <param name="ruleType">The type of rule handled.</param>
/// <param name="keywordKind">The syntax kind of the keyword associated with this declaration.</param>
/// <remarks>
/// <para>Declarations have a common structure that consists of a keyword, followed by a colon and a name.
/// The <see cref="DeclarationHelper"/> provides a <see cref="DeconstructDeclarationToken"/> method which
/// will deconstruct a syntax token and assign the parts to the <see cref="Keyword"/>, <see cref="Colon"/>
/// and <see cref="Name"/> properties.</para>
/// <para>The <see cref="CreateKeywordToken"/> and <see cref="CreateNameToken"/> methods provide the conversion
/// of the raw text values to their token representations, including the associated trivia. These methods can
/// be overriden to provide addtional behaviours, such as adding diagnostic information.</para>
/// </remarks>
internal class DeclarationHelper(SyntaxKind keywordKind) 
{
    public InternalNode? Keyword { get; private set; }

    public InternalNode? Colon { get; private set; }

    public InternalNode? Name { get; private set; }

    public void DeconstructDeclarationToken(Token token, TextLine line, ParsingContext context)
    {
        // Convert the line into tokens such that all characters are consumed.
        // Declaration lines have the following layout:
        //
        // [keyword][colon] [name] [end-of-line]
        //
        // Leading whitespace characters are tracked by the Gherkin parser.
        // The parser also provides the keyword text (without the trailing colon) and position, and the name text.

        // Extract the whitespace between the colon and name - should just be a space, but we can read to be sure.
        var colonPosition = line.Start + token.Line.Indent + token.MatchedKeyword.Length;
        var colonWhitespace = context.SourceText.ConsumeWhitespace(colonPosition + 1, line.End);

        var leading = context.ConsumeLeadingTriviaAndWhitespace(line, token);

        // Obtain any trailing trivia.
        var trailing = line.GetEndOfLineTrivia();

        Keyword = CreateKeywordToken(leading, token.MatchedKeyword, null);

        if (string.IsNullOrEmpty(token.MatchedText))
        {
            Colon = Token(null, SyntaxKind.ColonToken, colonWhitespace + trailing);
            return;
        }

        var nameEndPosition = colonPosition + (colonWhitespace?.Width ?? 0) + token.MatchedText.Length;
        trailing = context.SourceText.ConsumeWhitespace(nameEndPosition, line.End) + trailing;

        Colon = Token(null, SyntaxKind.ColonToken, colonWhitespace);
        Name = LiteralText(
            Literal(
                null,
                LiteralEscapingStyle.Default.Escape(token.MatchedText),
                token.MatchedText,
                trailing));
    }

    protected virtual InternalNode CreateKeywordToken(
        InternalNode? leadingTrivia,
        string value,
        InternalNode? trailingTrivia)
    {
        return string.IsNullOrEmpty(value) ?
            MissingToken(leadingTrivia, keywordKind, trailingTrivia) :
            Token(leadingTrivia, keywordKind, value, trailingTrivia);
    }
}
