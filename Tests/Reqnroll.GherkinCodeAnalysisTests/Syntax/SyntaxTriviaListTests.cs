using FluentAssertions;

namespace Reqnroll.CodeAnalysis.Gherkin.Syntax;

using static SyntaxFactory;

public class SyntaxTriviaListTests
{
    [Fact]
    public void CanCreateEmptyList()
    {
        var list = TriviaList();

        list.Should().HaveCount(0);
        list.Should().BeEmpty();

        list.Equals(default).Should().BeTrue();

        list.Invoking(list => list[0]).Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void CanCreateListFromSingleTrivia()
    {
        var item = Whitespace("    ");
        var list = TriviaList(item);

        list.Should().HaveCount(1);
        list.Should().HaveElementAt(0, item);

        list.Equals(list).Should().BeTrue();
    }

    [Fact]
    public void CanCreateListFromMultipleTrivia()
    {
        var item0 = Comment("# First");
        var item1 = Comment("# Second");
        var list = TriviaList([item0, item1]);

        list.Should().HaveCount(2);
        list[0].ToString().Should().Be("# First");
        list[1].ToString().Should().Be("# Second");

        list.Equals(list).Should().BeTrue();
    }
}
