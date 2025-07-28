using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.BoDi;
using Reqnroll.Events;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Tracing;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.PubSub;

public class FileWritingFormatterBaseTests
{
    private class ConsoleLogger : IFormatterLog
    {
        private readonly List<string> _entries = new();
        private bool _hasDumped = false;
        public void WriteMessage(string message)
        {
            _entries.Add($"{DateTime.Now:HH:mm:ss.fff}: {message}");
        }

        public void DumpMessages()
        {
            if (!_hasDumped)
                foreach (var msg in _entries)
                {
                    Console.WriteLine(msg);
                }
            _hasDumped = true;
        }
    }

    private class TestFileWritingFormatter(
        IFormattersConfigurationProvider configurationProvider,
        IFileSystem fileSystem,
        ICollection<Envelope> messageCollector)
        : FileWritingFormatterBase(configurationProvider, new ConsoleLogger(), fileSystem, "testPlugin", ".txt", "test_output.txt")
    {
        public string LastOutputPath { get; private set; }
        public bool WasCancelled = false;
        private readonly Mock<Stream> _fileStreamMock = new(MockBehavior.Loose);

        protected override async Task WriteToFile(Envelope env, CancellationToken cancellationToken)
        {

            // Simulate writing to a file
            messageCollector.Add(env);
        }

        protected override void OnTargetFileStreamInitialized(Stream targetFileStream)
        {
        }

        protected override void OnTargetFileStreamDisposing()
        {
        }

        protected override Stream CreateTargetFileStream(string outputPath)
        {
            return _fileStreamMock.Object;
        }

        protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
        {
            LastOutputPath = outputPath;
            base.FinalizeInitialization(outputPath, formatterConfiguration, onInitialized);
        }

        protected override async Task OnCancellation()
        {
            WasCancelled = true;
            await Task.CompletedTask;
        }
    }

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IObjectContainer _globalObjContainerStub;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IObjectContainer _testThreadObjContainerStub;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<ITestThreadExecutionEventPublisher> _eventPublisherMock;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<ITraceListener> _tracerMock;

    private readonly Mock<IFormattersConfigurationProvider> _configurationMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<ICucumberMessageBroker> _brokerMock;
    private readonly TestFileWritingFormatter _sut;
    private readonly List<Envelope> _postedEnvelopes = new();

    public FileWritingFormatterBaseTests()
    {
        _globalObjContainerStub = new ObjectContainer();
        _testThreadObjContainerStub = new ObjectContainer(_globalObjContainerStub);
        _eventPublisherMock = new Mock<ITestThreadExecutionEventPublisher>();
        _tracerMock = new Mock<ITraceListener>();
        _configurationMock = new Mock<IFormattersConfigurationProvider>();
        _fileSystemMock = new Mock<IFileSystem>();
        _brokerMock = new Mock<ICucumberMessageBroker>();

        _testThreadObjContainerStub.RegisterInstanceAs(_eventPublisherMock.Object);
        _testThreadObjContainerStub.RegisterInstanceAs(_tracerMock.Object);


        _sut = new TestFileWritingFormatter(_configurationMock.Object, _fileSystemMock.Object, _postedEnvelopes);
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
        _sut.LaunchSink(_brokerMock.Object);
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
        _sut.LaunchSink(_brokerMock.Object);
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
        _sut.LaunchSink(_brokerMock.Object);
        await _sut.PublishAsync(message);
        await _sut.CloseAsync();

        // Assert
        _postedEnvelopes.Should().Contain(message);
    }

    [Fact]
    public async Task LaunchFileSink_Should_Create_Directory_If_Not_Exists()
    {
        // Arrange
        _configurationMock.Setup(c => c.Enabled).Returns(true);
        _configurationMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                          .Returns(new Dictionary<string, object> { { "outputFilePath", "outputFilePath" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        // Act
        _sut.LaunchSink(_brokerMock.Object);
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
        _sut.LaunchSink(_brokerMock.Object);
        await _sut.PublishAsync(message);
        _sut.Dispose();

        // Assert
        _postedEnvelopes.Should().Contain(message);
        _sut.WasCancelled.Should().BeTrue();
    }

}