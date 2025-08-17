using Gherkin;
using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal class DataTableRuleHandler() : BaseRuleHandler(RuleType.DataTable)
{
    private readonly InternalSyntaxList<TableRowSyntax.Internal>.Builder _rows = new();

    protected override void AppendTableRow(Token token, TextLine line, ParsingContext context)
    {
        // Table rows have the following layout
        //
        // [bar] ([value] [bar])+
        //
        // The parser doesn't provide anything about the row structure,
        // nor does it provide guarantees about structure beyond the first bar token;
        // it's possible to get a line that just contains the initial bar.

        var source = context.SourceText;

        // Step through the line to read alternating delimeters and values.
        var values = new InternalSyntaxList<InternalNode?>.Builder();

        // Syntax for tables can get complex, so we'll tokenize the line before we parse structure.
        var nodes = TableLineLexer.EnumerateTokens(line).ToList();
        var index = 0;

        CodeAnalysisDebug.Assert(
            nodes.Sum(node => node.FullWidth) == line.Span.Length,
            "Lexed tokens do not account for all input text",
            "Expected lexed tokens to have total length {0} but found {1}",
            line.Span.Length,
            nodes.Sum(node => node.FullWidth));

        InternalNode? ConsumeTrivia()
        {
            InternalNode? trivia = null;

            while (index < nodes.Count)
            {
                var node = nodes[index];

                if (!node.IsTrivia)
                {
                    break;
                }

                trivia += node;
                index++;
            }

            return trivia;
        }

        // Start by consolidating leading trivia.
        var leading = context.ConsumeLeadingTrivia() + ConsumeTrivia();

        // The next token should be the first bar on the line, which is assigned the leading trivia.
        var firstBar = nodes[index++];

        CodeAnalysisDebug.Assert(
            firstBar.Kind == SyntaxKind.VerticalBarToken,
            "Parser sent table line without leading bar",
            "Expected the token to be VerticalBarToken but was {0}",
            firstBar.Kind);

        firstBar = firstBar.WithLeadingTrivia(leading);

        // Consume trailing trivia and prepare us to read the remaining tokens.
        var trailing = ConsumeTrivia();

        if (trailing != null)
        {
            firstBar = firstBar.WithTrailingTrivia(trailing);
        }

        // The remaining tokens should be tuples of [literal] [bar], but the literal is optional if the cell is empty.
        var previousTokenType = SyntaxKind.VerticalBarToken;
        InternalNode? trivia = null;

        while (index < nodes.Count)
        {
            var node = nodes[index++];

            switch (node.Kind)
            {
                case SyntaxKind.VerticalBarToken:
                    if (previousTokenType == SyntaxKind.VerticalBarToken)
                    {
                        // No literal between these bars, so we append an empty cell to the list.
                        values.Add(EmptyTableCell());
                    }

                    // Include leading trivia.
                    if (trivia != null)
                    {
                        node = node.WithLeadingTrivia(trivia);
                        trivia = null; // Reset trivia after use.
                    }

                    // Consume any trivia that may be present before the next token.
                    trivia = ConsumeTrivia();
                    if (trivia != null)
                    {
                        node = node.WithTrailingTrivia(trivia);
                        trivia = null; // Reset trivia after use.
                    }

                    // Include the node directly in the list.
                    values.Add(node);
                    break;

                case SyntaxKind.TableLiteralToken:
                    CodeAnalysisDebug.Assert(
                       previousTokenType == SyntaxKind.VerticalBarToken,
                       "Token list contains two literals in a row",
                       "Expected the token to be VerticalBarToken but was TableLiteralToken");

                    // Consume any trivia before the next token.
                    trivia = ConsumeTrivia();

                    // Include the token as a text cell.
                    values.Add(TextTableCell(node));
                    break;

                default:
                    throw new InvalidOperationException($"Encountered unexpected token {node}");
            }

            previousTokenType = node.Kind;
        }

        // Once we've exhausted all tokens, two cleanup jobs have to happen:
        // 1. If the last token is NOT a bar, we need to add that as a missing token.
        // 2. The end of line trivia needs to be added to the last token.
        InternalNode lastBar;
        var lastToken = values.LastOrDefault();
        if (lastToken == null)
        {
            // There is no other content besides the first bar.
            lastBar = MissingToken(null, SyntaxKind.VerticalBarToken, line.GetEndOfLineTrivia());
        }
        else if (lastToken.Kind != SyntaxKind.VerticalBarToken)
        {
            trivia = lastToken.GetTrailingTrivia();

            // The last token is not a vertical bar.
            // If there's a comment in the trivia, whitespace before the comment is assigned to the last token
            // and the comment and all subsequent trivia is assigned after the missing vertical bar.
            if (trivia != null && trivia.IsList)
            {
                var triviaList = (InternalSyntaxList<InternalNode>)trivia;

                var literalTrailing = InternalNode.CreateList(
                    triviaList.TakeWhile(trivia => trivia.Kind != SyntaxKind.CommentTrivia));
                var barTrailing = InternalNode.CreateList(
                    triviaList.SkipWhile(trivia => trivia.Kind != SyntaxKind.CommentTrivia));

                lastToken = lastToken.WithTrailingTrivia(literalTrailing);
                values[values.Count - 1] = lastToken;

                lastBar = MissingToken(null, SyntaxKind.VerticalBarToken, barTrailing + line.GetEndOfLineTrivia());
            }
            else
            {
                lastToken = lastToken.WithTrailingTrivia(trivia);
                values[values.Count - 1] = lastToken;

                lastBar = MissingToken(null, SyntaxKind.VerticalBarToken, line.GetEndOfLineTrivia());
            }
        }
        else
        {
            // The last token is a vertical bar.
            lastBar = lastToken.WithTrailingTrivia(lastToken.GetTrailingTrivia() + line.GetEndOfLineTrivia());
            values.RemoveAt(values.Count - 1);
        }

        _rows.Add(
            TableRow(
                firstBar,
                values.ToSyntaxList(),
                lastBar));
    }

    internal TableSyntax.Internal? CreateTableSyntax() => Table(_rows.ToSyntaxList());
}
