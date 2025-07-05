using FluentAssertions;
using Reqnroll.Bindings.CucumberExpressions;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.CucumberExpressions;


public class CucumberExpressionDetectorTests
{
    private readonly CucumberExpressionDetector _sut = new();

    [Theory]
    [InlineData("a/b", true)]
    [InlineData("a\\/b", true)]
    [InlineData("^a/b", false)]
    public void IsCucumberExpressionTest(string expression, bool expected)
    {
        // Arrange

        // Act
        var result = _sut.IsCucumberExpression(expression);

        // Assert
        result.Should().Be(expected);
    }
}