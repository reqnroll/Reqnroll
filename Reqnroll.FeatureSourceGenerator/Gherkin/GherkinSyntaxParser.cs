using Gherkin;
using Gherkin.Ast;
using System.Collections.Immutable;

namespace Reqnroll.FeatureSourceGenerator.Gherkin;

using Location = global::Gherkin.Ast.Location;

internal static class GherkinSyntaxParser
{
    public static readonly DiagnosticDescriptor SyntaxError = new(
        id: DiagnosticIds.SyntaxError,
        title: GherkinSyntaxParserResources.SyntaxErrorTitle,
        messageFormat: GherkinSyntaxParserResources.SyntaxErrorMessage,
        "Reqnroll.Gherkin",
        DiagnosticSeverity.Error,
        true);

    //public GherkinSyntaxTree Parse(SourceText text, string path, CancellationToken cancellationToken = default)
    //{
    //    var parser = new Parser { StopAtFirstError = false };

    //    GherkinDocument? document = null;
    //    ImmutableArray<Diagnostic>? diagnostics = null;

    //    cancellationToken.ThrowIfCancellationRequested();

    //    try
    //    {
    //        // CONSIDER: Using a parser that doesn't throw exceptions for syntax errors.
    //        document = parser.Parse(new SourceTokenScanner(text));
    //    }
    //    catch (CompositeParserException ex)
    //    {
    //        diagnostics = ex.Errors.Select(error => CreateGherkinDiagnostic(error, text, path)).ToImmutableArray();
    //    }

    //    return new GherkinSyntaxTree(
    //        document ?? new GherkinDocument(null, []),
    //        diagnostics ?? ImmutableArray<Diagnostic>.Empty,
    //        path);
    //}

    public static Diagnostic CreateGherkinDiagnostic(ParserException exception, SourceText text, string path)
    {
        return Diagnostic.Create(SyntaxError, CreateLocation(exception.Location, text, path), exception.Message);
    }

    private static Microsoft.CodeAnalysis.Location CreateLocation(Location location, SourceText text, string path)
    {
        // Roslyn uses 0-based indexes for line and character-offset numbers.
        // The Gherkin parser uses 1-based indexes for line and column numbers.
        var positionStart = location.ToLinePosition();
        var line = text.Lines[positionStart.Line];
        var lineLength = line.Span.Length;
        var positionEnd = new LinePosition(positionStart.Line, lineLength);

        return Microsoft.CodeAnalysis.Location.Create(
            path,
            new TextSpan(line.Span.Start + positionStart.Character, lineLength - positionStart.Character),
            new LinePositionSpan(positionStart, positionEnd)); ;
    }
}
