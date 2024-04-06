using Gherkin;
using Gherkin.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator;

using Location = Gherkin.Ast.Location;

public class GherkinSyntaxParser
{
    public static readonly DiagnosticDescriptor SyntaxError = new(
        id: DiagnosticIds.SyntaxError,
        title: GherkinParserResources.SyntaxErrorTitle,
        messageFormat: GherkinParserResources.SyntaxErrorMessage,
        "Reqnroll.Gherkin",
        DiagnosticSeverity.Error,
        true);

    public GherkinSyntaxTree Parse(SourceText text, string path, CancellationToken cancellationToken = default)
    {
        var parser = new Parser { StopAtFirstError = false };

        GherkinDocument? document = null;
        ImmutableArray<Diagnostic>? diagnostics = null;

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // CONSIDER: Using a parser that doesn't throw exceptions for syntax errors.
            document = parser.Parse(new SourceTokenScanner(text));
        }
        catch (CompositeParserException ex)
        {
            diagnostics = ex.Errors.Select(error => CreateGherkinDiagnostic(error, text, path)).ToImmutableArray();
        }

        return new GherkinSyntaxTree(
            document ?? new GherkinDocument(null, Array.Empty<Comment>()),
            diagnostics ?? ImmutableArray<Diagnostic>.Empty,
            path);
    }

    private Diagnostic CreateGherkinDiagnostic(ParserException exception, SourceText text, string path)
    {
        return Diagnostic.Create(SyntaxError, CreateLocation(exception.Location, text, path), exception.Message);
    }

    private Microsoft.CodeAnalysis.Location CreateLocation(Location location, SourceText text, string path)
    {
        var start = text.Lines[location.Line].Start + location.Column;

        return Microsoft.CodeAnalysis.Location.Create(
            path,
            new TextSpan(start, 0),
            new LinePositionSpan(
                new LinePosition(location.Line, location.Column),
                new LinePosition(location.Line, location.Column)));
    }
}

class SourceTokenScanner : ITokenScanner
{
    private readonly SourceText _source;

    private int _lineNumber = -1;

    public SourceTokenScanner(SourceText source)
    {
        _source = source;
    }

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
