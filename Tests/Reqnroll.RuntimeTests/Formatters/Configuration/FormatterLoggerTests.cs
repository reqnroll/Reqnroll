using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

    // OS-aware test paths
    private readonly string _testRunDirectory;
    private readonly string _expectedOutputPath;

    public FormatterLoggerTests()
    {
        _sut = new FormatterLogger();
        _testLoggerEventsMock = new Mock<TestLoggerEvents>();
        _environmentVariablesToCleanup = new List<string>();

        // Create OS-appropriate test paths
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _testRunDirectory = @"C:\TestResults";
        }
        else
        {
            _testRunDirectory = "/tmp/TestResults";
        }

        _expectedOutputPath = Path.Combine(_testRunDirectory, "reqnroll_report.ndjson");
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
            { "testRunDirectory", _testRunDirectory },
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
        outputFilePathElement.GetString().Should().Be(_expectedOutputPath);
    }

    [Fact]
    public void Initialize_Should_Set_Environment_Variable_With_HTML_Formatter()
    {
        // Arrange
        var expectedHtmlOutputPath = Path.Combine(_testRunDirectory, "reqnroll_report.html");
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", _testRunDirectory },
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
        outputFilePathElement.GetString().Should().Be(expectedHtmlOutputPath);
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
        var expectedAlternateOutputPath = Path.Combine(_testRunDirectory, "alternate_report.ndjson");
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", _testRunDirectory },
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
        outputFilePathElement.GetString().Should().Be(expectedAlternateOutputPath);
    }

    [Fact]
    public void Initialize_Should_Not_Set_Environment_Variable_When_Name_Is_Missing()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", _testRunDirectory },
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
            { "testRunDirectory", _testRunDirectory },
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
        outputFilePathElement.GetString().Should().Be(_testRunDirectory);
    }

    [Fact]
    public void Initialize_Should_Create_Valid_JSON_With_Expected_Structure()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            { "testRunDirectory", _testRunDirectory },
            { "name", "message" },
            { "outputFilePath", "reqnroll_report.ndjson" }
        };

        var expectedEnvironmentVariableName = "REQNROLL_FORMATTERS_LOGGER_message";
        _environmentVariablesToCleanup.Add(expectedEnvironmentVariableName);

        // Act
        _sut.Initialize(_testLoggerEventsMock.Object, parameters);

        // Assert
        var environmentVariableValue = Environment.GetEnvironmentVariable(expectedEnvironmentVariableName);
        
        // Verify it's valid JSON that matches the expected structure
        var actualJson = JsonDocument.Parse(environmentVariableValue);

        // Compare the structure - the actual path will be OS-appropriate
        actualJson.RootElement.GetProperty("formatters").GetProperty("message").GetProperty("outputFilePath").GetString()
            .Should().Be(_expectedOutputPath);
    }
}