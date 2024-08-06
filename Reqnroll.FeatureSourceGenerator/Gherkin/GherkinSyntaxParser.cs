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
        var start = text.Lines[location.Line - 1].Start + location.Column;

        return Microsoft.CodeAnalysis.Location.Create(
            path,
            new TextSpan(start, 0),
            new LinePositionSpan(
                new LinePosition(location.Line, location.Column),
                new LinePosition(location.Line, location.Column)));
    }
}
