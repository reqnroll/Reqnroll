using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal static class TextLineParsingExtensions
{
    public static InternalNode? GetEndOfLineTrivia(this TextLine line)
    {
        if (line.End == line.EndIncludingLineBreak)
        {
            return null;
        }

        return EndOfLine(line.Text!, TextSpan.FromBounds(line.End, line.EndIncludingLineBreak));
    }
}
