using FluentAssertions.Equivalency;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Reqnroll.FeatureSourceGenerator;
public class CSharpMethodDeclarationAssertions(MethodDeclarationSyntax? subject) :
    CSharpMethodDeclarationAssertions<CSharpMethodDeclarationAssertions>(subject)
{
}

public class CSharpMethodDeclarationAssertions<TAssertions>(MethodDeclarationSyntax? subject) :
    CSharpSyntaxAssertions<MethodDeclarationSyntax, TAssertions>(subject)
    where TAssertions : CSharpMethodDeclarationAssertions<TAssertions>
{
    protected override string Identifier => "method";

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
        var expectation = "Expected {context:method} to have a single attribute " +
            $"which is of type \"{type}\"{{reason}}";

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

    public AndWhichConstraint<TAssertions, AttributeSyntax> HaveAttribute(
        string type,
        string because = "",
        params object[] becauseArgs)
    {
        var expectation = "Expected {context:method} to have an attribute " +
            $"of type \"{type}\"{{reason}}";

        bool notNull = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject is not null)
            .FailWith(expectation + ", but found <null>.");

        AttributeSyntax? match = default;

        if (notNull)
        {
            var attributes = Subject!.AttributeLists.SelectMany(list => list.Attributes).ToList();

            if (attributes.Count == 0)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith(expectation + ", but the method has no attributes.");
            }
            else
            {
                match = attributes.FirstOrDefault(attribute => attribute.Name.ToString() == type);

                if (match == null)
                {
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith(expectation + ", but found {0}.", attributes);
                }
            }
        }

        return new AndWhichConstraint<TAssertions, AttributeSyntax>((TAssertions)this, match!);
    }

    public AndConstraint<TAssertions> HaveAttribuesEquivalentTo(
        AttributeDescriptor[] expectation,
        string because = "",
        params object[] becauseArgs) =>
        HaveAttribuesEquivalentTo((IEnumerable<AttributeDescriptor>)expectation, because, becauseArgs);

    public AndConstraint<TAssertions> HaveAttribuesEquivalentTo(
        IEnumerable<AttributeDescriptor> expectation,
        string because = "",
        params object[] becauseArgs)
    {
        using var scope = new AssertionScope();

        scope.FormattingOptions.UseLineBreaks = true;

        var assertion = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected {context:the method} to have attributes equivalent to {0}", expectation)
            .ForCondition(Subject is not null)
            .FailWith("but {context:the method} is <null>.");

        var expected = expectation.ToList();
        var actual = Subject!.AttributeLists
            .SelectMany(list => list.Attributes)
            .Select(SyntaxInterpreter.GetAttributeDescriptor)
            .ToList();


        var missing = expected.ToList();
        var extra = actual.ToList();

        foreach (var item in expected)
        {
            if (extra.Remove(item))
            {
                missing.Remove(item);
            }
        }

        if (missing.Count > 0)
        {
            if (extra.Count > 0)
            {
                assertion
                    .Then
                    .FailWith("but the method is missing attributes {0} and has extra attributes {1}", missing, extra);
            }
            else
            {
                assertion
                    .Then
                    .FailWith("but the method is missing attributes {0}", missing);
            }
        }
        else if (extra.Count > 0)
        {
            assertion
                .Then
                .FailWith("but the method has extra attributes {0}", extra);
        }

        return new AndConstraint<TAssertions>((TAssertions)this);
    }

    public AndConstraint<TAssertions> HaveParametersEquivalentTo(
        IEnumerable<ParameterDescriptor> expectation,
        string because = "",
        params object[] becauseArgs)
    {
        using var scope = new AssertionScope();

        scope.FormattingOptions.UseLineBreaks = true;

        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected {context:the method} to have parameters equivalent to {0} {reason}", expectation)
            .ForCondition(Subject is not null)
            .FailWith("but {context:the method} is <null>.");

        var expected = expectation.ToList();
        var actual = Subject!.ParameterList.Parameters;

        for (var i = 0; i < expected.Count; i++)
        {
            var expectedItem = expected[i];

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .WithExpectation("Expected {context:the method} to have parameter {0}: \"{1}\" {reason}", i+1, expectedItem)
                .ForCondition(Subject.ParameterList.Parameters.Count != 0)
                .FailWith("but {context:the method} has no parameters defined.", Subject.ParameterList.Parameters.Count)
                .Then
                .ForCondition(Subject.ParameterList.Parameters.Count > i)
                .FailWith("but {context:the method} only has {0} parameters defined.", Subject.ParameterList.Parameters.Count)
                .Then
                .Given(() => Subject.ParameterList.Parameters[i])
                .ForCondition(actual => false)// actual.IsEquivalentTo(expectedItem, true))
                .FailWith("but found \"{0}\".", expectedItem);
        }

        if (expected.Count < actual.Count)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .WithExpectation("Expected {context:the method} to have parameters equivalent to {0}{reason}", expectation)
                .FailWith("but {context:the method} has extra parameters {0}.", actual.Skip(expected.Count));
        }

        return new AndConstraint<TAssertions>((TAssertions)this);
    }
}

internal static class SyntaxInterpreter
{
    public static AttributeDescriptor GetAttributeDescriptor(AttributeSyntax attribute)
    {
        var type = attribute.Name switch
        {
            QualifiedNameSyntax qname => new TypeIdentifier(
                new NamespaceString(
                    qname.Left.ToString().StartsWith("global::") ? 
                        qname.Left.ToString()[8..] : 
                        qname.Left.ToString()),
                new IdentifierString(qname.Right.ToString())),
            _ => throw new NotImplementedException()
        };

        ImmutableArray<object?> positionalArguments;
        ImmutableDictionary<string, object?> namedArguments;

        if (attribute.ArgumentList == null)
        {
            positionalArguments = [];
            namedArguments = ImmutableDictionary<string, object?>.Empty;
        }
        else
        {
            positionalArguments = attribute.ArgumentList.Arguments
                .Where(arg => arg.NameEquals == null)
                .Select(arg => GetLiteralValue(arg.Expression))
                .ToImmutableArray();

            namedArguments = attribute.ArgumentList.Arguments
                .Where(arg => arg.NameEquals != null)
                .ToImmutableDictionary(arg => arg.NameEquals!.Name.ToString(), arg => GetLiteralValue(arg.Expression));
        }

        return new AttributeDescriptor(
            type,
            positionalArguments,
            namedArguments);
    }

    public static object? GetLiteralValue(ExpressionSyntax expression) => expression switch
    {
        LiteralExpressionSyntax literal => literal.Token.Value,
        ArrayCreationExpressionSyntax arrayCreation => GetLiteralValue(arrayCreation),
        _ => throw new NotSupportedException(
            $"Obtaining literal value of expressions of type \"{expression.GetType().Name}\" is not supported.")
    };

    private static object GetLiteralValue(ArrayCreationExpressionSyntax expression)
    {
        if (expression.Type.RankSpecifiers.Count > 1)
        {
            throw new NotSupportedException(
                "Getting literal values of mutli-dimensional arrays is not supported.");
        }

        var rank = expression.Type.RankSpecifiers[0];

        if (rank.Sizes.Count > 1)
        {
            throw new NotSupportedException(
                "Getting literal values of mutli-dimensional arrays is not supported.");
        }

        Type itemType = expression.Type.ElementType switch
        {
            PredefinedTypeSyntax predefined => predefined switch
            {
                { Keyword.Text: "string" } => typeof(string),
                { Keyword.Text: "object" } => typeof(object),
                _ => throw new NotSupportedException($"Getting array literals of predefined type {predefined} is not supported.")
            },
            _ => throw new NotSupportedException($"Getting array literals of type {expression.Type.ElementType} is not supported.")
        };

        var size = rank.Sizes[0];

        int? arrayLength = size switch
        {
            OmittedArraySizeExpressionSyntax => null,
            LiteralExpressionSyntax literal => literal.Kind() == SyntaxKind.NumericLiteralExpression ?
                (int)GetLiteralValue(literal)! :
                throw new InvalidOperationException(),
            _ => throw new NotSupportedException()
        };

        return expression.Initializer is null ?
            CreateImmutableArray(itemType, arrayLength!.Value) :
            CreateImmutableArray(itemType, expression.Initializer.Expressions.Select(GetLiteralValue));
    }

    private static object CreateImmutableArray(Type itemType, IEnumerable<object?> values)
    {
        return typeof(SyntaxInterpreter)
            .GetMethod(nameof(CreateImmutableArrayFromValues), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(itemType)
            .Invoke(null, [values])!;
    }

    private static ImmutableArray<T> CreateImmutableArrayFromValues<T>(IEnumerable<object?> values) => 
        values.Cast<T>().ToImmutableArray();

    private static object CreateImmutableArray(Type itemType, int size)
    {
        return typeof(SyntaxInterpreter)
            .GetMethod(nameof(CreateImmutableArrayOfSize), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(itemType)
            .Invoke(null, [size])!;
    }

    private static ImmutableArray<T> CreateImmutableArrayOfSize<T>(int size) =>
        new T[size].ToImmutableArray();
}
