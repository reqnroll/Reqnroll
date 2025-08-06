using System;
using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using FormattersTestLogger;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Moq;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FormatterLoggerTests : IDisposable
{
    private readonly FormatterLogger _sut;
    private readonly Mock<TestLoggerEvents> _testLoggerEventsMock;
    private readonly List<string> _environmentVariablesToCleanup;

    public FormatterLoggerTests()
    {
        _sut = new FormatterLogger();
        _testLoggerEventsMock = new Mock<TestLoggerEvents>();
        _environmentVariablesToCleanup = new List<string>();
    }

    public void Dispose()
    {
        // Clean up any environment variables that were set during tests
        foreach (var envVar in _environmentVariablesToCleanup)
        {
            Environment.SetEnvironmentVariable(envVar, null);
        }
    }

    [Fact]
    public void Initialize_Should_Set_Environment_Variable_With_Proper_JSON_Format()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", @"C:\TestResults" },
            { "name", "message" },
            { "outputFilePath", "reqnroll_report.ndjson" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        _sut.IsInitialized.Should().BeTrue();
        _sut.Parameters.Should().BeEquivalentTo(parameters);

        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        environmentVariableValue.Should().NotBeNullOrEmpty();

        // Parse and verify the JSON structure
        var jsonDocument = JsonDocument.Parse(environmentVariableValue);
        jsonDocument.RootElement.TryGetProperty("formatters", out var formattersElement).Should().BeTrue();
        formattersElement.TryGetProperty("message", out var messageElement).Should().BeTrue();
        messageElement.TryGetProperty("outputFilePath", out var outputFilePathElement).Should().BeTrue();
        outputFilePathElement.GetString().Should().Be(@"C:\TestResults\reqnroll_report.ndjson");
    }

    [Fact]
    public void Initialize_Should_Set_Environment_Variable_With_HTML_Formatter()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", @"C:\TestResults" },
            { "name", "html" },
            { "outputFilePath", "reqnroll_report.html" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_html";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        environmentVariableValue.Should().NotBeNullOrEmpty();

        // Parse and verify the JSON structure
        var jsonDocument = JsonDocument.Parse(environmentVariableValue);
        jsonDocument.RootElement.TryGetProperty("formatters", out var formattersElement).Should().BeTrue();
        formattersElement.TryGetProperty("html", out var htmlElement).Should().BeTrue();
        htmlElement.TryGetProperty("outputFilePath", out var outputFilePathElement).Should().BeTrue();
        outputFilePathElement.GetString().Should().Be(@"C:\TestResults\reqnroll_report.html");
    }

    [Fact]
    public void Initialize_Should_Handle_Missing_TestRunDirectory_Parameter()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "name", "message" },
            { "outputFilePath", "reqnroll_report.ndjson" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        environmentVariableValue.Should().NotBeNullOrEmpty();

        // Parse and verify the JSON structure
        var jsonDocument = JsonDocument.Parse(environmentVariableValue);
        jsonDocument.RootElement.TryGetProperty("formatters", out var formattersElement).Should().BeTrue();
        formattersElement.TryGetProperty("message", out var messageElement).Should().BeTrue();
        messageElement.TryGetProperty("outputFilePath", out var outputFilePathElement).Should().BeTrue();
        outputFilePathElement.GetString().Should().Be("reqnroll_report.ndjson");
    }

    [Fact]
    public void Initialize_Should_Use_LogFileName_When_OutputFilePath_Not_Present()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", @"C:\TestResults" },
            { "name", "message" },
            { "LogFileName", "alternate_report.ndjson" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        environmentVariableValue.Should().NotBeNullOrEmpty();

        // Parse and verify the JSON structure
        var jsonDocument = JsonDocument.Parse(environmentVariableValue);
        jsonDocument.RootElement.TryGetProperty("formatters", out var formattersElement).Should().BeTrue();
        formattersElement.TryGetProperty("message", out var messageElement).Should().BeTrue();
        messageElement.TryGetProperty("outputFilePath", out var outputFilePathElement).Should().BeTrue();
        outputFilePathElement.GetString().Should().Be(@"C:\TestResults\alternate_report.ndjson");
    }

    [Fact]
    public void Initialize_Should_Not_Set_Environment_Variable_When_Name_Is_Missing()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", @"C:\TestResults" },
            { "outputFilePath", "reqnroll_report.ndjson" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        _sut.IsInitialized.Should().BeTrue();
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        environmentVariableValue.Should().BeNull();
    }

    [Fact]
    public void Initialize_Should_Use_TestRunDirectory_When_OutputFilePath_Is_Missing()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", @"C:\TestResults" },
            { "name", "message" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        environmentVariableValue.Should().NotBeNullOrEmpty();

        // Parse and verify the JSON structure
        var jsonDocument = JsonDocument.Parse(environmentVariableValue);
        jsonDocument.RootElement.TryGetProperty("formatters", out var formattersElement).Should().BeTrue();
        formattersElement.TryGetProperty("message", out var messageElement).Should().BeTrue();
        messageElement.TryGetProperty("outputFilePath", out var outputFilePathElement).Should().BeTrue();
        outputFilePathElement.GetString().Should().Be(@"C:\TestResults");
    }

    [Fact]
    public void Initialize_Should_Create_Valid_JSON_With_Expected_Structure()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", @"C:\TestResults" },
            { "name", "message" },
            { "outputFilePath", "reqnroll_report.ndjson" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        
        // Verify it's valid JSON that matches the expected structure:
        // { "formatters": { "message": { "outputFilePath": "C:\TestResults\reqnroll_report.ndjson" } } }
        var expectedJson = """
                          {
                            "formatters": {
                              "message": {
                                "outputFilePath": "C:\\TestResults\\reqnroll_report.ndjson"
                              }
                            }
                          }
                          """;

        // Parse both actual and expected JSON to compare structure
        var actualJson = JsonDocument.Parse(environmentVariableValue);
        var expectedJsonDoc = JsonDocument.Parse(expectedJson);

        // Compare the structure
        actualJson.RootElement.GetProperty("formatters").GetProperty("message").GetProperty("outputFilePath").GetString()
            .Should().Be(@"C:\TestResults\reqnroll_report.ndjson");
    }
}