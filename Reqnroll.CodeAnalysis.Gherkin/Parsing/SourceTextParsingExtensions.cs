using Microsoft.CodeAnalysis.Text;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Text;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

using static InternalSyntaxFactory;

internal static class SourceTextParsingExtensions
{
    /// <summary>
    /// Reads whitespace beginning at the start of the specified text span and advancing through the source text
    /// until a non-whitespace character is encountered or the end of the span is reached.
    /// </summary>
    /// <param name="sourceText">The source text to read from.</param>
    /// <param name="textSpan">The span within the source text to read from.</param>
    /// <returns>A <see cref="InternalSyntaxTrivia"/> representing the whitespace read from the source, or <c>null</c>
    /// if there is no whitespace at the start of the text-span.</returns>
    public static InternalSyntaxTrivia? ConsumeWhitespace(this SourceText sourceText, TextSpan textSpan)
    {
        return sourceText.ConsumeWhitespace(textSpan.Start, textSpan.End);
    }

    /// <summary>
    /// Reads whitespace beginning at the specified index and advancing through the source text until a non-whitespace 
    /// character is encountered or the end index is reached.
    /// </summary>
    /// <param name="sourceText">The source text to read from.</param>
    /// <param name="startIndex">The index to start consuming characters from.</param>
    /// <param name="endIndex">The index at which to stop consuming.</param>
    /// <returns>A <see cref="InternalSyntaxTrivia"/> representing the whitespace read from the source, or <c>null</c>
    /// if there is no whitespace at the start index.</returns>
    public static InternalSyntaxTrivia? ConsumeWhitespace(this SourceText sourceText, int startIndex, int endIndex)
    {
        var builder = new StringBuilder();

        for (var index = startIndex; index < endIndex; index++)
        {
            var c = sourceText[index];

            if (char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
            else
            {
                break;
            }
        }

        return builder.Length > 0 ? Whitespace(builder.ToString()) : null;
    }

    /// <summary>
    /// Reads whitespace from before the end of the specified text span and moves backwards through the source text
    /// until a non-whitespace character is encountered or the start of the span is consumed.
    /// </summary>
    /// <param name="sourceText">The source text to read from.</param>
    /// <param name="textSpan">The span within the source text to read from.</param>
    /// <returns>A <see cref="InternalSyntaxTrivia"/> representing the whitespace read from the source, or <c>null</c>
    /// if there is no whitespace at the end of the text-span.</returns>
    public static InternalSyntaxTrivia? ReverseConsumeWhitespace(this SourceText sourceText, TextSpan textSpan)
    {
        return sourceText.ReverseConsumeWhitespace(textSpan.End - 1, textSpan.Start);
    }

    /// <summary>
    /// Reads whitespace from the specified index until a non-whitespace character is encountered or the end 
    /// index is reached.
    /// </summary>
    /// <param name="sourceText">The source text to read from.</param>
    /// <param name="startIndex">The index to start consuming characters from.</param>
    /// <param name="endIndex">The index at which to stop consuming.</param>
    /// <returns>A <see cref="InternalSyntaxTrivia"/> representing the whitespace read from the source, or <c>null</c>
    /// if there is no whitespace at the specified index.</returns>
    public static InternalSyntaxTrivia? ReverseConsumeWhitespace(this SourceText sourceText, int startIndex, int endIndex)
    {
        if (startIndex >= sourceText.Length)
        {
            return null;
        }

        var builder = new StringBuilder();

        for (var index = startIndex; index > endIndex; index--)
        {
            var c = sourceText[index];

            if (char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
            else
            {
                break;
            }
        }

        return builder.Length > 0 ? Whitespace(builder.ToString()) : null;
    }
}
