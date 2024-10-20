using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reqnroll.FeatureSourceGenerator;

public class CSharpSyntaxAssertions<TNode>(TNode? subject) : CSharpSyntaxAssertions<TNode, CSharpSyntaxAssertions<TNode>>(subject)
    where TNode : CSharpSyntaxNode
{
}

public class CSharpSyntaxAssertions<TNode, TAssertions>(TNode? subject) : ReferenceTypeAssertions<TNode, TAssertions>(subject!)
    where TNode: CSharpSyntaxNode
    where TAssertions : CSharpSyntaxAssertions<TNode, TAssertions>
{
    protected override string Identifier => "node";

    /// <summary>
    /// Expects the node contain only a single child which is a namespace declaration with a specific identifier.
    /// </summary>
    /// <param name="identifier">
    /// The identifier of the namespace.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    public AndWhichConstraint<TAssertions, NamespaceDeclarationSyntax> ContainSingleNamespaceDeclaration(
        string identifier,
        string because = "", 
        params object[] becauseArgs)
    {
        var expectation = "Expected {context:node} to contain a single child node " +
            $"which is the declaration of the namespace \"{identifier}\" {{reason}}";

        bool notNull = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is not null)
            .FailWith(expectation + ", but found <null>.");

        NamespaceDeclarationSyntax? match = default;

        if (notNull)
        {
            var actualChildNodes = Subject!.ChildNodes().Cast<CSharpSyntaxNode>().ToList();

            switch (actualChildNodes.Count)
            {
                case 0: // Fail, Collection is empty
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but the node has no children.");

                    break;
                case 1: // Success Condition
                    var single = actualChildNodes.Single();

                    if (single is not NamespaceDeclarationSyntax ns)
                    {
                        Execute.Assertion
                            .BecauseOf(because, becauseArgs)
                            .FailWith(expectation + ", but found {0}.", Subject);
                    }
                    else if (ns.Name.ToString() != identifier)
                    {
                        Execute.Assertion
                            .BecauseOf(because, becauseArgs)
                            .FailWith(expectation + ", but found the namespace \"{0}\".", ns.Name.ToString());
                    }
                    else
                    {
                        match = ns;
                    }

                    break;
                default: // Fail, Collection contains more than a single item
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but found {0}.", actualChildNodes);

                    break;
            }
        }

        return new AndWhichConstraint<TAssertions, NamespaceDeclarationSyntax>((TAssertions)this, match!);
    }
}
