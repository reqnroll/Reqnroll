namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class SyntaxListTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(8)]
    [InlineData(13)]
    [InlineData(21)]
    public void CountReturnsCorrectNumberOfItems(int count)
    {
        var list = List(Enumerable.Range(0, count).Select(i => Scenario("Scenario", LiteralText(i.ToString()))));

        list.Count.Should().Be(count);
    }

    [Fact]
    public void ToStringReturnsItemsInOrder()
    {
        var list = List([ 
            Scenario("Scenario", LiteralText("One")),
            Scenario("Scenario", LiteralText("Two"))]);

        list.ToString().Should().Be("Scenario:OneScenario:Two");
    }
}
