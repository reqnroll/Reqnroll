using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using Reqnroll.Bindings.Discovery;
using Cucumber.TagExpressions;

namespace Reqnroll.RuntimeTests.Bindings.Discovery;

public class ReqnrollTagExpressionParserTests
{
    private ReqnrollTagExpressionParser CreateParser()
    {
        return new ReqnrollTagExpressionParser();
    }

    [Theory]
    [InlineData("tag1")]
    [InlineData("myTag")]
    [InlineData("feature")]
    public void Single_term_expressions_without_at_prefix_are_correctly_prefixed(string tagName)
    {
        // Arrange
        var parser = CreateParser();
        
        // Act
        var expression = parser.Parse(tagName);
        
        // Assert
        expression.Should().NotBeNull();
        expression.Evaluate(new[] { "@" + tagName }).Should().BeTrue("tag with @ prefix should match");
        expression.Evaluate(new[] { tagName }).Should().BeFalse("tag without @ prefix should not match");
    }

    [Theory]
    [InlineData("@tag1")]
    [InlineData("@myTag")]
    [InlineData("@feature")]
    public void Single_term_expressions_with_at_prefix_remain_unchanged(string tagName)
    {
        // Arrange
        var parser = CreateParser();
        
        // Act
        var expression = parser.Parse(tagName);
        
        // Assert
        expression.Should().NotBeNull();
        expression.Evaluate(new[] { tagName }).Should().BeTrue("tag with @ prefix should match");
    }


    [Theory]
    [InlineData("tag1 and tag2")]
    [InlineData("tag1 or tag2")]
    [InlineData("not tag1")]
    [InlineData("tag1 and @tag2")]
    [InlineData("@tag1 or tag2")]
    public void Multi_term_expressions_without_at_prefix_throw_exception(string expression)
    {
        // Arrange
        var parser = CreateParser();

        // Act
        Action act = () => parser.Parse(expression);

        // Assert
        act.Should().Throw<TagExpressionException>();
    }


    [Theory]
    [InlineData("@tag1 and @tag2", new[] { "@tag1", "@tag2" }, true)]
    [InlineData("@tag1 and @tag2", new[] { "@tag1" }, false)]
    [InlineData("@tag1 or @tag2", new[] { "@tag1" }, true)]
    [InlineData("@tag1 or @tag2", new[] { "@tag2" }, true)]
    [InlineData("@tag1 or @tag2", new[] { "@tag3" }, false)]
    [InlineData("not @tag1", new[] { "@tag1" }, false)]
    [InlineData("not @tag1", new[] { "@tag2" }, true)]
    public void Multi_term_expressions_with_at_prefix_work_correctly(string expression, string[] tags, bool expectedResult)
    {
        // Arrange
        var parser = CreateParser();
        
        // Act
        var parsedExpression = parser.Parse(expression);
        
        // Assert
        parsedExpression.Should().NotBeNull();
        parsedExpression.Evaluate(tags).Should().Be(expectedResult);
    }


    [Fact]
    public void Empty_tag_expression_returns_true_for_any_tags()
    {
        // Arrange
        var parser = CreateParser();
        
        // Act
        var expression = parser.Parse("");
        
        // Assert
        expression.Should().NotBeNull();
        expression.Evaluate(new[] { "@tag1" }).Should().BeTrue();
        expression.Evaluate(Array.Empty<string>()).Should().BeTrue();
    }

    [Fact]
    public void Null_or_empty_tag_names_are_handled_correctly()
    {
        // Arrange
        var parser = CreateParser();
        
        // Act & Assert - empty string should be handled
        var expression1 = parser.Parse("");
        expression1.Should().NotBeNull();
        
        // Null expression should be handled by underlying parser
        Action act = () => parser.Parse(null);
        act.Should().NotThrow();
    }

}
