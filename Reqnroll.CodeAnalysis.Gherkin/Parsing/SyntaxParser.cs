using Gherkin;
using System.Runtime.ExceptionServices;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal class SyntaxParser : Parser<GherkinSyntaxTree>
{
    private readonly ParsedSyntaxTreeBuilder _builder;

    public SyntaxParser(ParsedSyntaxTreeBuilder syntaxTreeBuilder) : base(syntaxTreeBuilder)
    {
        StopAtFirstError = false;
        _builder = syntaxTreeBuilder;
    }

    protected override void AddError(ParserContext context, ParserException error)
    {
        if (error is ParsingException parsingException)
        {
            parsingException.Exception.Throw();
        }

        _builder.AddError(error);
    }
}

internal class ParsingException(ExceptionDispatchInfo exception) : ParserException(exception.SourceException.Message)
{
    public ExceptionDispatchInfo Exception { get; } = exception;
}
