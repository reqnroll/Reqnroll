using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration
{
    public class EnvVariableEnableFlagParserTests
    {
        private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock;
        private readonly EnvVariableEnableFlagParser _sut;

        public EnvVariableEnableFlagParserTests()
        {
            _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
            _sut = new EnvVariableEnableFlagParser(_environmentWrapperMock.Object);
        }

        [Fact]
        public void Parse_Should_Return_True_When_Environment_Variable_Is_Set_To_True()
        {
            // Arrange
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENABLE_ENVIRONMENT_VARIABLE))
                .Returns(new Success<string>("true"));

            // Act
            var result = _sut.Parse();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Parse_Should_Return_False_When_Environment_Variable_Is_Set_To_False()
        {
            // Arrange
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENABLE_ENVIRONMENT_VARIABLE))
                .Returns(new Success<string>("false"));

            // Act
            var result = _sut.Parse();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Parse_Should_Return_True_When_Environment_Variable_Is_Not_Set()
        {
            // Arrange
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable(It.IsAny<string>()))
                .Returns(new Failure<string>(It.IsAny<string>()));

            // Act
            var result = _sut.Parse();

            // Assert
            Assert.True(result);
        }
    }
}
