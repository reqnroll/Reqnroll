using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll.Bindings.CucumberExpressions;

namespace Reqnroll.Specs.Bindings.CucumberExpressions;

[TestClass]
public class CucumberExpressionDetectorTests
{
    private CucumberExpressionDetector _sut;

    [TestInitialize]
    public void SetUp()
    {
        _sut = new CucumberExpressionDetector();
    }

    [TestMethod]
    [DataRow("a/b", true)]
    [DataRow("a\\/b", true)]
    [DataRow("^a/b", false)]
    public void IsCucumberExpressionTest(string expression, bool expected)
    {
        // Arrange

        // Act
        var result = _sut.IsCucumberExpression(expression);

        // Assert
        result.Should().Be(expected);
    }
}