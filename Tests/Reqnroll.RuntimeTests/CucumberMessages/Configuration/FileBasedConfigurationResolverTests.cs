using FluentAssertions;
using Moq;
using Reqnroll.Analytics.UserId;
using Reqnroll.Configuration;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Reqnroll.RuntimeTests.CucumberMessages.Configuration
{
    public class FileBasedConfigurationResolverTests
    {
        private readonly Mock<IReqnrollJsonLocator> _jsonLocatorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly FileBasedConfigurationResolver _sut;

        public FileBasedConfigurationResolverTests()
        {
            _jsonLocatorMock = new Mock<IReqnrollJsonLocator>();
            _fileSystemMock = new Mock<IFileSystem>();
            _fileServiceMock = new Mock<IFileService>();

            _sut = new FileBasedConfigurationResolver(
                _jsonLocatorMock.Object,
                _fileSystemMock.Object,
                _fileServiceMock.Object
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
                    ""formatter1"": ""config1"",
                    ""formatter2"": ""config2""
                }
            }";

            _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
            _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
            _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(fileContent);

            // Act
            var result = _sut.Resolve();

            // Assert
            result.Should().HaveCount(2);
            result["formatter1"].Should().Be("\"config1\"");
            result["formatter2"].Should().Be("\"config2\"");
        }

        [Fact]
        public void Resolve_Should_Handle_Invalid_Json_File_ByThrowingException()
        {
            // Arrange
            var filePath = "config.json";
            var invalidJsonContent = "{ blah blah json }";

            _jsonLocatorMock.Setup(locator => locator.GetReqnrollJsonFilePath()).Returns(filePath);
            _fileSystemMock.Setup(fs => fs.FileExists(filePath)).Returns(true);
            _fileServiceMock.Setup(fs => fs.ReadAllText(filePath)).Returns(invalidJsonContent);

            // Act
            var act = () => _sut.Resolve();

            // Assert
            act.Should().Throw<Exception>().WithMessage("*invalid*");
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
    }
}
