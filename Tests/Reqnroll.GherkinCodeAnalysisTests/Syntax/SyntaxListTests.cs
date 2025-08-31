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
        var list = List(Enumerable.Range(0, count).Select(i => Example("Scenario", i.ToString())));

        list.Count.Should().Be(count);
    }

    [Fact]
    public void ToStringReturnsItemsInOrder()
    {
        var list = List([ 
            Example("Scenario", "One"),
            Example("Scenario", "Two")
        ]);

        list.ToString().Should().Be("Scenario:OneScenario:Two");
    }

    [Fact]
    public void GetEnumeratorReturnsListItemsInOrder()
    {
        var list = List([ 
            Example("Scenario", "One"),
            Example("Scenario", "Two"),
            Example("Scenario", "Three")
        ]);

        var enumerator = list.GetEnumerator();

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ToString().Should().Be("Scenario:One");
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ToString().Should().Be("Scenario:Two");
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ToString().Should().Be("Scenario:Three");
        enumerator.MoveNext().Should().BeFalse();
    }

    [Fact]
    public void GetEnumeratorReturnsListItemsInOrderEvenWhenBoxed()
    {
        var list = List([
            Example("Scenario", "One"),
            Example("Scenario", "Two"),
            Example("Scenario", "Three")
        ]);

        var enumerator = ((IEnumerable<ExampleSyntax>)list).GetEnumerator();

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ToString().Should().Be("Scenario:One");
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ToString().Should().Be("Scenario:Two");
        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.ToString().Should().Be("Scenario:Three");
        enumerator.MoveNext().Should().BeFalse();
    }
}
