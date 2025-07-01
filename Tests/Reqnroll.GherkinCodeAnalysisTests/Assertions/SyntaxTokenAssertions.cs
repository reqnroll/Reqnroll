using FluentAssertions;
using FluentAssertions.Execution;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

public class SyntaxTokenAssertions(SyntaxToken token)
{
    public SyntaxToken Subject => token;

    private const string Identifier = "syntax token";

    [CustomAssertion]
    public AndConstraint<SyntaxTokenAssertions> BeEquivalentTo(
        SyntaxToken expected,
        [StringSyntax("CompositeFormat")] string because = "",
        params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(token.IsEquivalentTo(expected))
            .WithDefaultIdentifier(Identifier)
            .FailWith("Expected {context} to be equivalent to {0}{reason}, but found {1}.", expected, token);

        return new AndConstraint<SyntaxTokenAssertions>(this);
    }
}
