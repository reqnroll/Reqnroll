using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.Bindings;
using Reqnroll.Bindings.Reflection;

namespace Reqnroll.RuntimeTests.Bindings
{
    
    public class BindingRegistryTests
    {
        [Fact]
        public void GetStepDefinitions_should_return_all_step_definitions()
        {
            var sut = new BindingRegistry();

            var stepDefinitionBinding1 = StepDefinitionHelper.CreateRegex(StepDefinitionType.Given, @"foo.*");
            var stepDefinitionBinding2 = StepDefinitionHelper.CreateRegex(StepDefinitionType.When, @"bar.*");
            sut.RegisterStepDefinitionBinding(stepDefinitionBinding1);
            sut.RegisterStepDefinitionBinding(stepDefinitionBinding2);

            var result = sut.GetStepDefinitions();

            result.Should().BeEquivalentTo(new List<IStepDefinitionBinding> { stepDefinitionBinding1, stepDefinitionBinding2 });
        }

        [Fact]
        public void GetHooks_should_return_all_hooks()
        {
            var sut = new BindingRegistry();

            var hook1 = new HookBinding(new Mock<IBindingMethod>().Object, HookType.BeforeScenario, null, 1);
            var hook2 = new HookBinding(new Mock<IBindingMethod>().Object, HookType.AfterFeature, null, 2);
            sut.RegisterHookBinding(hook1);
            sut.RegisterHookBinding(hook2);

            var result = sut.GetHooks();

            result.Should().BeEquivalentTo(new List<HookBinding> { hook1, hook2 });
        }
        
        [Fact]
        public void GetStepTransformations_should_return_all_step_transformers_in_correct_order()
        {
            var sut = new BindingRegistry();

            var sat1 = new StepArgumentTransformationBinding(string.Empty, new Mock<IBindingMethod>().Object);
            var sat2 = new StepArgumentTransformationBinding(string.Empty, new Mock<IBindingMethod>().Object);
            var sat3 = new StepArgumentTransformationBinding(string.Empty, new Mock<IBindingMethod>().Object, order: 1);
            var sat4 = new StepArgumentTransformationBinding(string.Empty, new Mock<IBindingMethod>().Object, null, 2);

            sut.RegisterStepArgumentTransformationBinding(sat4);
            sut.RegisterStepArgumentTransformationBinding(sat1);
            sut.RegisterStepArgumentTransformationBinding(sat3);
            sut.RegisterStepArgumentTransformationBinding(sat2);

            var result = sut.GetStepTransformations();

            result.Should().BeEquivalentTo(new List<StepArgumentTransformationBinding> { sat1, sat2, sat3, sat4 });
        }
    }
}
