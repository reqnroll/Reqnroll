using FluentAssertions.Equivalency;
using Reqnroll.CodeAnalysis.Gherkin.Syntax;

namespace Reqnroll.CodeAnalysis.Gherkin.Assertions;

public sealed class SyntaxNodeOrTokenStep : IEquivalencyStep
{
    public EquivalencyResult Handle(
        Comparands comparands,
        IEquivalencyValidationContext context,
        IEquivalencyValidator nestedValidator)
    {
        if (!IsSyntaxNodeOrTokenType(comparands.RuntimeType))
        {
            return EquivalencyResult.ContinueWithNext;
        }

        var subject = Unwrap(comparands.Subject);
        var expectation = Unwrap(comparands.Expectation);

        var unwrapped = new Comparands(subject, expectation, typeof(object));

        nestedValidator.RecursivelyAssertEquality(unwrapped, context);

        return EquivalencyResult.AssertionCompleted;
    }

    private static object? Unwrap(object subject)
    {
        if (subject == null)
        {
            return null;
        }

        var type = subject.GetType();

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SyntaxNodeOrToken<>))
        {
            return subject;
        }

        var unwrapMethod = typeof(SyntaxNodeOrTokenStep)
            .GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Single(method => method.Name == nameof(Unwrap) && method.IsGenericMethod);

        return unwrapMethod.MakeGenericMethod(type.GetGenericArguments()[0]).Invoke(null, [subject]);
    }

    private static object? Unwrap<TNode>(SyntaxNodeOrToken<TNode> syntaxNodeOrToken)
        where TNode : SyntaxNode
    {
        return syntaxNodeOrToken.IsToken ? syntaxNodeOrToken.AsToken() : syntaxNodeOrToken.AsNode();
    }

    private static bool IsSyntaxNodeOrTokenType(Type type) => 
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SyntaxNodeOrToken<>);
}
