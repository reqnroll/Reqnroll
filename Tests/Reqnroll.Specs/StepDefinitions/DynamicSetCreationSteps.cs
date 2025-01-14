using System;
using System.Linq;
using FluentAssertions;
using Reqnroll.Assist.Dynamic;

namespace Reqnroll.Specs.StepDefinitions;

[Binding]
public class DynamicSetCreationSteps(State state)
{
    private dynamic GetItem(int itemNumber)
    {
        return state.OriginalSet[itemNumber - 1];
    }


    [Given("I create a set of dynamic instances from this table")]
    [When("I create a set of dynamic instances from this table")]
    public void WithMethodBinding(Table table)
    {
        state.OriginalSet = table.CreateDynamicSet().ToList();
    }

    [Given("I create a set of dynamic instances from this table using no type conversion")]
    public void WithMethodBindingNoTypeConversion(Table table)
    {
        state.OriginalSet = table.CreateDynamicSet(false).ToList();
    }


    [Then(@"I should have a list of (\d+) dynamic objects")]
    public void ShouldContain(int expectedNumberOfItems)
    {

        var actualNumberOfItems = state.OriginalSet.Count;
        actualNumberOfItems.Should().Be(expectedNumberOfItems);
    }

    [Then(@"the (\d+) item should have BirthDate equal to '(.*)'")]
    public void ItemInSetShouldHaveExpectedBirthDate(int itemNumber, string expectedBirthDate)
    {
        DateTime actualBirthDate = GetItem(itemNumber).BirthDate;
        actualBirthDate.Should().Be(DateTime.Parse(expectedBirthDate));
    }

    [Then(@"the (\d+) item should have Age equal to '(\d+)'")]
    public void ItemInSetShouldHaveExpectedAge(int itemNumber, int expectedAge)
    {
        int actualAge = GetItem(itemNumber).Age;
        actualAge.Should().Be(expectedAge);
    }

    [Then("the (.*) item should still Name equal '(.*)'")]
    public void ThenTheItemShouldStillNameEqual(int itemNumber, string expectedName)
    {
        string actualName = GetItem(itemNumber).Name;
        actualName.Should().Be(expectedName);
    }

    [Then("the (.*) item should still Age equal '(.*)'")]
    public void ThenTheItemShouldStillAgeEqual(int itemNumber, string expectedAge)
    {
        string actualAge = GetItem(itemNumber).Age;
        actualAge.Should().Be(expectedAge);
    }


    [Then(@"the (\d+) item should have Name equal to '(.*)'")]
    public void ItemInSetShouldHaveExpectedName(int itemNumber, string expectedName)
    {
        string actualName = GetItem(itemNumber).Name;
        actualName.Should().Be(expectedName);
    }

    [Then(@"the (\d+) item should have LengthInMeters equal to '(\d+\.\d+)'")]
    public void ItemInSetShouldHaveExpectedLengthInMeters(int itemNumber, double expectedLengthInMetersItem)
    {
        double actualLengthInMetersItem = GetItem(itemNumber).LengthInMeters;
        actualLengthInMetersItem.Should().Be(expectedLengthInMetersItem);
    }

    [When("I create a set of dynamic instances from this table using no type conversion")]
    public void WhenICreateASetOfDynamicInstancesFromThisTableUsingNoTypeConversion(Table table)
    {
        state.OriginalSet = table.CreateDynamicSet(false).ToList();
    }
}