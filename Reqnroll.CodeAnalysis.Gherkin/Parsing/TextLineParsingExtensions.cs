using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal static class TextLineParsingExtensions
{
    /// <summary>
    /// Gets a node representing any end-o-line trivia on the line.
    /// </summary>
    /// <param name="line">The line to read from.</param>
    /// <returns>An <see cref="InternalNode"/> that is the end-of-line token representing the end of the line, 
    /// or <c>null</c> if the line does not have any end-of-line trivia.</returns>
    public static InternalNode? GetEndOfLineTrivia(this TextLine line)
    {
        if (line.End == line.EndIncludingLineBreak)
        {
            return null;
        }

        return EndOfLine(line.Text!, TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
    }
}
