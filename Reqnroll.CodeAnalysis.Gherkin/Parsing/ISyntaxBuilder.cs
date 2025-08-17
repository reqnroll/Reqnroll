using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Parsing;

internal interface ISyntaxBuilder
{
    InternalNode? CreateSyntax();
}
