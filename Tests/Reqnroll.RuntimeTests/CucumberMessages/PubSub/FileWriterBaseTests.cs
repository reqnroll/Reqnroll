using Moq;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.CucumberMessages.PubSub;
using Reqnroll.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Io.Cucumber.Messages.Types;
using Reqnroll.Events;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using Reqnroll.BoDi;
using Reqnroll.Plugins;
using Reqnroll.Tracing;

namespace Reqnroll.RuntimeTests.CucumberMessages.PubSub
{
    public class FileWritingFormatterPluginBaseTests
    {
        private class TestFileWritingFormatterPlugin : FileWritingFormatterPluginBase
        {
            public string LastOutputPath { get; private set; }

            public TestFileWritingFormatterPlugin(
                ICucumberMessagesConfiguration configuration,
                ICucumberMessageBroker broker,
                IFileSystem fileSystem,
                ICollection<Envelope> messageCollector)
                : base(configuration, broker, "testPlugin", ".txt", "test_output.txt", fileSystem)
            {
                FileSystem = fileSystem;
                _messageCollector = messageCollector;
            }

            internal override void ConsumeAndWriteToFilesBackgroundTask(string outputPath)
            {
                LastOutputPath = outputPath;
                foreach (var message in _postedMessages.GetConsumingEnumerable())
                {
                    // Simulate writing to a file
                    _messageCollector.Add(message);
                }
            }

            public IFileSystem FileSystem { get; }

            private ICollection<Envelope> _messageCollector;
        }

        private IObjectContainer _globalObjContainerStub;
        private IObjectContainer _testThreadObjContainerStub;
        private Mock<ITestThreadExecutionEventPublisher> _eventPublisherMock;
        private Mock<ITraceListener> _tracerMock;
        private RuntimePluginEvents _rtpe = new();
        private RuntimePluginParameters _rtpp = new();

        private readonly Mock<ICucumberMessagesConfiguration> _configurationMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ICucumberMessageBroker> _brokerMock;
        private readonly TestFileWritingFormatterPlugin _sut;
        private List<Envelope> postedEnvelopes = new();

        public FileWritingFormatterPluginBaseTests()
        {
            _globalObjContainerStub = new ObjectContainer(null);
            _testThreadObjContainerStub = new ObjectContainer(_globalObjContainerStub);
            _eventPublisherMock = new Mock<ITestThreadExecutionEventPublisher>();
            _tracerMock = new Mock<ITraceListener>();
            _configurationMock = new Mock<ICucumberMessagesConfiguration>();
            _fileSystemMock = new Mock<IFileSystem>();
            _brokerMock = new Mock<ICucumberMessageBroker>();

            _testThreadObjContainerStub.RegisterInstanceAs<ITestThreadExecutionEventPublisher>(_eventPublisherMock.Object);
            _testThreadObjContainerStub.RegisterInstanceAs<ITraceListener>(_tracerMock.Object);


            _sut = new TestFileWritingFormatterPlugin(_configurationMock.Object, _brokerMock.Object, _fileSystemMock.Object, postedEnvelopes);
            _sut.Initialize(_rtpe, _rtpp, new UnitTestProvider.UnitTestProviderConfiguration());
            _rtpe.RaiseCustomizeGlobalDependencies(_globalObjContainerStub as ObjectContainer, null);
            _rtpe.RaiseCustomizeTestThreadDependencies(_testThreadObjContainerStub as ObjectContainer);
        }

        [Fact]
        public async Task LaunchFileSink_Should_Create_Output_File_With_Default_Name_If_No_Configuration()
        {
            // Arrange
            var sp = Path.DirectorySeparatorChar;
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns("{\"outputFilePath\": \"\"}");
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            // Act
            await _sut.LaunchFileSinkAsync();
            await _sut.CloseAsync();

            // Assert
            Assert.NotNull(_sut.LastOutputPath);
            Assert.EndsWith($".{sp}test_output.txt", _sut.LastOutputPath);
        }

        [Fact]
        public async Task LaunchFileSink_Should_Create_Local_Path_When_No_Path_Provided_in_Configuration()
        {
            // Arrange
            var sp = Path.DirectorySeparatorChar;
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns("{\"outputFilePath\": \"aFileName.txt\"}");
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            // Act
            await _sut.LaunchFileSinkAsync();
            await _sut.CloseAsync();

            // Assert
            Assert.NotNull(_sut.LastOutputPath);
            Assert.Equal($".{sp}aFileName.txt", _sut.LastOutputPath);
        }
        [Fact]
        public async Task PublishAsync_Should_Write_Envelopes()
        {
            // Arrange
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns("""
                { "outputFilePath": "C:\/valid\/path/output.txt" }
                """);
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "started"));

            // Act
            await _sut.LaunchFileSinkAsync();
            await _sut.PublishAsync(message);
            await _sut.CloseAsync();

            // Assert
            postedEnvelopes.Should().Contain(message);
        }


        [Fact]
        public void ParseConfigurationString_Should_Return_Valid_OutputFilePath()
        {
            // Arrange
            var validConfig = """
                { "outputFilePath": "C:\\valid/path/output.txt" }
                """;
            // Act
            var result = _sut.ParseConfigurationString(validConfig, "testPlugin");

            // Assert
            Assert.Equal(@"C:\valid/path/output.txt", result);
        }

        [Fact]
        public void ParseConfigurationString_Should_Return_Empty_For_Invalid_Json()
        {
            // Arrange
            var invalidConfig = "invalid_json";

            // Act
            var result = _sut.ParseConfigurationString(invalidConfig, "testPlugin");

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task LaunchFileSink_Should_Create_Directory_If_Not_Exists()
        {
            // Arrange
            var sp = Path.DirectorySeparatorChar;
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns("""
                { "outputFilePath": "C:\/valid\/path/output.txt" }
                """);
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            // Act
            await _sut.LaunchFileSinkAsync();
            await _sut.CloseAsync();

            // Assert
            _fileSystemMock.Verify(fs => fs.CreateDirectory($"C:{sp}valid{sp}path"), Times.Once);
        }
    }
}
