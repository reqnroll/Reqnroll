using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

public static class SyntaxAssertionExtensions
{
    public static SyntaxTokenAssertions Should(this SyntaxToken token) => new(token);
}
