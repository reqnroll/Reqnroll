using FluentAssertions;
using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration
{
    public class EnvironmentConfigurationResolverTests
    {
        private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock;
        private readonly EnvironmentConfigurationResolver _sut;

        public EnvironmentConfigurationResolverTests()
        {
            _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
            _sut = new EnvironmentConfigurationResolver(_environmentWrapperMock.Object);
        }

        [Fact]
        public void Resolve_Should_Return_Empty_When_No_Environment_Variables_Are_Set()
        {
            // Arrange
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable(It.IsAny<string>()))
                .Returns(new Failure<string>("Variable not set"));

            // Act
            var result = _sut.Resolve();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Resolve_Should_Return_Configuration_From_Environment_Variables()
        {
            // Arrange
            var json = """
            {
                "formatters": {
                    "formatter1": {
                        "configSetting1": "configValue1" }
                }
            }
            """;
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE))
                .Returns(new Success<string>(json));

            // Act
            var result = _sut.Resolve();

            // Assert
            result["formatter1"]["configSetting1"].Should().Be("configValue1");
        }

        [Fact]
        public void Resolve_Should_Return_MultipleConfigurations_From_Environment_Variables()
        {
            // Arrange
            var json = """
                {"formatters": {
                    "html": { "outputFilePath": "forHtml" }, 
                    "messages": { "outputFilePath": "forMessages" } 
                    }
                }
                """;
            _environmentWrapperMock
                .Setup(e => e.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE))
                .Returns(new Success<string>(json));

            // Act
            var result = _sut.Resolve();

            // Assert
            Assert.Equal(2, result.Count);
            var first = result["html"];
            var second = result["messages"];
            Assert.Equal("forHtml", first["outputFilePath"]);
            Assert.Equal("forMessages", second["outputFilePath"]);
        }

    }
}
