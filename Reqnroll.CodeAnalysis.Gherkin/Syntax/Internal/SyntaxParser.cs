using Gherkin;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

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
        _builder.AddError(error);
    }
}
