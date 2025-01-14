using System;
using FluentAssertions;
using Reqnroll.Assist.Dynamic;

namespace Reqnroll.Specs.StepDefinitions;

[Binding]
public class DynamicInstanceCreationSteps(State state)
{
    [Given("I create a dynamic instance from this table")]
    [When("I create a dynamic instance from this table")]
    public void CreateDynamicInstanceFromTable(Table table)
    {
        state.OriginalInstance = table.CreateDynamicInstance();
    }

    [Then("the Name property should equal '(.*)'")]
    public void NameShouldBe(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.Name;
        actual.Should().Be(expectedValue);
    }

    [Then(@"the Age property should equal (\d+)")]
    public void AgeShouldBe(int expectedAge)
    {
        var actualAge = (int)state.OriginalInstance.Age;
        actualAge.Should().Be(expectedAge);
    }

    [Then(@"the age property should equal (\d+)")]
    public void LowerCaseAgeShouldBe(int expectedAge)
    {
        var actualAge = (int)state.OriginalInstance.age;
        actualAge.Should().Be(expectedAge);
    }

    [Then("the BirthDate property should equal (.*)")]
    public void BirthDateShouldBe(string expectedDate)
    {
        var expected = DateTime.Parse(expectedDate);
        var actual = (DateTime)state.OriginalInstance.BirthDate;
        actual.Should().Be(expected);
    }

    [Then(@"the LengthInMeters property should equal '(\d+\.\d+)'")]
    public void LengthInMeterShouldBe(double expectedLengthInMeters)
    {
        CheckLengthInMeters(expectedLengthInMeters);
    }

    [Then(@"the MolecularWeight property should equal '(\d+\.\d+)'")]
    public void MolecularWeightShouldBe(decimal expectedMolecularWeight)
    {
        CheckMolecularWeight(expectedMolecularWeight);
    }

    private void CheckLengthInMeters(double expectedLengthInMeters)
    {
        var actual = (double)state.OriginalInstance.LengthInMeters;
        actual.Should().Be(expectedLengthInMeters);
    }

    private void CheckMolecularWeight(decimal expectedMolecularWeight)
    {
        var actual = (decimal)state.OriginalInstance.MolecularWeight;
        actual.Should().Be(expectedMolecularWeight);
    }

    [Then(@"the SATScore should be (\d+)")]
    public void SatTest(int expectedScore)
    {
        var actual = (int)state.OriginalInstance.SATScore;
        actual.Should().Be(expectedScore);
    }

    [Then("the IsDeveloper property should equal '(.*)'")]
    public void ThenTheIsDeveloperPropertyShouldEqualTrueAndBeOfTypeBool(bool expectedValue)
    {
        var actual = (bool)state.OriginalInstance.IsDeveloper;
        actual.Should().Be(expectedValue);
    }

    [Then("the CharpNmeWithStrangeChars property should equal '(.*)'")]
    public void ThenTheCharpNmeWithStrangeCharsPropertyShouldEqual(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.CharpNmeWithStrangeChars;
        actual.Should().Be(expectedValue);
    }

    [Then("the My_Nice_Variable property should equal '(.*)'")]
    public void ThenTheMy_Nice_VariablePropertyShouldEqual(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.My_Nice_Variable;
        actual.Should().Be(expectedValue);
    }

    [Then("the MyVariableNeedsCleanUp property should equal '(.*)'")]
    public void ThenTheMyVariableNeedsCleanUpPropertyShouldEqual(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.MyVariableNeedsCleanUp;
        actual.Should().Be(expectedValue);
    }

    [When("I create a dynamic instance with only reserved chars")]
    public void OnlyReservedChars(Table table)
    {
        try
        {
            state.OriginalInstance = table.CreateDynamicInstance();
        }
        catch (DynamicInstanceFromTableException ex)
        {
            state.CurrentException = ex;
        }
    }

    [Then("an exception with a nice error message about the property only containing reserved chars should be thrown")]
    public void ThenAnExceptionWithANiceErrorMessageAboutThePropertyOnlyContainingReservedCharsShouldBeThrown()
    {
        var ex = state.CurrentException as DynamicInstanceFromTableException;
        ex.Should().NotBeNull();
        ex?.Message.Should().Contain("only contains");
    }

    [Given("I create a dynamic instance from this table using no type conversion")]
    [When("I create a dynamic instance from this table using no type conversion")]
    public void WhenICreateADynamicInstanceFromThisTableUsingNoTypeConversion(Table table)
    {
        state.OriginalInstance = table.CreateDynamicInstance(false);
    }

    [Then("the Name value should still be '(.*)'")]
    public void ThenTheNameValueShouldStillBe(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.Name;
        actual.Should().Be(expectedValue);
    }

    [Then("the Age value should still be '(.*)'")]
    public void ThenTheAgeValueShouldStillBe(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.Age;
        actual.Should().Be(expectedValue);
    }

    [Then("the birth date should still be '(.*)'")]
    public void ThenTheBirthDateShouldStillBe(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.BirthDate;
        actual.Should().Be(expectedValue);
    }

    [Then("length in meter should still be '(.*)'")]
    public void ThenLengthInMeterShouldStillBe(string expectedValue)
    {
        var actual = (string)state.OriginalInstance.LengthInMeters;
        actual.Should().Be(expectedValue);
    }
}