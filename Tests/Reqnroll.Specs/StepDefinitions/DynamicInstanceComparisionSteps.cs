using System.Collections.Generic;
using FluentAssertions;
using Reqnroll.Assist.Dynamic;

namespace Reqnroll.Specs.StepDefinitions;

[Binding]
public class DynamicInstanceComparisionSteps(State state)
{
    private DynamicInstanceComparisonException GetInstanceComparisonException()
    {
        var ex = state.CurrentException as DynamicInstanceComparisonException;
        ex.Should().NotBeNull();
        return ex!;
    }

    private void CheckForOneDifferenceContainingString(string expectedString)
    {
        var ex = GetInstanceComparisonException();
        var diffs = (List<string>)ex.Differences;
        var diff = diffs.Find(f => f.Contains(expectedString));
        diff.Should().NotBeNull();
    }

    [When("I compare it to this table")]
    public void ComparingAgainstDynamicInstance(Table table)
    {
        try
        {
            var org = (object)state.OriginalInstance;
            table.CompareToDynamicInstance(org);
        }
        catch (DynamicInstanceComparisonException ex)
        {
            state.CurrentException = ex;
        }
    }

    [Then("no instance comparison exception should have been thrown")]
    public void NoException()
    {
        state.CurrentException.Should().BeNull();
    }

    [Then(@"an instance comparison exception should be thrown with (\d+) differences")]
    [Then(@"an instance comparison exception should be thrown with (\d+) difference")]
    public void ExceptionShouldHaveBeenThrown(int expectedNumberOfDifferences)
    {
        state.CurrentException.Should().NotBeNull();
        var ex = GetInstanceComparisonException();
        ex.Differences.Should().HaveCount(expectedNumberOfDifferences);
    }

    [Then("one difference should be on the (.*) column of the table")]
    public void DifferenceOnTheColumnOfTheTable(string expectedColumnToDiffer)
    {
        CheckForOneDifferenceContainingString(expectedColumnToDiffer);
    }

    [Then("one difference should be on the (.*) field of the instance")]
    public void DifferenceOnFieldOfInstance(string expectedFieldToDiffer)
    {
        CheckForOneDifferenceContainingString(expectedFieldToDiffer);
    }

    [Then("one message should state that the instance had the value (.*)")]
    public void ExceptionMessageValueOnInstance(string expectedValueOfInstance)
    {
        CheckForOneDifferenceContainingString(expectedValueOfInstance);
    }

    [Then("one message should state that the table had the value (.*)")]
    public void ExceptionMessageValueInTable(string expectedValueOfTable)
    {
        CheckForOneDifferenceContainingString(expectedValueOfTable);
    }

    [Then("one difference should be on the (.*) property")]
    public void ExceptionMessageValueOnProperty(string expectedPropertyName)
    {
        CheckForOneDifferenceContainingString(expectedPropertyName);
    }

    [When("I compare it to this table using no type conversion")]
    public void WhenICompareItToThisTableUsingNoTypeConversion(Table table)
    {
        try
        {
            var org = (object)state.OriginalInstance;
            table.CompareToDynamicInstance(org, false);
        }
        catch (DynamicInstanceComparisonException ex)
        {
            state.CurrentException = ex;
        }
    }
}