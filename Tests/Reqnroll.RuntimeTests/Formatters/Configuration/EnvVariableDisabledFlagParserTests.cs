using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class EnvVariableDisabledFlagParserTests
{
    private readonly Mock<IEnvironmentOptions> _environmentOptionsMock;
    private readonly FormattersDisabledOverrideProvider _sut;

    public EnvVariableDisabledFlagParserTests()
    {
        _environmentOptionsMock = new Mock<IEnvironmentOptions>();
        _sut = new FormattersDisabledOverrideProvider(_environmentOptionsMock.Object);
    }

    [Fact]
    public void Disabled_Should_Return_True_When_Environment_Variable_Is_Set_To_True()
    {
        // Arrange
        _environmentOptionsMock
            .Setup(e => e.FormattersDisabled)
            .Returns(true);

        // Act
        var result = _sut.Disabled();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Disabled_Should_Return_False_When_Environment_Variable_Is_Set_To_False()
    {
        // Arrange
        _environmentOptionsMock
            .Setup(e => e.FormattersDisabled)
            .Returns(false);

        // Act
        var result = _sut.Disabled();

        // Assert
        Assert.False(result);
    }
}