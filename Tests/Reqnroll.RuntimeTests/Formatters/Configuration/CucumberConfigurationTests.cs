using Moq;
using Reqnroll.Formatters.Configuration;
using System.Collections.Generic;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class CucumberConfigurationTests
{
    private readonly Mock<IFormattersConfigurationDisableOverrideProvider> _disableOverrideProviderMock;
    private readonly Mock<IFileBasedConfigurationResolver> _fileResolverMock;
    private readonly Mock<IJsonEnvironmentConfigurationResolver> _environmentResolverMock;
    private readonly FormattersConfigurationProvider _sut;

    public CucumberConfigurationTests()
    {
        _disableOverrideProviderMock = new Mock<IFormattersConfigurationDisableOverrideProvider>();
        _fileResolverMock = new Mock<IFileBasedConfigurationResolver>();
        _environmentResolverMock = new Mock<IJsonEnvironmentConfigurationResolver>();
        var keyValueEnvironmentConfigurationResolverMock = new Mock<IKeyValueEnvironmentConfigurationResolver>();
        keyValueEnvironmentConfigurationResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());

        _sut = new FormattersConfigurationProvider(
            _fileResolverMock.Object,
            _environmentResolverMock.Object,
            keyValueEnvironmentConfigurationResolverMock.Object,
            _disableOverrideProviderMock.Object);
    }

    [Fact]
    public void Enabled_Should_Return_False_When_No_Configuration_Is_Resolved()
    {
        // Arrange
        _fileResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());
        _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());
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
        var mockedSetup = new Dictionary<string, IDictionary<string, object>>();
        var htmlConfig = new Dictionary<string, object> { { "outputFileName", @"c:\html\html_report.html" } };
        mockedSetup.Add("html", htmlConfig);
        _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);
        _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());

        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(true);

        // Act
        var result = _sut.Enabled;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFormatterConfigurationByName_Should_Return_Configuration_For_Existing_Formatter()
    {
        // Arrange
        var mockedSetup = new Dictionary<string, IDictionary<string, object>> { { "html", new Dictionary<string, object> { { "outputFileName", @"c:\html\html_report.html" } } } };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(mockedSetup);
        _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfigurationByName("html");

        // Assert
        Assert.Equal(@"c:\html\html_report.html", result["outputFileName"]);
    }

    [Fact]
    public void GetFormatterConfigurationByName_Should_Respect_Formatter_Given_By_EnvironmentVariable_Override()
    {
        // Arrange
        var configFileSetup = new Dictionary<string, IDictionary<string, object>> { { "html", new Dictionary<string, object> { { "outputFileName", @"c:\html\html_report.html" } } } };
        _fileResolverMock.Setup(r => r.Resolve()).Returns(configFileSetup);

        var envVarSetup = new Dictionary<string, IDictionary<string, object>> { { "html", new Dictionary<string, object> { { "outputFileName", @"c:\html\html_overridden_name.html" } } } };
        _environmentResolverMock.Setup(r => r.Resolve()).Returns(envVarSetup);
        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfigurationByName("html");

        // Assert
        Assert.Equal(@"c:\html\html_overridden_name.html", result["outputFileName"]);
    }


    [Fact]
    public void GetFormatterConfigurationByName_Should_Return_Null_For_Nonexistent_Formatter()
    {
        // Arrange
        _fileResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());
        _environmentResolverMock.Setup(r => r.Resolve()).Returns(new Dictionary<string, IDictionary<string, object>>());

        _disableOverrideProviderMock.Setup(p => p.Disabled()).Returns(false);

        // Act
        var result = _sut.GetFormatterConfigurationByName("nonexistent");

        // Assert
        Assert.Null(result);
    }
}