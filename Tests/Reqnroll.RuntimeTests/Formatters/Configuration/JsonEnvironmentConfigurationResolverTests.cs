#nullable enable
using FluentAssertions;
using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class JsonEnvironmentConfigurationResolverTests
{
    private readonly Mock<IEnvironmentOptions> _environmentOptionsMock;
    private readonly JsonEnvironmentConfigurationResolver _sut;

    public JsonEnvironmentConfigurationResolverTests()
    {
        _environmentOptionsMock = new Mock<IEnvironmentOptions>();
        _sut = new JsonEnvironmentConfigurationResolver(_environmentOptionsMock.Object);
    }

    [Fact]
    public void Resolve_Should_Return_Empty_When_No_Environment_Variables_Are_Set()
    {
        // Arrange
        _environmentOptionsMock
            .Setup(e => e.FormattersJson)
            .Returns((string?)null);

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
        _environmentOptionsMock
            .Setup(e => e.FormattersJson)
            .Returns(json);

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
        _environmentOptionsMock
            .Setup(e => e.FormattersJson)
            .Returns(json);

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
        _environmentOptionsMock
            .Setup(e => e.FormattersJson)
            .Returns(json);

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
        
        _environmentOptionsMock
            .Setup(e => e.FormattersJson)
            .Returns(expectedJson);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().ContainKey("message");
        result["message"]["outputFilePath"].Should().Be("foo.ndjson");
        result.Should().HaveCount(1);
    }
}