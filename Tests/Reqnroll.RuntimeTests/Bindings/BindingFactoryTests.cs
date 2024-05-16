using System;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings;

public class BindingFactoryTests
{
    private readonly IBindingMethod _bindingMethod = new Mock<IBindingMethod>().Object;
    private readonly BindingScope _bindingScope = new BindingScope("testTag", "testTitle", "TestScenario");

    private static BindingFactory CreateSut() => new(
        new Mock<IStepDefinitionRegexCalculator>().Object,
        new Mock<ICucumberExpressionStepDefinitionBindingBuilderFactory>().Object,
        ConfigurationLoader.GetDefault());

    [Fact]
    public void CreateHookBinding_Passes_Arguments()
    {
        var sut = CreateSut();

        const int hookOrder = 0;
        const HookType hookType = HookType.AfterFeature;

        var hookBinding = sut.CreateHookBinding(_bindingMethod, hookType, _bindingScope, hookOrder);

        Assert.Equal(_bindingMethod, hookBinding.Method);
        Assert.Equal(hookType, hookBinding.HookType);
        Assert.Equal(_bindingScope, hookBinding.BindingScope);
        Assert.Equal(hookOrder, hookBinding.HookOrder);
    }

    [Fact]
    public void CreateStepArgumentTransformation_Passes_Arguments()
    {
        var sut = CreateSut();

        const string regexString = "^testRegexString$";
        const string parameterTypeName = "testParameterTypeName";

        var stepArgumentTransformation = sut.CreateStepArgumentTransformation(regexString, _bindingMethod, parameterTypeName);

        Assert.Equal(_bindingMethod, stepArgumentTransformation.Method);
        Assert.Equal(regexString, stepArgumentTransformation.Regex.ToString());
        Assert.Equal(parameterTypeName, stepArgumentTransformation.Name);
    }

    [Theory]
    [InlineData(null, typeof(MethodNameStepDefinitionBindingBuilder), true)]
    [InlineData(null, typeof(MethodNameStepDefinitionBindingBuilder), false)]
    [InlineData("I have {int} cucumber(s) in my belly/tummy", typeof(CucumberExpressionStepDefinitionBindingBuilder), true)]
    [InlineData("I have {int} cucumber(s) in my belly/tummy", typeof(RegexStepDefinitionBindingBuilder), false)]
    [InlineData("I have (\\d+) cucumber(?:s)? in my (?:belly|tummy)", typeof(RegexStepDefinitionBindingBuilder), true)]
    [InlineData("I have (\\d+) cucumber(?:s)? in my (?:belly|tummy)", typeof(RegexStepDefinitionBindingBuilder), false)]
    public void CreateStepDefinitionBindingBuilder_ReturnsBindingTypeBuilder_BasedOnInput(string expressionString, Type expectedBindingType, bool enableCucumberStepDefinitionBindings)
    {
        var cucumberExpressionStepDefinitionBindingBuilderFactory = new Mock<ICucumberExpressionStepDefinitionBindingBuilderFactory>();
        var reqnrollConfiguration = ConfigurationLoader.GetDefault();

        var sut = new BindingFactory(
            new Mock<IStepDefinitionRegexCalculator>().Object,
            cucumberExpressionStepDefinitionBindingBuilderFactory.Object,
            reqnrollConfiguration);

        reqnrollConfiguration.EnableCucumberStepDefinitionBindings = enableCucumberStepDefinitionBindings;

        const StepDefinitionType stepDefinitionType = StepDefinitionType.When;

        cucumberExpressionStepDefinitionBindingBuilderFactory.Setup(m => m.Create(stepDefinitionType, _bindingMethod, _bindingScope, expressionString))
                                                             .Returns(new CucumberExpressionStepDefinitionBindingBuilder(default, default, default, default, default));

        var stepDefinitionBindingBuilder = sut.CreateStepDefinitionBindingBuilder(stepDefinitionType, _bindingMethod, _bindingScope, expressionString);

        Assert.Equal(expectedBindingType, stepDefinitionBindingBuilder.GetType());
    }
}
