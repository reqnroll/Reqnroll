using Microsoft.CodeAnalysis.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

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
