using FluentAssertions.Execution;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reqnroll.FeatureSourceGenerator;

public class CSharpNamespaceDeclarationAssertions(NamespaceDeclarationSyntax? subject) :
    CSharpNamespaceDeclarationAssertions<CSharpNamespaceDeclarationAssertions>(subject)
{
}

public class CSharpNamespaceDeclarationAssertions<TAssertions>(NamespaceDeclarationSyntax? subject) :
    CSharpSyntaxAssertions<NamespaceDeclarationSyntax, TAssertions>(subject)
    where TAssertions : CSharpNamespaceDeclarationAssertions<TAssertions>
{
    protected override string Identifier => "namespace";

    /// <summary>
    /// Expects the namespace contain only a single child which is a class declaration with a specific identifier.
    /// </summary>
    /// <param name="identifier">
    /// The identifier of the class.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndWhichConstraint<TAssertions, ClassDeclarationSyntax> ContainSingleClassDeclaration(
        string identifier,
        string because = "",
        params object[] becauseArgs)
    {
        var expectation = "Expected {context:namespace} to contain a single child node " +
            $"which is the declaration of the class \"{identifier}\" {{reason}}";

        bool notNull = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is not null)
            .FailWith(expectation + ", but found <null>.");

        ClassDeclarationSyntax? match = default;

        if (notNull)
        {
            var members = Subject!.Members;

            switch (members.Count)
            {
                case 0: // Fail, Collection is empty
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but the node has no children.");

                    break;
                case 1: // Success Condition
                    var single = members.Single();

                    if (single is not ClassDeclarationSyntax declaration)
                    {
                        Execute.Assertion
                            .BecauseOf(because, becauseArgs)
                            .FailWith(expectation + ", but found {0}.", Subject);
                    }
                    else if (declaration.Identifier.ToString() != identifier)
                    {
                        Execute.Assertion
                            .BecauseOf(because, becauseArgs)
                            .FailWith(expectation + ", but found the class \"{0}\".", declaration.Identifier.ToString());
                    }
                    else
                    {
                        match = declaration;
                    }

                    break;
                default: // Fail, Collection contains more than a single item
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but found {0}.", members);

                    break;
            }
        }

        return new AndWhichConstraint<TAssertions, ClassDeclarationSyntax>((TAssertions)this, match!);
    }
}
