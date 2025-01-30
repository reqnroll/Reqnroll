using System;
using System.Linq;
using FluentAssertions;
using Reqnroll.Bindings;
using Reqnroll.Bindings.CucumberExpressions;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Bindings.Reflection;
using Reqnroll.Infrastructure;
using Xunit;

namespace Reqnroll.RuntimeTests.Bindings.CucumberExpressions
{

    public class CucumberExpressionParameterTypeRegistryTests
    {
        // Most of the logic in CucumberExpressionParameterTypeRegistry can only be tested in integration of a cucumber expression match,
        // so those are tested in the CucumberExpressionIntegrationTests class.

        private BindingRegistry _bindingRegistry;
        private CucumberExpressionParameterTypeRegistry CreateSut()
        {
            _bindingRegistry = new BindingRegistry();
            return new CucumberExpressionParameterTypeRegistry(_bindingRegistry);
        }

        [Fact]
        public void Should_provide_string_type()
        {
            var sut = CreateSut();
            var paramType = sut.LookupByTypeName("string");

            // The regex '.*' provided by the CucumberExpressionParameterTypeRegistry is fake and
            // will be ignored because of the special string type handling implemented in ReqnrollCucumberExpression.
            // See ReqnrollCucumberExpression.HandleStringType for detailed explanation.
            paramType.Should().NotBeNull();
            paramType.RegexStrings.Should().HaveCount(1);
            paramType.RegexStrings[0].Should().Be(".*");
        }

        public class SampleEnumUsingClass
        {
            public void MethodUsingSampleColorEnum1(SampleColorEnum color) { }
        }
        [Fact]
        public void Should_not_error_on_multiple_enums_of_the_same_name()
        {
            var sut = CreateSut();
            IBindingMethod enumUsingBindingMethod1 = new RuntimeBindingMethod(typeof(SampleEnumUsingClass).GetMethod(nameof(SampleEnumUsingClass.MethodUsingSampleColorEnum1)));
            sut.OnBindingMethodProcessed(enumUsingBindingMethod1);
            IBindingMethod enumUsingBindingMethod2 = new RuntimeBindingMethod(typeof(CucumberAddtionalExpressions.KlasWithCucumberExpressions).GetMethod(nameof(CucumberAddtionalExpressions.KlasWithCucumberExpressions.MethodUsingSampleColorEnum2)));
            sut.OnBindingMethodProcessed(enumUsingBindingMethod2);
            var paramTypes = sut.GetParameterTypes().Where(pt => pt.ParameterType.IsEnum).ToList();

            paramTypes.Should().HaveCount(2);
        }

        [Fact]
        public void ParameterTypeRegistry_should_identify_error_when_given_multiple_bindings_with_an_ambiguous_enum_as_a_parameter()
        {
            var expression = "I have {SampleColorEnum} cucumbers in my belly";
            var containerBuilder = new ContainerBuilder(new CucumberExpressionIntegrationTests.TestDependencyProvider());
            var globalContainer = containerBuilder.CreateGlobalContainer(GetType().Assembly);

            var bindingSourceProcessor = globalContainer.Resolve<IRuntimeBindingSourceProcessor>();

            var bindingRegistry = globalContainer.Resolve<IBindingRegistry>();

            // set up first method binding that uses an ambiguous enum parameter
            SetupBoundMethod(expression, bindingRegistry, bindingSourceProcessor, typeof(SampleEnumUsingClass), nameof(SampleEnumUsingClass.MethodUsingSampleColorEnum1));

            // set up second method binding that uses an ambiguous enum parameter
            SetupBoundMethod(expression, bindingRegistry, bindingSourceProcessor, typeof(CucumberAddtionalExpressions.KlasWithCucumberExpressions), nameof(CucumberAddtionalExpressions.KlasWithCucumberExpressions.MethodUsingSampleColorEnum2));

            bindingSourceProcessor.BuildingCompleted();

            bindingRegistry.IsValid.Should().BeFalse();
            var stepDefs = bindingRegistry.GetStepDefinitions().ToArray();
            stepDefs.Count().Should().Be(2);
            stepDefs.All(sd => sd.SourceExpression == expression).Should().BeTrue();
            stepDefs.All(sd => sd.IsValid == false).Should().BeTrue();
            stepDefs.All(sd => sd.ErrorMessage.StartsWith("Ambiguous enum")).Should().BeTrue();
        }

        [Fact]
        public void ParameterTypeRegistry_should_identify_error_when_given_multiple_bindings_with_an_ambiguous_type_as_a_parameter()
        {
            var expression = "a user {SampleUser} is registered";
            var containerBuilder = new ContainerBuilder(new CucumberExpressionIntegrationTests.TestDependencyProvider());
            var globalContainer = containerBuilder.CreateGlobalContainer(GetType().Assembly);

            var bindingSourceProcessor = globalContainer.Resolve<IRuntimeBindingSourceProcessor>();

            var bindingRegistry = globalContainer.Resolve<IBindingRegistry>();

            IStepArgumentTransformationBinding transformation1 = new StepArgumentTransformationBinding(
                "user ([A-Z][a-z]+)",
                new RuntimeBindingMethod(typeof(Reqnroll.RuntimeTests.Bindings.CucumberExpressions.SampleUser).GetMethod(nameof(Reqnroll.RuntimeTests.Bindings.CucumberExpressions.SampleUser.Create))));

            // set up first method binding that uses an ambiguous enum parameter
            SetupBoundMethod(expression, bindingRegistry, bindingSourceProcessor, typeof(CucumberExpressionIntegrationTests.SampleBindings), nameof(CucumberExpressionIntegrationTests.SampleBindings.StepDefWithCustomClassParam), transformation1);

            IStepArgumentTransformationBinding transformation2 = new StepArgumentTransformationBinding(
                "user ([A-Z][a-z]+)",
                new RuntimeBindingMethod(typeof(Reqnroll.RuntimeTests.Bindings.CucumberAddtionalExpressions.SampleUser).GetMethod(nameof(Reqnroll.RuntimeTests.Bindings.CucumberAddtionalExpressions.SampleUser.Create))));

            // set up second method binding that uses an ambiguous enum parameter
            SetupBoundMethod(expression, bindingRegistry, bindingSourceProcessor, typeof(CucumberAddtionalExpressions.KlasWithCucumberExpressions), nameof(CucumberAddtionalExpressions.KlasWithCucumberExpressions.MethodUsingSampleUser), transformation2);

            bindingSourceProcessor.BuildingCompleted();

            bindingRegistry.IsValid.Should().BeFalse();
            var stepDefs = bindingRegistry.GetStepDefinitions().ToArray();
            stepDefs.Count().Should().Be(2);
            stepDefs.All(sd => sd.SourceExpression == expression).Should().BeTrue();
            stepDefs.All(sd => sd.IsValid == false).Should().BeTrue();
            stepDefs.All(sd => sd.ErrorMessage.StartsWith("Ambiguous type used in cucumber expressions")).Should().BeTrue();
        }


        private static void SetupBoundMethod(string expression, IBindingRegistry bindingRegistry, IRuntimeBindingSourceProcessor bindingSourceProcessor, Type testType, string methodName, IStepArgumentTransformationBinding transformation = null)
        {
            if (transformation != null)
            {
                bindingRegistry.RegisterStepArgumentTransformationBinding(transformation);
            }
            var bindingSourceMethod = new BindingSourceMethod
            {
                BindingMethod = new RuntimeBindingMethod(testType.GetMethod(methodName)),
                IsPublic = true,
                Attributes = new[]
                {
                new BindingSourceAttribute
                {
                    AttributeType = new RuntimeBindingType(typeof(GivenAttribute)),
                    AttributeValues = new IBindingSourceAttributeValueProvider[]
                    {
                        new BindingSourceAttributeValueProvider(expression)
                    }
                }
            }
            };
            bindingSourceProcessor.ProcessType(
                new BindingSourceType
                {
                    BindingType = new RuntimeBindingType(typeof(CucumberExpressionIntegrationTests.SampleBindings)),
                    Attributes = new[]
                    {
                    new BindingSourceAttribute
                    {
                        AttributeType = new RuntimeBindingType(typeof(BindingAttribute))
                    }
                    },
                    IsPublic = true,
                    IsClass = true
                });
            bindingSourceProcessor.ProcessMethod(bindingSourceMethod);
        }
    }
}
namespace Reqnroll.RuntimeTests.Bindings.CucumberAddtionalExpressions
{
    public class KlasWithCucumberExpressions
    {
        public enum SampleColorEnum { Yellow, Brown };

        public void MethodUsingSampleColorEnum2(SampleColorEnum color) { }

        public void MethodUsingSampleUser(SampleUser user) { }
    }

    public class SampleUser
    {
        public string UserName { get; }

        public SampleUser(string userName)
        {
            UserName = userName;
        }
        public static SampleUser Create(string userName) => new(userName);
    }
}