using System;
using CucumberExpressions;
using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Reflection;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings;

public class BindingFactoryTests
{
    [Theory]
    [InlineData(ExpressionType.Unspecified, typeof(RegularExpression))]
    [InlineData(ExpressionType.RegularExpression, typeof(RegularExpression))]
    [InlineData(ExpressionType.CucumberExpression, typeof(ReqnrollCucumberExpression))]
    public void CreateStepDefinitionBindingBuilderTest(ExpressionType expressionType, Type expected)
    {
        // Arrange
        var stepDefinitionRegexCalculator = new Mock<IStepDefinitionRegexCalculator>();
        var cucumberExpressionStepDefinitionBindingBuilderFactory = new CucumberExpressionStepDefinitionBindingBuilderFactory(new CucumberExpressionParameterTypeRegistry(Mock.Of<IBindingRegistry>()));
        var cucumberExpressionDetector = new CucumberExpressionDetector();
        var sut = new BindingFactory(stepDefinitionRegexCalculator.Object, cucumberExpressionStepDefinitionBindingBuilderFactory, cucumberExpressionDetector);

        var stepDefinitionType = StepDefinitionType.Given;
        var bindingMethod = new Mock<IBindingMethod>().Object;
        var bindingScope = new BindingScope("tag1", "feature1", "scenario1");
        var expressionString = "The product {string} has the price {int}$";

        // Act
        var result = sut.CreateStepDefinitionBindingBuilder(stepDefinitionType, bindingMethod, bindingScope, expressionString, expressionType).Build().ToList();

        // Assert
        result[0].Should().BeOfType<StepDefinitionBinding>().Which.Expression.Should().BeOfType(expected);
    }
}