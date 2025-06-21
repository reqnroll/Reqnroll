using FluentAssertions;
using Gherkin.CucumberMessages;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.PubSub
{
    public class BindingMessagesGeneratorTests
    {
        private readonly Mock<IBindingRegistry> _bindingRegistryMock;
        private readonly Mock<IIdGenerator> _idGeneratorMock;
        private readonly BindingMessagesGenerator _sut;
        private int CurrentId = 0;

        public BindingMessagesGeneratorTests()
        {
            _bindingRegistryMock = new Mock<IBindingRegistry>();
            _idGeneratorMock = new Mock<IIdGenerator>();
            _idGeneratorMock.Setup(i => i.GetNewId()).Returns(CurrentId++.ToString());
            _sut = new BindingMessagesGenerator(_idGeneratorMock.Object, new CucumberMessageFactory());
        }

        [Fact]
        public void Should_Generate_ParameterType_Messages_For_StepTransformations()
        {
            // Arrange
            var stepTransform = new Mock<IStepArgumentTransformationBinding>();
            stepTransform.Setup(st => st.Name).Returns("TransformName");
            stepTransform.Setup(sd => sd.Method.Name).Returns("MethodName");
            stepTransform.Setup(sd => sd.Method.Type.AssemblyName).Returns("AssemblyName");
            stepTransform.Setup(sd => sd.Method.Type.FullName).Returns("TypeName");
            stepTransform.Setup(sd => sd.Method.Parameters).Returns([]);

            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new[] { stepTransform.Object });

            // Act
            var messages = _sut.PopulateBindingCachesAndGenerateBindingMessages(_bindingRegistryMock.Object).ToList();

            // Assert
            messages.Should().HaveCount(1);
            _sut.StepArgumentTransformCache.Should().Contain(stepTransform.Object);
        }

        [Fact]
        public void Should_Generate_UndefinedParameterType_Messages_For_Invalid_StepDefinitions()
        {
            // Arrange
            var invalidStepDefinition = new Mock<IStepDefinitionBinding>();
            invalidStepDefinition.Setup(sd => sd.IsValid).Returns(false);
            invalidStepDefinition.Setup(sd => sd.ErrorMessage).Returns("Undefined parameter type 'paramName'");
            invalidStepDefinition.Setup(sd => sd.SourceExpression).Returns("sourceExpression");

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new[] { invalidStepDefinition.Object });

            // Act
            var messages = _sut.PopulateBindingCachesAndGenerateBindingMessages(_bindingRegistryMock.Object).ToList();

            // Assert
            messages.Should().HaveCount(1);
            _sut.UndefinedParameterTypeBindingsCache.Should().Contain(invalidStepDefinition.Object);
        }

        [Fact]
        public void Should_Generate_StepDefinition_Messages_For_Valid_StepDefinitions()
        {
            // Arrange
            var validStepDefinition = new Mock<IStepDefinitionBinding>();
            validStepDefinition.Setup(sd => sd.IsValid).Returns(true);
            validStepDefinition.Setup(sd => sd.SourceExpression).Returns("sourceExpression");
            validStepDefinition.Setup(sd => sd.Method.Name).Returns("MethodName");
            validStepDefinition.Setup(sd => sd.Method.Type.AssemblyName).Returns("AssemblyName");
            validStepDefinition.Setup(sd => sd.Method.Type.FullName).Returns("TypeName");
            validStepDefinition.Setup(sd => sd.Method.Parameters).Returns([]);

            _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns(new[] { validStepDefinition.Object });

            // Act
            var messages = _sut.PopulateBindingCachesAndGenerateBindingMessages(_bindingRegistryMock.Object).ToList();

            // Assert
            messages.Should().HaveCount(1);
            _sut.StepDefinitionIdByBinding.Should().HaveCount(1);
            _sut.StepDefinitionIdByBinding.Should().ContainKey(validStepDefinition.Object);
            _sut.StepDefinitionIdByBinding[validStepDefinition.Object].Should().Be("0");
        }

        [Fact]
        public void Should_Generate_Hook_Messages_For_HookBindings()
        {
            // Arrange
            var hookBinding = new Mock<IHookBinding>();
            hookBinding.Setup(hb => hb.HookType).Returns(HookType.BeforeScenario);
            hookBinding.Setup(sd => sd.Method.Name).Returns("MethodName");
            hookBinding.Setup(sd => sd.Method.Type.AssemblyName).Returns("AssemblyName");
            hookBinding.Setup(sd => sd.Method.Type.FullName).Returns("TypeName");
            hookBinding.Setup(sd => sd.Method.Parameters).Returns([]);

            _bindingRegistryMock.Setup(br => br.GetHooks()).Returns(new[] { hookBinding.Object });
            // Act
            var messages = _sut.PopulateBindingCachesAndGenerateBindingMessages(_bindingRegistryMock.Object).ToList();

            // Assert
            messages.Should().HaveCount(1);
            _sut.StepDefinitionIdByBinding.Should().ContainKey(hookBinding.Object);
        }

        [Fact]
        public void Should_Not_Duplicate_Messages_For_Already_Cached_Items()
        {
            // Arrange
            var stepTransform = new Mock<IStepArgumentTransformationBinding>();
            stepTransform.Setup(st => st.Name).Returns("TransformName");

            _sut.StepArgumentTransformCache.Add(stepTransform.Object);

            _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns(new[] { stepTransform.Object });

            // Act
            var messages = _sut.PopulateBindingCachesAndGenerateBindingMessages(_bindingRegistryMock.Object).ToList();

            // Assert
            messages.Should().BeEmpty();
        }
    }
}
