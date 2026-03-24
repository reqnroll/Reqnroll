using FluentAssertions;
using Moq;
using Reqnroll.Configuration;
using Reqnroll.Configuration.JsonConfig;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FileBasedConfigurationResolverTests
{
    private readonly Mock<IConfigurationLoader> _configurationLoaderMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<IFormatterLog> _log;
    private readonly ReqnrollConfigConfigurationResolver _sut;

    public FileBasedConfigurationResolverTests()
    {
        _configurationLoaderMock = new Mock<IConfigurationLoader>();
        _log = new Mock<IFormatterLog>();


        _sut = new ReqnrollConfigConfigurationResolver(
            _configurationLoaderMock.Object,
            _log.Object
        );
    }

    [Fact]
    public void Resolve_Should_Return_Empty_Dictionary_When_Config_File_Has_No_Formatters()
    {
        // Arrange
        _configurationLoaderMock.Setup(x => x.Load(It.IsAny<ReqnrollConfiguration>())).Returns(ConfigurationLoader.GetDefault());
        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_Should_Return_Formatters_From_Valid_File()
    {
        // Arrange
        var reqnrollConfig = ConfigurationLoader.GetDefault();
        var jsonLoader = new JsonConfigurationLoader();
        reqnrollConfig = jsonLoader.LoadJson(reqnrollConfig, @"{
            ""formatters"": {
                ""formatter1"": {
                    ""config1"": ""setting1""
                },
                ""formatter2"": {
                    ""config2"": ""setting2""
                }
            }
        }");

        _configurationLoaderMock.Setup(x => x.Load(It.IsAny<ReqnrollConfiguration>())).Returns(reqnrollConfig);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(2);
        result["formatter1"].AdditionalSettings["config1"].Should().Be("setting1");
        result["formatter2"].AdditionalSettings["config2"].Should().Be("setting2");
    }

    [Fact]
    public void Resolve_Should_Return_An_EmptyEntry_When_Key_Has_no_Content()
    {
        // Arrange
        var reqnrollConfig = ConfigurationLoader.GetDefault();
        var jsonLoader = new JsonConfigurationLoader();
        reqnrollConfig = jsonLoader.LoadJson(reqnrollConfig, @"{
            ""formatters"": {
                ""emptyFormatter"": {}
                }
            }");

        _configurationLoaderMock.Setup(x => x.Load(It.IsAny<ReqnrollConfiguration>())).Returns(reqnrollConfig);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["emptyFormatter"].OutputFilePath.Should().BeNull();
        result["emptyFormatter"].AdditionalSettings.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_Should_Map_OutputFilePath()
    {
        // Arrange
        var reqnrollConfig = ConfigurationLoader.GetDefault();
        reqnrollConfig = new JsonConfigurationLoader().LoadJson(reqnrollConfig, @"{
            ""formatters"": {
                ""html"": { ""outputFilePath"": ""output/report.html"" }
            }
        }");
        _configurationLoaderMock.Setup(x => x.Load(It.IsAny<ReqnrollConfiguration>())).Returns(reqnrollConfig);

        // Act
        var result = _sut.Resolve();

        // Assert
        result["html"].OutputFilePath.Should().Be("output/report.html");
    }

    [Fact]
    public void Resolve_Should_Return_Empty_When_Loader_Returns_Null_Formatters()
    {
        // Arrange
        var config = ConfigurationLoader.GetDefault();
        config.Formatters = null;
        _configurationLoaderMock.Setup(x => x.Load(It.IsAny<ReqnrollConfiguration>())).Returns(config);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldMergeSettings_Should_Be_False()
    {
        _sut.ShouldMergeSettings.Should().BeFalse();
    }
}