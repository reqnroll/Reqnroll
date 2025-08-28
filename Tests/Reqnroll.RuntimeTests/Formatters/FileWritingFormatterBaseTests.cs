#nullable enable
using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters;

public class FileWritingFormatterBaseTests
{
    private class TestFileWritingFormatter : FileWritingFormatterBase
    {
        public bool OnTargetFileStreamInitializedCalled = false;
        public bool OnTargetFileStreamDisposingCalled = false;
        public bool WriteToFileCalled = false;
        public bool OnCancellationCalled = false;
        public bool FlushTargetFileStreamCalled = false;
        public string? LastOutputPath;
        public Envelope? LastEnvelope;
        public CancellationToken? LastToken;
        public bool ThrowOnCreateTargetFileStream = false;
        public bool ThrowOnWriteToFile = false;
        public bool ThrowOnFlush = false;
        public bool ThrowOnTargetFileStreamInitialized = false;
        public bool ThrowOnTargetFileStreamDisposing = false;
        public bool ThrowOnOnCancellation = false;
        public bool ThrowOnFinalizeInitialization = false;
        public bool FinalizeInitializationCalled = false;
        public Stream? LastStream;

        public TestFileWritingFormatter(IFormattersConfigurationProvider config, IFormatterLog logger, IFileSystem fileSystem)
            : base(config, logger, fileSystem, "testPlugin", ".txt", "default.txt") { }

        protected override void OnTargetFileStreamInitialized(Stream targetFileStream)
        {
            OnTargetFileStreamInitializedCalled = true;
            LastStream = targetFileStream;
            if (ThrowOnTargetFileStreamInitialized) throw new System.Exception("fail");
        }
        protected override void OnTargetFileStreamDisposing()
        {
            OnTargetFileStreamDisposingCalled = true;
            if (ThrowOnTargetFileStreamDisposing) throw new System.Exception("fail");
        }
        protected override async Task WriteToFile(Envelope envelope, CancellationToken cancellationToken)
        {
            WriteToFileCalled = true;
            LastEnvelope = envelope;
            LastToken = cancellationToken;
            if (ThrowOnWriteToFile) throw new System.Exception("fail");
            await Task.CompletedTask;
        }
        protected override Task OnCancellation()
        {
            OnCancellationCalled = true;
            if (ThrowOnOnCancellation) throw new System.Exception("fail");
            return Task.CompletedTask;
        }
        protected override async Task FlushTargetFileStream(CancellationToken cancellationToken)
        {
            FlushTargetFileStreamCalled = true;
            if (ThrowOnFlush) throw new System.Exception("fail");
            await base.FlushTargetFileStream(cancellationToken);
        }
        protected override Stream CreateTargetFileStream(string outputPath)
        {
            if (ThrowOnCreateTargetFileStream) throw new System.Exception("fail");
            return new MemoryStream();
        }
        protected override void FinalizeInitialization(string outputPath, IDictionary<string, object> formatterConfiguration, Action<bool> onInitialized)
        {
            FinalizeInitializationCalled = true;
            if (ThrowOnFinalizeInitialization) throw new System.Exception("fail");
            base.FinalizeInitialization(outputPath, formatterConfiguration, onInitialized);
            LastOutputPath = outputPath;
        }
        public async Task PostEnvelopeAsync(Envelope envelope)
        {
            await PostedMessages.Writer.WriteAsync(envelope);
        }

        public string TestConfiguredOutputFilePath(IDictionary<string, object> formatterConfiguration)
        {
            return ConfiguredOutputFilePath(formatterConfiguration);
        }
    }

    private readonly Mock<IFormattersConfigurationProvider> _configMock = new();
    private readonly Mock<IFormatterLog> _loggerMock = new();
    private readonly Mock<IFileSystem> _fileSystemMock = new();
    private readonly TestFileWritingFormatter _sut;

    public FileWritingFormatterBaseTests()
    {
        _sut = new TestFileWritingFormatter(_configMock.Object, _loggerMock.Object, _fileSystemMock.Object);
    }

    [Fact]
    public void LaunchInner_InvalidPathCharacters_HandlesGracefully()
    {
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "invalid\0path.txt" } };
        _sut.LaunchInner(config, enabled => enabled.Should().BeFalse());
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains( "is invalid or missing."))), Times.Once);
    }

    [Fact]
    public void LaunchInner_EmptyFileName_UsesDefaultFileName()
    {
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "somedir/" } };
        _sut.LaunchInner(config, enabled => enabled.Should().BeTrue());
        _sut.LastOutputPath.Should().EndWith("default.txt");
    }


    [Fact]
    public void LaunchInner_InvalidFile_DisablesFormatter()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip this test on non-Windows platforms as it checks for invalid file names specific to Windows.
        }

        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        _configMock.Setup(c => c.Enabled).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "invalid|file.txt" } };
        _sut.LaunchInner(config, enabled => enabled.Should().BeFalse());
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("invalid or missing"))), Times.Once);
    }

    [Fact]
    public void LaunchInner_CreatesDirectoryIfNotExists()
    {
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(false);
        _configMock.Setup(c => c.Enabled).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "dir/file.txt" } };
        _sut.LaunchInner(config, _ => { });
        _fileSystemMock.Verify(f => f.CreateDirectory(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void LaunchInner_HandlesExceptionOnCreateDirectory()
    {
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(false);
        _fileSystemMock.Setup(f => f.CreateDirectory(It.IsAny<string>())).Throws(new System.Exception("fail"));
        var config = new Dictionary<string, object> { { "outputFilePath", "dir/file.txt" } };
        _sut.LaunchInner(config, enabled => enabled.Should().BeFalse());
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("occurred creating the destination directory"))), Times.Once);
    }

    [Fact]
    public void LaunchInner_ValidConfig_InitializesFileStream()
    {
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "file.txt" } };
        _sut.LaunchInner(config, enabled => enabled.Should().BeTrue());
        _sut.OnTargetFileStreamInitializedCalled.Should().BeTrue();
        _sut.LastOutputPath.Should().NotBeNull();
    }

    [Fact]
    public async Task ConsumeAndFormatMessagesBackgroundTask_HandlesNullTargetFileStream()
    {
        // Arrange: set a flag so that the SUT sets TargetFileStream to null during initialization
        _sut.ThrowOnCreateTargetFileStream = true; // Use this flag to simulate failure and set TargetFileStream to null

        var config = new Dictionary<string, object> { { "outputFilePath", "file.txt" } };
        _sut.LaunchInner(config, _ => { });

        // Act: invoke the background task
        var method = _sut.GetType().BaseType!.GetMethod("ConsumeAndFormatMessagesBackgroundTask", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (method != null)
        {
            var task = method.Invoke(_sut, new object[] { CancellationToken.None }) as Task;
            if (task != null)
            {
                try { await task; } catch { /* ignore exceptions for this test */ }
            }
        }

        // Assert: logger should have been called with the expected message
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("filestream is not open"))), Times.Once);
    }

    [Fact]
    public async Task ConsumeAndFormatMessagesBackgroundTask_HandlesOperationCanceledException()
    {
        // Arrange: set up a valid file stream and post a message
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "file.txt" } };
        _sut.LaunchInner(config, _ => { });
        var envelope = Envelope.Create(new TestRunStarted(new Io.Cucumber.Messages.Types.Timestamp(0, 0), ""));
        await _sut.PostEnvelopeAsync(envelope);

        // Act: cancel the token before running the background task
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        var method = _sut.GetType().BaseType!.GetMethod("ConsumeAndFormatMessagesBackgroundTask", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (method != null)
        {
            var task = method.Invoke(_sut, new object[] { tokenSource.Token }) as Task;
            if (task != null)
            {
                await task;
            }
        }

        // Assert: should log cancellation
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("has been cancelled"))), Times.AtLeastOnce);
        _sut.OnCancellationCalled.Should().BeTrue();
    }

    [Fact]
    public void Dispose_CallsDisposeFileStreamAndBaseDispose()
    {
        _fileSystemMock.Setup(f => f.DirectoryExists(It.IsAny<string>())).Returns(true);
        var config = new Dictionary<string, object> { { "outputFilePath", "file.txt" } };
        _sut.LaunchInner(config, _ => { });
        _sut.Dispose();
        _sut.OnTargetFileStreamDisposingCalled.Should().BeTrue();
    }

    [Fact]
    public void LaunchFormatter_Should_Create_Local_Path_When_No_Path_Provided_in_Configuration()
    {
        var sp = Path.DirectorySeparatorChar;
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", "aFileName.txt" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        _sut.LaunchFormatter(new Mock<ICucumberMessageBroker>().Object);
        _sut.Dispose();

        _sut.LastOutputPath.Should().NotBeNull();
        _sut.LastOutputPath.Should().Be($".{sp}aFileName.txt");
    }

    [Fact]
    public void LaunchFormatter_Should_Apply_Default_Extension_When_Filename_Has_No_Extension()
    {
        var sp = Path.DirectorySeparatorChar;
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", "myoutput" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        _sut.LaunchFormatter(new Mock<ICucumberMessageBroker>().Object);
        _sut.Dispose();

        _sut.LastOutputPath.Should().NotBeNull();
        _sut.LastOutputPath.Should().EndWith($".{sp}myoutput.txt");
    }

    [Fact]
    public void LaunchFormatter_Should_Not_Apply_Default_Extension_When_Filename_Has_Extension()
    {
        var sp = Path.DirectorySeparatorChar;
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object> { { "outputFilePath", "myoutput.log" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        _sut.LaunchFormatter(new Mock<ICucumberMessageBroker>().Object);
        _sut.Dispose();

        _sut.LastOutputPath.Should().NotBeNull();
        _sut.LastOutputPath!.Should().NotContain(".txt");
    }

    [Fact]
    public async Task PublishAsync_Should_Write_Envelopes()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                          .Returns(new Dictionary<string, object> { { "outputFilePath", @"C:\/valid\/path/output.txt" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var message = Envelope.Create(new TestRunStarted(new Io.Cucumber.Messages.Types.Timestamp(1, 0), "started"));

        _sut.LaunchFormatter(new Mock<ICucumberMessageBroker>().Object);
        await _sut.PublishAsync(message);
        await _sut.CloseAsync();

        _sut.LastEnvelope.Should().Be(message);
    }

    [Fact]
    public void LaunchFormatter_Should_Create_Directory_If_Not_Exists()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                          .Returns(new Dictionary<string, object> { { "outputFilePath", "outputFilePath" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(false);

        _sut.LaunchFormatter(new Mock<ICucumberMessageBroker>().Object);
        _sut.Dispose();

        _fileSystemMock.Verify(fs => fs.CreateDirectory(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Publish_FollowedBy_Dispose_Should_Cause_CancelToken_to_Fire()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin"))
                          .Returns(new Dictionary<string, object> { { "outputFilePath", @"C:\/valid\/path/output.txt" } });
        _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);

        var message = Envelope.Create(new TestRunStarted(new Io.Cucumber.Messages.Types.Timestamp(1, 0), "started"));

        _sut.LaunchFormatter(new Mock<ICucumberMessageBroker>().Object);
        await _sut.PublishAsync(message);
        _sut.Dispose();

        _sut.LastEnvelope.Should().Be(message);
        _sut.OnCancellationCalled.Should().BeTrue();
    }

    [Fact]
    public void ConfiguredOutputFilePath_MissingKey_ReturnsEmptyString()
    {
        var config = new Dictionary<string, object>();
        var result = _sut.TestConfiguredOutputFilePath(config);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ConfiguredOutputFilePath_NullValue_ReturnsEmptyString()
    {
        var config = new Dictionary<string, object> { { "outputFilePath", null! } };
        var result = _sut.TestConfiguredOutputFilePath(config);
        result.Should().BeEmpty();
    }
}
