using NUnit.Framework;
using NUnit.Framework.Legacy;
using Reqnroll.Assist.Dynamic;

namespace AssistDynamic.Specs.Steps
{
  [Binding]
  public class DynamicSetComparisonSteps
  {
    private readonly State state;

    public DynamicSetComparisonSteps(State state) => this.state = state;

    private DynamicSetComparisonException GetSetComparisonException()
    {
      return this.state.CurrentException as DynamicSetComparisonException;
    }

    private void CheckForOneDifferenceContainingString(string expectedString)
    {
      var ex = GetSetComparisonException();
      var diffs = ((List<string>)ex.Differences);
      var diff = diffs.Find(f => f.Contains(expectedString));
      ClassicAssert.NotNull(diff);
    }

    [When(@"I compare the set to this table")]
    public void CompareSetToInstance(Table table)
    {
      try
      {
        table.CompareToDynamicSet(this.state.OriginalSet);
      }
      catch (DynamicSetComparisonException ex)
      {
        this.state.CurrentException = ex;
      }
    }

    [When(@"I compare the set to this table using no type conversion")]
    public void CompareSetToInstanceNoConversion(Table table)
    {
      try
      {
        table.CompareToDynamicSet(this.state.OriginalSet, false);
      }
      catch (DynamicSetComparisonException ex)
      {
        this.state.CurrentException = ex;
      }
    }

    [Then(@"no set comparison exception should have been thrown")]
    public void NoSetExceptionThrown()
    {
        ClassicAssert.IsNull(this.state.CurrentException);
    }

    [Then(@"an set comparison exception should be thrown")]
    public void SetComparisonExceptionThrown()
    {
        ClassicAssert.NotNull(GetSetComparisonException());
    }

    [Then(@"an set comparision exception should be thrown with (\d+) differences")]
    [Then(@"an set comparision exception should be thrown with (\d+) difference")]
    public void SetComparisionExceptionWithNumberOfDifferences(int expectedNumberOfDifference)
    {
      SetComparisonExceptionThrown();
      ClassicAssert.AreEqual(expectedNumberOfDifference, GetSetComparisonException().Differences.Count);
    }


    [Then(@"the error message for different rows should expect (.*) for table and (.*) for instance")]
    public void ShouldDifferInRowCount(string tableRowCountString, string instanceRowCountString)
    {
      var message = GetSetComparisonException().Message;
      ClassicAssert.IsTrue(message.Contains(tableRowCountString));
      ClassicAssert.IsTrue(message.Contains(instanceRowCountString));
    }

    [Then(@"one set difference should be on the (.*) column of the table")]
    public void DifferenceOnTheColumnOfTheTable(string expectedColumnToDiffer)
    {
      CheckForOneDifferenceContainingString(expectedColumnToDiffer);
    }

    [Then(@"one set difference should be on the (.*) field of the instance")]
    public void DifferenceOnFieldOfInstance(string expectedFieldToDiffer)
    {
      CheckForOneDifferenceContainingString(expectedFieldToDiffer);
    }

    [Then(@"(\d+) difference should be on row (\d+) on property '(.*)' for the values '(.*)' and '(.*)'")]
    public void DifferenceOnValue(int differenceNumber, int rowNumber, string expectedProperty, string instanceValue, string tableRowValue)
    {
      var exception = GetSetComparisonException();
      var difference = exception.Differences[differenceNumber - 1];
      CheckForOneDifferenceContainingString("'" + rowNumber + "'");
      CheckForOneDifferenceContainingString("'" + expectedProperty + "'");
    }
  }
}
