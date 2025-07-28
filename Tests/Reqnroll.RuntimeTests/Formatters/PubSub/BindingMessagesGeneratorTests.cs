using FluentAssertions;
using Gherkin.CucumberMessages;
using Moq;
using Reqnroll.Bindings;
using Reqnroll.Formatters.PayloadProcessing.Cucumber;
using Reqnroll.Formatters.PubSub;
using System.Linq;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.PubSub;

public class BindingMessagesGeneratorTests
{
    private readonly Mock<IBindingRegistry> _bindingRegistryMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<IIdGenerator> _idGeneratorMock;
    private readonly BindingMessagesGenerator _sut;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private int _currentId = 0;

    public BindingMessagesGeneratorTests()
    {
        _bindingRegistryMock = new Mock<IBindingRegistry>();
        _idGeneratorMock = new Mock<IIdGenerator>();
        _idGeneratorMock.Setup(i => i.GetNewId()).Returns(_currentId++.ToString());
        _sut = new BindingMessagesGenerator(_idGeneratorMock.Object, new CucumberMessageFactory(), _bindingRegistryMock.Object);
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

        _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns([stepTransform.Object]);

        _bindingRegistryMock.SetupGet(br => br.Ready).Returns(true);
        // Act
        _sut.OnBindingRegistryReady(null, null);
        var messages = _sut.StaticBindingMessages.ToList();

        // Assert
        messages.Should().HaveCount(1);
    }

    [Fact]
    public void Should_Generate_UndefinedParameterType_Messages_For_Invalid_StepDefinitions()
    {
        // Arrange
        var invalidStepDefinition = new Mock<IStepDefinitionBinding>();
        invalidStepDefinition.Setup(sd => sd.IsValid).Returns(false);
        invalidStepDefinition.Setup(sd => sd.ErrorMessage).Returns("Undefined parameter type 'paramName'");
        invalidStepDefinition.Setup(sd => sd.SourceExpression).Returns("sourceExpression");

        _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns([invalidStepDefinition.Object]);

        _bindingRegistryMock.SetupGet(br => br.Ready).Returns(true);
        // Act
        _sut.OnBindingRegistryReady(null, null);
        var messages = _sut.StaticBindingMessages.ToList();

        // Assert
        messages.Should().HaveCount(1);
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

        _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns([validStepDefinition.Object]);

        _bindingRegistryMock.SetupGet(br => br.Ready).Returns(true);
        // Act
        _sut.OnBindingRegistryReady(null, null);
        var messages = _sut.StaticBindingMessages.ToList();

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

        _bindingRegistryMock.Setup(br => br.GetHooks()).Returns([hookBinding.Object]);
        _bindingRegistryMock.SetupGet(br => br.Ready).Returns(true);
        // Act
        _sut.OnBindingRegistryReady(null, null);
        var messages = _sut.StaticBindingMessages.ToList();

        // Assert
        messages.Should().HaveCount(1);
        _sut.StepDefinitionIdByBinding.Should().ContainKey(hookBinding.Object);
    }

    [Fact]
    public void Should_Not_Duplicate_Messages_For_Already_Processed_StepTransformations_And_Invalid_StepDefinitions()
    {
        // Arrange
        var stepTransform = new Mock<IStepArgumentTransformationBinding>();
        stepTransform.Setup(st => st.Name).Returns("TransformName");
        stepTransform.Setup(sd => sd.Method.Name).Returns("MethodName");
        stepTransform.Setup(sd => sd.Method.Type.AssemblyName).Returns("AssemblyName");
        stepTransform.Setup(sd => sd.Method.Type.FullName).Returns("TypeName");
        stepTransform.Setup(sd => sd.Method.Parameters).Returns([]);

        var invalidStepDefinition = new Mock<IStepDefinitionBinding>();
        invalidStepDefinition.Setup(sd => sd.IsValid).Returns(false);
        invalidStepDefinition.Setup(sd => sd.ErrorMessage).Returns("Undefined parameter type 'paramName'");
        invalidStepDefinition.Setup(sd => sd.SourceExpression).Returns("sourceExpression");

        _bindingRegistryMock.Setup(br => br.GetStepDefinitions()).Returns([invalidStepDefinition.Object, invalidStepDefinition.Object]);
        _bindingRegistryMock.Setup(br => br.GetStepTransformations()).Returns([stepTransform.Object, stepTransform.Object]);

        _bindingRegistryMock.SetupGet(br => br.Ready).Returns(true);
        // Act
        _sut.OnBindingRegistryReady(null, null);
        var messages = _sut.StaticBindingMessages.ToList();

        // Assert
        messages.Should().HaveCount(2);
    }
}