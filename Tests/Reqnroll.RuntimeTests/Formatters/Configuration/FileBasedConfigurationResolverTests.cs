using FluentAssertions;
using Moq;
using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.Configuration;

public class FileBasedConfigurationResolverTests
{
    private readonly Mock<IReqnrollJsonLocator> _jsonLocatorMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<IFileService> _fileServiceMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<IFormatterLog> _log;
    private readonly FileBasedConfigurationResolver _sut;

    public FileBasedConfigurationResolverTests()
    {
        _jsonLocatorMock = new Mock<IReqnrollJsonLocator>();
        _fileSystemMock = new Mock<IFileSystem>();
        _fileServiceMock = new Mock<IFileService>();
        _log = new Mock<IFormatterLog>();

        _sut = new FileBasedConfigurationResolver(
            _jsonLocatorMock.Object,
            _fileSystemMock.Object,
            _fileServiceMock.Object,
            _log.Object
        );
    }

    [Fact]
    public void Resolve_Should_Return_Empty_Dictionary_When_File_Does_Not_Exist()
    {
        // Arrange
        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns("nonexistent.json");
        _fileSystemMock.Setup(fs => fs.FileExists("nonexistent.json")).Returns(false);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_Should_Return_Empty_Dictionary_When_File_Has_No_Formatters()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = "{}";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_Should_Return_Formatters_From_Valid_File()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""formatter1"": {
                        ""config1"": ""setting1"" },
                    ""formatter2"": {
                        ""config2"": ""setting2"" }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(2);
        result["formatter1"]["config1"].Should().Be("setting1");
        result["formatter2"]["config2"].Should().Be("setting2");
    }

    [Fact]
    public void Resolve_Should_Handle_Invalid_Json_File_ByEmittingLog_and_ReturningEmpty()
    {
        // Arrange
        var filePath = "config.json";
        var invalidJsonContent = "{ blah blah json }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(invalidJsonContent);
        IDictionary<string, IDictionary<string, object>> result;
        // Act
        var act = () => result = _sut.Resolve();

        // Assert
        act.Should().NotThrow<Exception>();
        result = _sut.Resolve();
        result.Should().BeEmpty();

    }

    [Fact]
    public void Resolve_Should_Handle_File_With_No_Formatters_Key()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""otherKey"": {
                    ""key1"": ""value1""
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_Should_Return_An_EmptyEntry_When_Key_Has_no_Content()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""emptyFormatter"": {}
                }
            }";
        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);
        // Act
        var result = _sut.Resolve();
        // Assert
        result.Should().HaveCount(1);
        result["emptyFormatter"].Should().BeEmpty();
    }
    [Fact]
    public void Resolve_Should_Return_Formatter_Of_Multiple_Levels_Of_Settings()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""formatter1"": {
                        ""config1"": ""setting1"",
                        ""nestedConfig"": {
                            ""subConfig1"": ""subSetting1""
                        }
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["formatter1"]["config1"].Should().Be("setting1");
        
        var nestedConfig = result["formatter1"]["nestedConfig"];
        nestedConfig.Should().NotBeNull();
        nestedConfig.Should().BeOfType<Dictionary<string, object>>();
        var nestedConfigDict = (IDictionary<string, object>)nestedConfig;
        var subConfig1 = nestedConfigDict["subConfig1"];
        subConfig1.Should().Be("subSetting1");
    }


    [Fact]
    public void Resolve_Should_Parse_AttachmentHandling_Mode_As_Enum_Embed()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""cucumberMessages"": {
                        ""mode"": ""Embed""
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["cucumberMessages"]["mode"].Should().Be(AttachmentHandlingOption.Embed);
    }

    [Fact]
    public void Resolve_Should_Parse_AttachmentHandling_Mode_As_Enum_External()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""cucumberMessages"": {
                        ""mode"": ""External""
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["cucumberMessages"]["mode"].Should().Be(AttachmentHandlingOption.External);
    }

    [Fact]
    public void Resolve_Should_Parse_AttachmentHandling_Mode_As_Enum_None()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""cucumberMessages"": {
                        ""mode"": ""None""
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["cucumberMessages"]["mode"].Should().Be(AttachmentHandlingOption.None);
    }

    [Fact(Skip = "Skip while deciding on proper behavior for invalid configurations")]
    public void Resolve_Should_Parse_AttachmentHandling_Mode_CaseInsensitive()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""cucumberMessages"": {
                        ""mode"": ""embed""
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["cucumberMessages"]["mode"].Should().Be(AttachmentHandlingOption.Embed);
    }

    [Fact]
    public void Resolve_Should_Keep_String_When_AttachmentHandling_Mode_Value_Is_Invalid()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""cucumberMessages"": {
                        ""mode"": ""INVALID_VALUE""
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["cucumberMessages"]["mode"].Should().Be("INVALID_VALUE");
    }

    [Fact]
    public void Resolve_Should_Parse_AttachmentHandling_Mode_In_Nested_Configuration()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""cucumberMessages"": {
                        ""outputPath"": ""output/results.ndjson"",
                        ""mode"": ""External"",
                        ""otherSettings"": {
                            ""enabled"": true
                        }
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(1);
        result["cucumberMessages"]["outputPath"].Should().Be("output/results.ndjson");
        result["cucumberMessages"]["mode"].Should().Be(AttachmentHandlingOption.External);
        
        var otherSettings = result["cucumberMessages"]["otherSettings"];
        otherSettings.Should().BeOfType<Dictionary<string, object>>();
        var otherSettingsDict = (IDictionary<string, object>)otherSettings;
        otherSettingsDict["enabled"].Should().Be(true);
    }

    [Fact]
    public void Resolve_Should_Parse_Multiple_Formatters_With_Different_AttachmentHandling()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""formatter1"": {
                        ""mode"": ""Embed""
                    },
                    ""formatter2"": {
                        ""mode"": ""External""
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result.Should().HaveCount(2);
        result["formatter1"]["mode"].Should().Be(AttachmentHandlingOption.Embed);
        result["formatter2"]["mode"].Should().Be(AttachmentHandlingOption.External);
    }

    [Fact]
    public void Resolve_Should_Parse_Formatter_With_AttachmentOptions()
    {
        // Arrange
        var filePath = "config.json";
        var fileContent = @"
            {
                ""formatters"": {
                    ""formatter1"": {
                        ""attachmentHandling"": {
                            ""mode"": ""External"",
                            ""attachmentsStoragePath"": ""/path/to/attachments""
                        }
                    }
                }
            }";

        _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
        _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
        _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

        // Act
        var result = _sut.Resolve();

        // Assert
        result["formatter1"]["attachmentHandling"].Should().BeOfType<AttachmentHandlingOptions>();
        var attachmentOptions = (AttachmentHandlingOptions)result["formatter1"]["attachmentHandling"];
        attachmentOptions.AttachmentHandlingOption.Should().Be(AttachmentHandlingOption.External);
        attachmentOptions.ExternalAttachmentsStoragePath.Should().Be("/path/to/attachments");
    }

}