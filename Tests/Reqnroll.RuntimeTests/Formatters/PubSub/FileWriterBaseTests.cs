using Moq;
using Reqnroll.Formatters.PubSub;
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
using Reqnroll.Formatters;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.RuntimeSupport;
using System.Text.Json;

namespace Reqnroll.RuntimeTests.Formatters.PubSub
{
    public class FileWritingFormatterPluginBaseTests
    {
        private class TestFileWritingFormatterPlugin : FileWritingFormatterPluginBase
        {
            public string LastOutputPath { get; private set; }

            public TestFileWritingFormatterPlugin(
                IFormattersConfigurationProvider configurationProvider,
                ICucumberMessageBroker broker,
                IFileSystem fileSystem,
                ICollection<Envelope> messageCollector)
                : base(configurationProvider, broker, new FormatterLog(), "testPlugin", ".txt", "test_output.txt", fileSystem)
            {
                FileSystem = fileSystem;
                _messageCollector = messageCollector;
            }

            protected override async Task ConsumeAndWriteToFilesBackgroundTask(string outputPath)
            {
                LastOutputPath = outputPath;
                foreach (var message in PostedMessages.GetConsumingEnumerable())
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

        private readonly Mock<IFormattersConfigurationProvider> _configurationMock;
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
            _configurationMock = new Mock<IFormattersConfigurationProvider>();
            _fileSystemMock = new Mock<IFileSystem>();
            _brokerMock = new Mock<ICucumberMessageBroker>();

            _testThreadObjContainerStub.RegisterInstanceAs(_eventPublisherMock.Object);
            _testThreadObjContainerStub.RegisterInstanceAs(_tracerMock.Object);


            _sut = new TestFileWritingFormatterPlugin(_configurationMock.Object, _brokerMock.Object, _fileSystemMock.Object, postedEnvelopes);
            _sut.Initialize(_rtpe, _rtpp, new UnitTestProvider.UnitTestProviderConfiguration());
        }

        [Fact]
        public async Task LaunchFileSink_Should_Create_Output_File_With_Default_Name_If_No_Configuration()
        {
            // Arrange
            var sp = Path.DirectorySeparatorChar;
            var filepath = "";
            using var doc = CreateJsonDoc("outputFilePath", filepath);

            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", doc.RootElement.GetProperty("outputFilePath") } });
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            // Act
            _sut.LaunchSink();
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
            var filepath = "aFileName.txt";
            using var doc = CreateJsonDoc("outputFilePath", filepath);

            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", doc.RootElement.GetProperty("outputFilePath") } });
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            // Act
            _sut.LaunchSink();
            await _sut.CloseAsync();

            // Assert
            Assert.NotNull(_sut.LastOutputPath);
            Assert.Equal($".{sp}aFileName.txt", _sut.LastOutputPath);
        }
        [Fact]
        public async Task PublishAsync_Should_Write_Envelopes()
        {
            // Arrange
            var filepath = @"C:\/valid\/path/output.txt";
            using var doc = CreateJsonDoc("outputFilePath", filepath);

            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                .Returns(new Dictionary<string, object> { { "outputFilePath", doc.RootElement.GetProperty("outputFilePath") } });
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "started"));

            // Act
            _sut.LaunchSink();
            await _sut.PublishAsync(message);
            await _sut.CloseAsync();

            // Assert
            postedEnvelopes.Should().Contain(message);
        }

        [Fact]
        public async Task LaunchFileSink_Should_Create_Directory_If_Not_Exists()
        {
            // Arrange
            var sp = Path.DirectorySeparatorChar;
            var filepath = @"C:\/valid\/path\/output.txt".Replace('/', sp);
            using var doc = CreateJsonDoc("outputFilePath", filepath);
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                .Returns(new Dictionary<string, object> { { "outputFilePath", doc.RootElement.GetProperty("outputFilePath") } });
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            // Act
            _sut.LaunchSink();
            await _sut.CloseAsync();

            // Assert
            _fileSystemMock.Verify(fs => fs.CreateDirectory(Path.GetDirectoryName(filepath)), Times.Once);
        }

        private JsonDocument CreateJsonDoc(string key, string value)
        {
            var jsonText = $" {{ \"{key}\": \"{value}\" }}";
            return JsonDocument.Parse(jsonText);
        }
    }
}
