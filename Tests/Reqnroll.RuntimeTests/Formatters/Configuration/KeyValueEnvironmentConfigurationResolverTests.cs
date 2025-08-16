using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class KeyValueEnvironmentConfigurationResolverTests
{
    private readonly Mock<IEnvironmentWrapper> _environmentWrapperMock;
    private readonly Mock<IFormatterLog> _formatterLogMock;
    private readonly KeyValueEnvironmentConfigurationResolver _sut;
    private const string SampleFormatterName = "sample";
    private const string SampleFormatterEnvironmentName = FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX + SampleFormatterName;

    public KeyValueEnvironmentConfigurationResolverTests()
    {
        _environmentWrapperMock = new Mock<IEnvironmentWrapper>();
        _formatterLogMock = new Mock<IFormatterLog>();
        _sut = new KeyValueEnvironmentConfigurationResolver(_environmentWrapperMock.Object, _formatterLogMock.Object);
    }

    [Fact]
    public void Constructor_Should_Throw_ArgumentNullException_When_EnvironmentWrapper_Is_Null()
    {
        // Act & Assert
        var act = () => new KeyValueEnvironmentConfigurationResolver(null, _formatterLogMock.Object);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("environmentWrapper");
    }

    [Fact]
    public void Constructor_Should_Accept_Null_FormatterLog()
    {
        // Act & Assert
        var act = () => new KeyValueEnvironmentConfigurationResolver(_environmentWrapperMock.Object);
        act.Should().NotThrow();
    }

    [Fact]
    public void Resolve_Should_Return_Empty_When_Environment_Variables_List_Is_Empty()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(new Dictionary<string, string>());

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_should_ignore_formatter_with_empty_settings()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_should_configure_single_formatter_with_true_setting()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "true" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(SampleFormatterName)
              .WhoseValue.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_should_disable_formatter_with_false_setting()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "false" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(SampleFormatterName)
              .WhoseValue.Should().BeNull();
    }

    [Fact]
    public void Resolve_should_throw_exception_with_invalid_settings()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "foo" }
                });

        // Act
        FluentActions.Invoking(() => _sut.Resolve())
                     .Should()
                     .Throw<ReqnrollException>()
                     .WithMessage("*'foo'*");
    }

    [Fact]
    public void Resolve_should_configure_single_formatter_with_output_file_settings()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "outputFilePath=foo.txt" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(SampleFormatterName)
              .WhoseValue.Should().Contain("outputFilePath", "foo.txt");
    }

    [Fact]
    public void Resolve_should_configure_single_formatter_with_case_insensitive_settings()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "OUTPUTFilePath=foo.txt" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(SampleFormatterName)
              .WhoseValue.Should().Contain("outputFilePath", "foo.txt");
    }

    [Fact]
    public void Resolve_should_configure_single_formatter_with_multiple_settings()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "setting1=value1;setting2=value2" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(SampleFormatterName)
              .WhoseValue.Should().Contain("setting1", "value1")
              .And.Contain("setting2", "value2");
    }

    [Fact]
    public void Resolve_should_trim_whitespaces_in_settings()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { SampleFormatterEnvironmentName, "  setting1  =  value1  ;   setting2  =  value2  " }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result.Should().ContainKey(SampleFormatterName)
              .WhoseValue.Should().Contain("setting1", "value1")
              .And.Contain("setting2", "value2");
    }

    [Fact]
    public void Resolve_should_configure_multiple_formatters()
    {
        // Arrange
        _environmentWrapperMock
            .Setup(e => e.GetEnvironmentVariables(FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX))
            .Returns(
                new Dictionary<string, string>
                {
                    { FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX + "f1", "outputFilePath=foo.txt" },
                    { FormattersConfigurationConstants.REQNROLL_FORMATTERS_ENVIRONMENT_VARIABLE_PREFIX + "f2", "outputFilePath=bar.txt" }
                });

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey("f1")
              .WhoseValue.Should().Contain("outputFilePath", "foo.txt");
        result.Should().ContainKey("f2")
              .WhoseValue.Should().Contain("outputFilePath", "bar.txt");
    }
}