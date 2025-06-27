using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Plugins;
using Reqnroll.Tracing;
using Reqnroll.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.PubSub
{
    public class FileWritingFormatterPluginBaseTests
    {
        private class ConsoleLogger : IFormatterLog
        {
            public List<string> entries = new();
            private bool hasDumped = false;
            public void WriteMessage(string message)
            {
                entries.Add($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}");
            }

            public void DumpMessages()
            {
                if (!hasDumped)
                    foreach (var msg in entries)
                    {
                        Console.WriteLine(msg);
                    }
                hasDumped = true;
            }
        }
        private class TestFileWritingFormatterPlugin : FileWritingFormatterPluginBase
        {
            public string LastOutputPath { get; private set; }
            public bool WasCancelled = false;

            public TestFileWritingFormatterPlugin(
                IFormattersConfigurationProvider configurationProvider,
                ICucumberMessageBroker broker,
                IFileSystem fileSystem,
                ICollection<Envelope> messageCollector)
                : base(configurationProvider, broker, new ConsoleLogger(), "testPlugin", ".txt", "test_output.txt", fileSystem)
            {
                FileSystem = fileSystem;
                _messageCollector = messageCollector;
            }

            protected override async Task ConsumeAndWriteToFilesBackgroundTask(string outputPath, CancellationToken cancellationToken)
            {
                LastOutputPath = outputPath;
                foreach (var message in PostedMessages.GetConsumingEnumerable())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        WasCancelled = true;
                        break;
                    }

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
        }

        [Fact]
        public async Task LaunchFileSink_Should_Create_Output_File_With_Default_Name_If_No_Configuration()
        {
            // Arrange
            var sp = Path.DirectorySeparatorChar;

            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", "" } });
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
 
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", "aFileName.txt" } });
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
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                .Returns(new Dictionary<string, object> { { "outputFilePath", @"C:\/valid\/path/output.txt" } });
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
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                .Returns(new Dictionary<string, object> { { "outputFilePath", "outputFilePath" } });
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

            // Act
            _sut.LaunchSink();
            await _sut.CloseAsync();

            // Assert
            _fileSystemMock.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Publish_FollowedBy_Dispose_Should_Cause_CancelToken_to_Fire()
        {
            // Arrange
            _configurationMock.Setup(c => c.Enabled).Returns(true);
            _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                .Returns(new Dictionary<string, object> { { "outputFilePath", @"C:\/valid\/path/output.txt" } });
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

            var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "started"));

            // Act
            _sut.LaunchSink();
            await _sut.PublishAsync(message);
            _sut.Dispose();

            // Assert
            postedEnvelopes.Should().Contain(message);
            _sut.WasCancelled.Should().BeTrue();
        }

    }
}
