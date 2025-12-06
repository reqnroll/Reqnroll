using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace Reqnroll.Analyzers;

/// <summary>
/// A test-helper type representing a source text with a location marked in it.
/// </summary>
/// <remarks>
/// <para>Often in source-parsing tests, we need to check that the output includes a location that indicates
/// a specific span in the source text. Whilst we can hard-code locations into our tests, this makes
/// changing the sample source brittle and it can be hard to calculate the expected span and line numbers
/// from the source.</para>
/// <para>The <see cref="MarkedSourceText"/> type provides a way to mark a piece of source text with
/// a pair of delimeter characters that indicate the interesting location in the source. The markers are
/// converted into a <see cref="Location"/> value which can then be used to perform test assertions.</para>
/// <para>The location is marked using a pair of characters:
/// <list type="bullet">
///   <item><c>⇥</c> - Marks the start of the location as the character to the right of this.</item>
///   <item><c>⇤</c> - Marks the end of the location as the character to the left of this.</item>
/// </list>
/// The marker characters are removed from the source text and the indexes returned in the
/// <see cref="MarkedLocation"/> property reflect the position as it appears in</para>
/// </remarks>
public partial class MarkedSourceText : SourceText
{
    private readonly string _undecoratedText;
    private readonly TextSpan _markedSpan;

    private MarkedSourceText(string undecoratedText, TextSpan markedSpan)
    {
        _undecoratedText = undecoratedText;
        _markedSpan = markedSpan;
    }

    public Location GetMarkedLocation(string path = "")
    {
        return Location.Create(
            path,
            _markedSpan,
            Lines.GetLinePositionSpan(_markedSpan));
    }

    public override Encoding? Encoding => null;

    public override int Length => _undecoratedText.Length;

    public override char this[int position] => _undecoratedText[position];

    public static MarkedSourceText Parse(string source)
    {
        var start = source.IndexOf('⇥');
        var end = source.IndexOf('⇤');

        if (start == -1)
        {
            throw new FormatException("Source text does not contain a start marker '⇥'.");
        }

        if (end == -1)
        {
            throw new FormatException("Source text does not contain an end marker '⇤'.");
        }

        if (end < start)
        {
            throw new FormatException("End marker '⇤' appears before start marker '⇥'.");
        }

        var text = Undecorate(source);
        var span = TextSpan.FromBounds(start, end - 1);

        return new MarkedSourceText(text, span);
    }

    private static string Undecorate(string source) => LocationMarkerRegex().Replace(source, "");

    [GeneratedRegex("⇥|⇤")]
    private static partial Regex LocationMarkerRegex();

    public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        _undecoratedText.CopyTo(sourceIndex, destination, destinationIndex, count);
    }
}
