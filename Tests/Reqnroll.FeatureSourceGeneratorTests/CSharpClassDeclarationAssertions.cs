using FluentAssertions.Execution;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reqnroll.FeatureSourceGenerator;
public class CSharpClassDeclarationAssertions(ClassDeclarationSyntax? subject) :
    CSharpClassDeclarationAssertions<CSharpClassDeclarationAssertions>(subject)
{
}

public class CSharpClassDeclarationAssertions<TAssertions>(ClassDeclarationSyntax? subject) : 
    CSharpSyntaxAssertions<ClassDeclarationSyntax, TAssertions>(subject)
    where TAssertions : CSharpClassDeclarationAssertions<TAssertions>
{
    protected override string Identifier => "class";

    /// <summary>
    /// Expects the class declaration have only a single attribute with a specific identifier.
    /// </summary>
    /// <param name="type">
    /// The declared type of the attribute.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndWhichConstraint<TAssertions, AttributeSyntax> HaveSingleAttribute(
        string type,
        string because = "",
        params object[] becauseArgs)
    {
        var expectation = "Expected {context:class} to have a single attribute " +
            $"which is of type \"{type}\" {{reason}}";

        bool notNull = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is not null)
            .FailWith(expectation + ", but found <null>.");

        AttributeSyntax? match = default;

        if (notNull)
        {
            var attributes = Subject!.AttributeLists.SelectMany(list => list.Attributes).ToList();

            switch (attributes.Count)
            {
                case 0: // Fail, Collection is empty
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but the class has no attributes.");

                    break;
                case 1: // Success Condition
                    var single = attributes.Single();

                    if (single.Name.ToString() != type)
                    {
                        Execute.Assertion
                            .BecauseOf(because, becauseArgs)
                            .FailWith(expectation + ", but found the attribute \"{0}\".", single.Name);
                    }
                    else
                    {
                        match = single;
                    }

                    break;
                default: // Fail, Collection contains more than a single item
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but found {0}.", attributes);

                    break;
            }
        }

        return new AndWhichConstraint<TAssertions, AttributeSyntax>((TAssertions)this, match!);
    }

    public AndWhichConstraint<TAssertions, MethodDeclarationSyntax> ContainMethod(
        string identifier,
        string because = "",
        params object[] becauseArgs)
    {
        var expectation = "Expected {context:class} to have a method " +
            $"named \"{identifier}\" {{reason}}";

        bool notNull = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is not null)
            .FailWith(expectation + ", but found <null>.");

        MethodDeclarationSyntax? match = default;

        if (notNull)
        {
            var methods = Subject!.Members.OfType<MethodDeclarationSyntax>().ToList();

            if (methods.Count == 0)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith(expectation + ", but the class has no methods.");
            }
            else
            {
                match = methods.FirstOrDefault(method => method.Identifier.Text == identifier);

                if (match == null)
                {
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but found {0}.", methods);
                }
            }
        }

        return new AndWhichConstraint<TAssertions, MethodDeclarationSyntax>((TAssertions)this, match!);
    }
}
