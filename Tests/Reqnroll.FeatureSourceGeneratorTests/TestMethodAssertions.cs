using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Reqnroll.FeatureSourceGenerator;
public class TestMethodAssertions(TestMethod? subject) :
    TestMethodAssertions<TestMethodAssertions>(subject)
{
}

public class TestMethodAssertions<TAssertions>(TestMethod? subject) :
    ReferenceTypeAssertions<TestMethod, TAssertions>(subject!)
    where TAssertions : TestMethodAssertions<TAssertions>
{
    protected override string Identifier => "method";

    ///// <summary>
    ///// Expects the class declaration have only a single attribute with a specific identifier.
    ///// </summary>
    ///// <param name="type">
    ///// The declared type of the attribute.
    ///// </param>
    ///// <param name="because">
    ///// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
    ///// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
    ///// </param>
    ///// <param name="becauseArgs">
    ///// Zero or more objects to format using the placeholders in <paramref name="because" />.
    ///// </param>
    //public AndWhichConstraint<TAssertions, AttributeSyntax> HaveSingleAttribute(
    //    string type,
    //    string because = "",
    //    params object[] becauseArgs)
    //{
    //    var expectation = "Expected {context:method} to have a single attribute " +
    //        $"which is of type \"{type}\"{{reason}}";

    //    bool notNull = Execute.Assertion
    //        .BecauseOf(because, becauseArgs)
    //        .ForCondition(Subject is not null)
    //        .FailWith(expectation + ", but found <null>.");

    //    AttributeSyntax? match = default;

    //    if (notNull)
    //    {
    //        var attributes = Subject!.Attributes;

    //        switch (attributes.Length)
    //        {
    //            case 0: // Fail, Collection is empty
    //                Execute.Assertion
    //                    .BecauseOf(because, becauseArgs)
    //                    .FailWith(expectation + ", but the class has no attributes.");

    //                break;
    //            case 1: // Success Condition
    //                var single = attributes.Single();

    //                if (single.Name.ToString() != type)
    //                {
    //                    Execute.Assertion
    //                        .BecauseOf(because, becauseArgs)
    //                        .FailWith(expectation + ", but found the attribute \"{0}\".", single.Name);
    //                }
    //                else
    //                {
    //                    match = single;
    //                }

    //                break;
    //            default: // Fail, Collection contains more than a single item
    //                Execute.Assertion
    //                    .BecauseOf(because, becauseArgs)
    //                    .FailWith(expectation + ", but found {0}.", attributes);

    //                break;
    //        }
    //    }

    //    return new AndWhichConstraint<TAssertions, AttributeSyntax>((TAssertions)this, match!);
    //}

    //public AndWhichConstraint<TAssertions, AttributeSyntax> HaveAttribute(
    //    string type,
    //    string because = "",
    //    params object[] becauseArgs)
    //{
    //    var expectation = "Expected {context:method} to have an attribute " +
    //        $"of type \"{type}\"{{reason}}";

    //    bool notNull = Execute.Assertion
    //        .BecauseOf(because, becauseArgs)
    //        .ForCondition(Subject is not null)
    //        .FailWith(expectation + ", but found <null>.");

    //    AttributeSyntax? match = default;

    //    if (notNull)
    //    {
    //        var attributes = Subject!.AttributeLists.SelectMany(list => list.Attributes).ToList();

    //        if (attributes.Count == 0)
    //        {
    //            Execute.Assertion
    //                .BecauseOf(because, becauseArgs)
    //                .FailWith(expectation + ", but the method has no attributes.");
    //        }
    //        else
    //        {
    //            match = attributes.FirstOrDefault(attribute => attribute.Name.ToString() == type);

    //            if (match == null)
    //            {
    //                Execute.Assertion
    //                    .BecauseOf(because, becauseArgs)
    //                    .FailWith(expectation + ", but found {0}.", attributes);
    //            }
    //        }
    //    }

    //    return new AndWhichConstraint<TAssertions, AttributeSyntax>((TAssertions)this, match!);
    //}

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
        var actual = Subject!.Attributes;


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
                    .FailWith("but {context:the method} is missing attributes {0} and has extra attributes {1}", missing, extra);
            }
            else
            {
                assertion
                    .Then
                    .FailWith("but {context:the method} is missing attributes {0}", missing);
            }
        }
        else if (extra.Count > 0)
        {
            assertion
                .Then
                .FailWith("but {context:the method} has extra attributes {0}", extra);
        }

        return new AndConstraint<TAssertions>((TAssertions)this);
    }

    public AndConstraint<TAssertions> HaveNoParameters(
        string because = "",
        params object[] becauseArgs) =>
        HaveParametersEquivalentTo([], because, becauseArgs);

    public AndConstraint<TAssertions> HaveParametersEquivalentTo(
        ParameterDescriptor[] expectation,
        string because = "",
        params object[] becauseArgs) => 
        HaveParametersEquivalentTo((IEnumerable<ParameterDescriptor>)expectation, because, becauseArgs);

    public AndConstraint<TAssertions> HaveParametersEquivalentTo(
        IEnumerable<ParameterDescriptor> expectation,
        string because = "",
        params object[] becauseArgs)
    {
        using var scope = new AssertionScope();

        scope.FormattingOptions.UseLineBreaks = true;

        var setup = Execute.Assertion
            .BecauseOf(because, becauseArgs);

        var expected = expectation.ToList();

        var assertion = (expected.Count == 0 ?
            setup.WithExpectation("Expected {context:the method} to have no parameters") :
            setup.WithExpectation("Expected {context:the method} to have parameters equivalent to {0}", expected))
            .ForCondition(Subject is not null)
            .FailWith("but {context:the method} is <null>.");

        var actual = Subject!.Parameters;

        if (expected.Count == 0 && actual.Length != 0)
        {
            assertion
                .Then
                .FailWith("but {context:the method} has parameters defined");
        }
        else
        {
            for (var i = 0; i < expected.Count; i++)
            {
                var expectedItem = expected[i];
                var itemAssertion = Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .WithExpectation("Expected parameter {0} to be \"{1}\" ", i, expectedItem)
                    .ForCondition(actual.Length > i)
                    .FailWith(" but {context:the method} does not define a parameter {0}.", i);

                if (actual.Length >= i)
                {
                    var actualItem = actual[i];

                    itemAssertion
                        .Then
                        .ForCondition(actualItem.Equals(expectedItem))
                        .FailWith("but found \"{0}\".", actual[i]);
                }
            }

            if (actual.Length > expected.Count)
            {
                assertion
                    .Then
                    .FailWith("but {context:the method} has additional parameters {0}.", actual.Skip(expected.Count));
            }
        }

        return new AndConstraint<TAssertions>((TAssertions)this);
    }

    //public AndConstraint<TAssertions> HaveParametersEquivalentTo(
    //    IEnumerable<ParameterDescriptor> expectation,
    //    string because = "",
    //    params object[] becauseArgs)
    //{
    //    using var scope = new AssertionScope();

    //    scope.FormattingOptions.UseLineBreaks = true;

    //    Execute.Assertion
    //        .BecauseOf(because, becauseArgs)
    //        .WithExpectation("Expected {context:the method} to have parameters equivalent to {0} {reason}", expectation)
    //        .ForCondition(Subject is not null)
    //        .FailWith("but {context:the method} is <null>.");

    //    var expected = expectation.ToList();
    //    var actual = Subject!.ParameterList.Parameters;

    //    for (var i = 0; i < expected.Count; i++)
    //    {
    //        var expectedItem = expected[i];

    //        Execute.Assertion
    //            .BecauseOf(because, becauseArgs)
    //            .WithExpectation("Expected {context:the method} to have parameter {0}: \"{1}\" {reason}", i+1, expectedItem)
    //            .ForCondition(Subject.ParameterList.Parameters.Count != 0)
    //            .FailWith("but {context:the method} has no parameters defined.", Subject.ParameterList.Parameters.Count)
    //            .Then
    //            .ForCondition(Subject.ParameterList.Parameters.Count > i)
    //            .FailWith("but {context:the method} only has {0} parameters defined.", Subject.ParameterList.Parameters.Count)
    //            .Then
    //            .Given(() => Subject.ParameterList.Parameters[i])
    //            .ForCondition(actual => false)// actual.IsEquivalentTo(expectedItem, true))
    //            .FailWith("but found \"{0}\".", expectedItem);
    //    }

    //    if (expected.Count < actual.Count)
    //    {
    //        Execute.Assertion
    //            .BecauseOf(because, becauseArgs)
    //            .WithExpectation("Expected {context:the method} to have parameters equivalent to {0}{reason}", expectation)
    //            .FailWith("but {context:the method} has extra parameters {0}.", actual.Skip(expected.Count));
    //    }

    //    return new AndConstraint<TAssertions>((TAssertions)this);
    //}
}
