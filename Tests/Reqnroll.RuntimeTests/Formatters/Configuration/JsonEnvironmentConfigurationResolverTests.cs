using FluentAssertions;
using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class JsonEnvironmentConfigurationResolverTests
{
    private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock;
    private readonly JsonEnvironmentConfigurationResolver _sut;

    public JsonEnvironmentConfigurationResolverTests()
    {
        _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
        _sut = new JsonEnvironmentConfigurationResolver(_environmentWrapperMock.Object);
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
    public void Resolve_should_return_configuration_with_case_insensitive_formatter_and_setting_names()
    {
        // Arrange
        var json = """
                   {
                       "formatters": {
                           "FORMATTER1": {
                               "CONFIGSetting1": "configValue1" }
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
                       "message": { "outputFilePath": "forMessages" } 
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
        var second = result["message"];
        Assert.Equal("forHtml", first["outputFilePath"]);
        Assert.Equal("forMessages", second["outputFilePath"]);
    }

    [Fact]
    public void Resolve_Should_Parse_JSON_Format_Environment_Variable()
    {
        // Arrange
        var expectedJson = """
                          {
                              "formatters": {
                                  "message": {
                                      "outputFilePath": "foo.ndjson"
                                  }
                              }
                          }
                          """;
        
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariable(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE))
            .Returns(new Success<string>(expectedJson));

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().ContainKey("message");
        result["message"]["outputFilePath"].Should().Be("foo.ndjson");
        result.Should().HaveCount(1);
    }
}