using Gherkin;

namespace Reqnroll.FeatureSourceGenerator.Gherkin;

using Location = global::Gherkin.Ast.Location;

class SourceTokenScanner(SourceText source) : ITokenScanner
{
    private readonly SourceText _source = source;

    private int _lineNumber = -1;

    public Token Read()
    {
        var lineNumber = ++_lineNumber;

        if (lineNumber >= _source.Lines.Count)
        {
            return new Token(null, new Location(lineNumber + 1));
        }

        var line = _source.Lines[lineNumber];
        var location = new Location(line.LineNumber + 1);

        return new Token(new GherkinLine(_source.ToString(line.Span), line.LineNumber + 1), location);
    }
}
