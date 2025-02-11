using Gherkin;
using Microsoft.CodeAnalysis;
using Reqnroll.CodeAnalysis.Gherkin.Syntax.Internal;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

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
