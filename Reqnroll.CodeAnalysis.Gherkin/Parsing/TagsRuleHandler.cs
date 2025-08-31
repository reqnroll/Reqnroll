using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class TagsRuleHandler() : ParsingRuleHandler(RuleType.Tags)
{
    public InternalNode? Tags { get; private set; }

    protected override void AppendTagLine(Token token, TextLine line, ParsingContext context)
    {
        // Tag lines have a truly token-based layout, where the tag structured is defined as:
        //
        // [at-symbol][identifier]
        //
        // And a tag-line can have any number of tags on it.
        //
        // Importantly, the identifier cannot contain whitespace. Any whitespace must be followed
        // by either another tag or a comment, where a comment marker will indicate the rest of the
        // text should be processed as a comment.

        // The parser doesn't provide much help here except to tell us that this line starts with a tag.

        // Skip past leading whitespace characters.
        var leadingWhitespace = context.SourceText.ConsumeWhitespace(line.Start, line.Start + token.Line.Indent);
        var leading = context.ConsumeLeadingTrivia() + leadingWhitespace;

        // Step through the line to extract the tags.
        var reader = new LineReader(line);
        var tags = new InternalSyntaxList<TagSyntax.Internal>.Builder();

        if (leadingWhitespace != null)
        {
            reader.Skip(leadingWhitespace.Width);
        }

        while (reader.MoveNext())
        {
            if (reader.Current == '@')
            {
                // Start of tag:
                var atSymbolPosition = reader.Position;

                // Consume content until we hit another tag marker or a comment.
                var content = reader.Fork();
                content.SkipUntil(c => c == '@' || c == '#');

                var end = content.Position - 1;

                // Attach leading trivia to the at symbol.
                var atSymbol = Token(leading, SyntaxKind.AtToken, null);

                // All remaining characters are the tag indentifier and its trailing whitespace.
                var identifierWhitespace = context.SourceText.ReverseConsumeWhitespace(end, atSymbolPosition);
                var identifierEnd = end - (identifierWhitespace?.Width ?? 0);
                var identifierText = context.SourceText.ToString(TextSpan.FromBounds(atSymbolPosition + 1, identifierEnd + 1));
                var identifier = Literal(null, SyntaxKind.NameToken, identifierText, identifierText, identifierWhitespace);

                var tag = Tag(atSymbol, identifier);

                tags.Add(tag);

                // Move the primary reader to the end of the content.
                reader.Skip(content.Position - atSymbolPosition - 1);
            }
            else if (reader.Current == '#')
            {
                // Start of comment:
                
                // All remaining characters on the line are now comment.
                var commentStart = reader.Position;
                var commentWhitespace = context.SourceText.ReverseConsumeWhitespace(line.End, commentStart);
                var commentEnd = line.End - (commentWhitespace?.Width ?? 0);
                var commentText = context.SourceText.ToString(TextSpan.FromBounds(commentStart, commentEnd));

                var comment = Comment(commentText) + commentWhitespace;

                // Add to the last tag.
                var tag = tags.Last();
                tag = (TagSyntax.Internal)tag.WithTrailingTrivia(tag.GetTrailingTrivia() + comment);
                tags[tags.Count - 1] = tag;
            }
            else
            {
                throw new InvalidOperationException("Unexpected content in tag node.");
            }
        }

        // Include the end-of-line trivia on the final tag.
        var lastTag = tags.Last();
        lastTag = (TagSyntax.Internal)lastTag.WithTrailingTrivia(line.GetEndOfLineTrivia());

        tags[tags.Count - 1] = lastTag;

        Tags += tags.ToSyntaxList();
    }
}
