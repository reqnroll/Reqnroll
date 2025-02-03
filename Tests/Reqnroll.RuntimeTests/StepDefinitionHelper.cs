using FluentAssertions;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.RuntimeTests;

internal static class StepDefinitionHelper
{
    public static IStepDefinitionBinding CreateRegex(StepDefinitionType stepDefinitionType, string regex, IBindingMethod bindingMethod = null, BindingScope bindingScope = null)
    {
        bindingMethod ??= new Mock<IBindingMethod>().Object;
        var builder = new RegexStepDefinitionBindingBuilder(stepDefinitionType, bindingMethod, bindingScope, regex);
        var stepDefinitionBinding = builder.BuildSingle();
        stepDefinitionBinding.IsValid.Should().BeTrue($"the {nameof(CreateRegex)} method should create valid step definitions");
        return stepDefinitionBinding;
    }
}