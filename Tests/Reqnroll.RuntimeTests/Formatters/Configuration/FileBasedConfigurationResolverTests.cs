using FluentAssertions;
using Moq;
using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
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
}