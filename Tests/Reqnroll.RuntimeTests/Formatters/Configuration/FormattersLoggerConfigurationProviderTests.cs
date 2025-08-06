using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FormattersLoggerConfigurationProviderTests
{
    private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock;
    private readonly Mock<IFormatterLog> _formatterLogMock;
    private readonly FormattersLoggerConfigurationProvider _sut;

    public FormattersLoggerConfigurationProviderTests()
    {
        _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
        _formatterLogMock = new Mock<IFormatterLog>();
        _sut = new FormattersLoggerConfigurationProvider(_environmentWrapperMock.Object, _formatterLogMock.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_EnvironmentWrapper_Is_Null()
    {
        // Act & Assert
        var act = () => new FormattersLoggerConfigurationProvider(null, _formatterLogMock.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("environmentWrapper");
    }

    [Fact]
    public void Constructor_Should_Accept_Null_FormatterLog()
    {
        // Act & Assert
        var act = () => new FormattersLoggerConfigurationProvider(_environmentWrapperMock.Object, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Return_Empty_When_No_Environment_Variables_Found()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Failure<IEnumerable<string>>("No environment variables found"));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Return_Empty_When_Environment_Variables_List_Is_Empty()
    {
        // Arrange
        var emptyList = new List<string>();
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(emptyList));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Create_Resolver_For_Single_Formatter()
    {
        // Arrange
        var formatterNames = new List<string> { "REQNROLL_FORMATTERS_LOGGER_message" };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result.Should().HaveCount(1);
        result.Should().AllBeOfType<EnvironmentConfigurationResolver>();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Create_Resolvers_For_Multiple_Formatters()
    {
        // Arrange
        var formatterNames = new List<string> 
        { 
            "REQNROLL_FORMATTERS_LOGGER_message", 
            "REQNROLL_FORMATTERS_LOGGER_html",
            "REQNROLL_FORMATTERS_LOGGER_json"
        };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<EnvironmentConfigurationResolver>();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Skip_Null_Or_Empty_Formatter_Names()
    {
        // Arrange
        var formatterNames = new List<string> 
        { 
            "REQNROLL_FORMATTERS_LOGGER_message", 
            null,
            "",
            "   ",
            "REQNROLL_FORMATTERS_LOGGER_html"
        };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllBeOfType<EnvironmentConfigurationResolver>();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Pass_EnvironmentWrapper_To_Created_Resolvers()
    {
        // Arrange
        var formatterNames = new List<string> { "REQNROLL_FORMATTERS_LOGGER_message" };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Mock the GetEnvironmentVariable call that will be made by the EnvironmentConfigurationResolver
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariable("REQNROLL_FORMATTERS_LOGGER_message"))
            .Returns(new Success<string>("{}"));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();
        var resolver = result.First();

        // Trigger the resolver to use the environment wrapper
        resolver.Resolve();

        // Assert
        _environmentWrapperMock.Verify(e => e.GetEnvironmentVariable("REQNROLL_FORMATTERS_LOGGER_message"), Times.Once);
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Pass_FormatterLog_To_Created_Resolvers()
    {
        // Arrange
        var formatterNames = new List<string> { "REQNROLL_FORMATTERS_LOGGER_message" };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Mock the GetEnvironmentVariable call to return invalid JSON to trigger logging
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariable("REQNROLL_FORMATTERS_LOGGER_message"))
            .Returns(new Success<string>("invalid json"));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();
        var resolver = result.First();

        // Trigger the resolver to use the formatter log
        resolver.Resolve();

        // Assert
        _formatterLogMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("Failed to parse JSON"))), Times.Once);
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Handle_Realistic_Environment_Variable_Names()
    {
        // Arrange - Simulate realistic environment variable names as they would appear
        var formatterNames = new List<string> 
        { 
            "REQNROLL_FORMATTERS_LOGGER_message", 
            "REQNROLL_FORMATTERS_LOGGER_html",
            "REQNROLL_FORMATTERS_LOGGER_json",
            "REQNROLL_FORMATTERS_LOGGER_junit"
        };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result.Should().HaveCount(4);
        result.Should().AllBeOfType<EnvironmentConfigurationResolver>();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Return_Same_Results_On_Multiple_Calls()
    {
        // Arrange
        var formatterNames = new List<string> { "REQNROLL_FORMATTERS_LOGGER_message" };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Act
        var result1 = _sut.GetFormattersConfigurationResolvers();
        var result2 = _sut.GetFormattersConfigurationResolvers();

        // Assert
        result1.Should().HaveCount(1);
        result2.Should().HaveCount(1);
        // Note: The results should be equivalent in structure, but they are new instances each time
        result1.Should().AllBeOfType<EnvironmentConfigurationResolver>();
        result2.Should().AllBeOfType<EnvironmentConfigurationResolver>();
    }

    [Fact]
    public void GetFormattersConfigurationResolvers_Should_Use_Correct_Environment_Variable_Prefix()
    {
        // Arrange
        var formatterNames = new List<string> { "REQNROLL_FORMATTERS_LOGGER_test" };
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Success<IEnumerable<string>>(formatterNames));

        // Act
        var result = _sut.GetFormattersConfigurationResolvers();

        // Assert
        _environmentWrapperMock.Verify(
            e => e.GetEnvironmentVariableNames(FormattersConfigurationConstants.REQNROLL_FORMATTERS_LOGGER_ENVIRONMENT_VARIABLE_PREFIX), 
            Times.Once);
        result.Should().HaveCount(1);
    }
}