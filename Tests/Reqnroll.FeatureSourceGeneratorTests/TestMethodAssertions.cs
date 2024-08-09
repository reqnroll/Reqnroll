using FluentAssertions.Execution;
using Reqnroll.FeatureSourceGenerator.SourceModel;

namespace Reqnroll.FeatureSourceGenerator;

public class TestMethodAssertions(TestMethod? subject) :
    TestMethodAssertions<TestMethod?,TestMethodAssertions>(subject)
{
}

public class TestMethodAssertions<TSubject, TAssertions>(TestMethod? subject) :
    AttributeAssertions<TestMethod?, TAssertions>(subject)
    where TSubject: TestMethod?
    where TAssertions : TestMethodAssertions<TSubject,TAssertions>
{
    protected override string Identifier => "method";

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
            setup.WithExpectation("Expected {context:the subject} to have no parameters") :
            setup.WithExpectation("Expected {context:the subject} to have parameters equivalent to {0}", expected))
            .ForCondition(Subject is not null)
            .FailWith("but {context:the subject} is <null>.");

        var actual = Subject!.Parameters;

        if (expected.Count == 0 && actual.Length != 0)
        {
            assertion
                .Then
                .FailWith("but {context:the subject} has parameters defined");
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
                    .FailWith(" but {context:the subject} does not define a parameter {0}.", i);

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
                    .FailWith("but {context:the subject} has additional parameters {0}.", actual.Skip(expected.Count));
            }
        }

        return new AndConstraint<TAssertions>((TAssertions)this);
    }
}
