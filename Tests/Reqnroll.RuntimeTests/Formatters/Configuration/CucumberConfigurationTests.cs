using Moq;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class CucumberConfigurationTests
{
    private readonly Mock<IFormattersConfigurationDisableOverrideProvider> _disableOverrideProviderMock;
    private readonly Mock<IFileBasedConfigurationResolver> _fileResolverMock;
    private readonly Mock<IJsonEnvironmentConfigurationResolver> _jsonEnvironmentResolverMock;
    private readonly Mock<IKeyValueEnvironmentConfigurationResolver> _keyValueEnvironmentResolverMock;
    private readonly Mock<IVariableSubstitutionService> _variableSubstitutionServiceMock;   
    private readonly FormattersConfigurationProvider _sut;

    public CucumberConfigurationTests()
    {
        _disableOverrideProviderMock = new Mock<IFormattersConfigurationDisableOverrideProvider>();
        _fileResolverMock = new Mock<IFileBasedConfigurationResolver>();
        _jsonEnvironmentResolverMock = new Mock<IJsonEnvironmentConfigurationResolver>();
        _keyValueEnvironmentResolverMock = new Mock<IKeyValueEnvironmentConfigurationResolver>();
        _variableSubstitutionServiceMock = new Mock<IVariableSubstitutionService>();

        // Setup ShouldMergeSettings for each resolver
        _fileResolverMock.Setup(r => r.ShouldMergeSettings).Returns(false);
        _jsonEnvironmentResolverMock.Setup(r => r.ShouldMergeSettings).Returns(false);
        _keyValueEnvironmentResolverMock.Setup(r => r.ShouldMergeSettings).Returns(true);

        // Default empty returns
        _fileResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, FormatterConfiguration>());
        _jsonEnvironmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, FormatterConfiguration>());
        _keyValueEnvironmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, FormatterConfiguration>());

        _sut = new FormattersConfigurationProvider(
            _fileResolverMock.Object,
            _jsonEnvironmentResolverMock.Object,
            _keyValueEnvironmentResolverMock.Object,
            _disableOverrideProviderMock.Object,
            _variableSubstitutionServiceMock.Object);
    }

    [Fact]
    public void Enabled_Should_Return_False_When_No_Configuration_Is_Resolved()
    {
        // Arrange
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.Enabled;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Enabled_Should_Respect_Environment_Variable_Override()
    {
        // Arrange
        var mockedSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration { AdditionalSettings = new Dictionary<string, object> { { "outputFileName", @"c:\html\html_report.html" } } } }
        };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);

        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(true);

        // Act
        var result = _sut.Enabled;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFormatterConfiguration_Should_Return_Configuration_For_Existing_Formatter()
    {
        // Arrange
        var mockedSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration { AdditionalSettings = new Dictionary<string, object> { { "outputFileName", @"c:\html\html_report.html" } } } }
        };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);


        // Act
        var result = _sut.GetFormatterConfiguration("html");

        // Assert
        Assert.Equal(@"c:\html\html_report.html", result.AdditionalSettings["outputFileName"]);
    }

    [Fact]
    public void GetFormatterConfiguration_JsonEnvVar_Should_Replace_Entire_Configuration()
    {
        // Arrange - Config file has outputFilePath and setting1
        var configFileSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration 
                { 
                    OutputFilePath = @"c:\html\report.html",
                    AdditionalSettings = new Dictionary<string, object> { { "setting1", "fileValue" } } 
                } 
            }
        };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(configFileSetup);

        // JSON environment variable (ShouldMergeSettings=false) REPLACES entirely
        var envVarSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration 
                { 
                    AdditionalSettings = new Dictionary<string, object> { { "setting1", "envValue" } } 
                } 
            }
        };
        _jsonEnvironmentResolverMock.Setup(r => r.Resolve()).Returns(envVarSetup);
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfiguration("html");

        // Assert - JSON env var REPLACES, so outputFilePath should be LOST
        Assert.Equal("envValue", result.AdditionalSettings["setting1"]);
        Assert.Null(result.OutputFilePath); // Replaced entirely, so outputFilePath is gone
    }

    [Fact]
    public void GetFormatterConfiguration_KeyValueEnvVar_Should_Merge_Settings_Not_Replace()
    {
        // Arrange - Config file has outputFilePath and setting1
        var configFileSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration 
                { 
                    OutputFilePath = @"c:\html\report.html",
                    AdditionalSettings = new Dictionary<string, object> { { "setting1", "fileValue" } } 
                } 
            }
        };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(configFileSetup);

        // KeyValue environment variable (ShouldMergeSettings=true) MERGES
        var keyValueSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration 
                { 
                    AdditionalSettings = new Dictionary<string, object> { { "setting1", "envValue" } } 
                } 
            }
        };
        _keyValueEnvironmentResolverMock.Setup(r => r.Resolve()).Returns(keyValueSetup);
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfiguration("html");

        // Assert - KeyValue env var MERGES, so outputFilePath should be PRESERVED
        Assert.Equal("envValue", result.AdditionalSettings["setting1"]);
        Assert.Equal(@"c:\html\report.html", result.OutputFilePath); // Merged, so outputFilePath is preserved!
    }

    [Fact]
    public void GetFormatterConfiguration_KeyValueEnvVar_Should_Override_OutputFilePath_When_Specified()
    {
        // Arrange - Config file has outputFilePath
        var configFileSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration { OutputFilePath = @"c:\html\original.html" } }
        };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(configFileSetup);

        // KeyValue environment variable specifies a different outputFilePath
        var keyValueSetup = new Dictionary<string, FormatterConfiguration>
        {
            { "html", new FormatterConfiguration { OutputFilePath = @"c:\html\overridden.html" } }
        };
        _keyValueEnvironmentResolverMock.Setup(r => r.Resolve()).Returns(keyValueSetup);
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfiguration("html");

        // Assert - outputFilePath should be overridden
        Assert.Equal(@"c:\html\overridden.html", result.OutputFilePath);
    }


    [Fact]
    public void GetFormatterConfiguration_Should_Return_Null_For_Nonexistent_Formatter()
    {
        // Arrange
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfiguration("nonexistent");

        // Assert
        Assert.Null(result);
    }
}