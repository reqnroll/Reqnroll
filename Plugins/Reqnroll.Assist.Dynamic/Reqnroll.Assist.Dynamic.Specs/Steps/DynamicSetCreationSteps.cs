using NUnit.Framework;
using NUnit.Framework.Legacy;
using Reqnroll.Assist.Dynamic;

namespace AssistDynamic.Specs.Steps
{
  [Binding]
  public class DynamicSetCreationSteps
  {
    private readonly State state;

    public DynamicSetCreationSteps(State state) => this.state = state;
    private dynamic GetItem(int itemNumber)
    {
      return this.state.OriginalSet[itemNumber - 1];
    }


    [Given(@"I create a set of dynamic instances from this table")]
    [When(@"I create a set of dynamic instances from this table")]
    public void WithMethodBInding(Table table)
    {
      this.state.OriginalSet = table.CreateDynamicSet().ToList();
    }

    [Given(@"I create a set of dynamic instances from this table using no type conversion")]
    public void WithMethodBIndingNoTypeConversion(Table table)
    {
      this.state.OriginalSet = table.CreateDynamicSet(false).ToList();
    }


    [Then(@"I should have a list of (\d+) dynamic objects")]
    public void ShouldContain(int expectedNumberOfItems)
    {
        ClassicAssert.AreEqual(expectedNumberOfItems, this.state.OriginalSet.Count);
    }

    [Then(@"the (\d+) item should have BirthDate equal to '(.*)'")]
    public void ItemInSetShouldHaveExpectedBirthDate(int itemNumber, string expectedBirthDate)
    {
        ClassicAssert.AreEqual(DateTime.Parse(expectedBirthDate), GetItem(itemNumber).BirthDate);
    }

    [Then(@"the (\d+) item should have Age equal to '(\d+)'")]
    public void ItemInSetShouldHaveExpectedAge(int itemNumber, int expectedAge)
    {
        ClassicAssert.AreEqual(expectedAge, GetItem(itemNumber).Age);
    }

    [Then(@"the (.*) item should still Name equal '(.*)'")]
    public void ThenTheItemShouldStillNameEqual(int itemNumber, string expectedName)
    {
        ClassicAssert.AreEqual(expectedName, GetItem(itemNumber).Name);
    }

    [Then(@"the (.*) item should still Age equal '(.*)'")]
    public void ThenTheItemShouldStillAgeEqual(int itemNumber, string expectedAge)
    {
      ClassicAssert.AreEqual(expectedAge, GetItem(itemNumber).Age);
    }


    [Then(@"the (\d+) item should have Name equal to '(.*)'")]
    public void ItemInSetShouldHaveExpectedName(int itemNumber, string expectedName)
    {
      ClassicAssert.AreEqual(expectedName, GetItem(itemNumber).Name);
    }

    [Then(@"the (\d+) item should have LengthInMeters equal to '(\d+\.\d+)'")]
    public void ItemInSetShouldHaveExpectedLenghtInMeters(int itemNumber, double expectedLengthInMetersItem)
    {
      ClassicAssert.AreEqual(expectedLengthInMetersItem, GetItem(itemNumber).LengthInMeters);
    }

    [When(@"I create a set of dynamic instances from this table using no type conversion")]
    public void WhenICreateASetOfDynamicInstancesFromThisTableUsingNoTypeConversion(Table table)
    {
      this.state.OriginalSet = table.CreateDynamicSet(false).ToList();
    }
  }
}
