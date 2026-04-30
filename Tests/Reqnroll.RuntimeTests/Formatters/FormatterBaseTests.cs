#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Formatters.PubSub;
using Reqnroll.Formatters.RuntimeSupport;
using Xunit;
using System.IO;

namespace Reqnroll.RuntimeTests.Formatters;

public class FormatterBaseTests
{
    private class TestFormatter : FormatterBase
    {
        public bool LaunchInnerCalled = false;
        public IDictionary<string, object> LaunchInnerConfig = null!;
        public Action<bool, AttachmentHandlingOption> LaunchInnerCallback = null!;
        public bool ConsumeAndFormatMessagesCalled = false;
        public CancellationToken? ConsumedToken;
        public List<Envelope> ConsumedMessages = new();
        public bool ReportInitializedCalled = false;
        public bool CloseAsyncCalled = false;
        public bool CompleteWriterOnLaunchInner = false;

        public TestFormatter(IFormattersConfigurationProvider config, IFormatterLog logger, string name)
            : base(config, logger, name) { }

        public override void LaunchInner(IDictionary<string, object> formatterConfig, Action<bool, AttachmentHandlingOption> onAfterInitialization)
        {
            LaunchInnerCalled = true;
            LaunchInnerConfig = formatterConfig;
            LaunchInnerCallback = onAfterInitialization;
            if (CompleteWriterOnLaunchInner)
                PostedMessages.Writer.Complete();
            onAfterInitialization(true, AttachmentHandlingOption);
        }

        protected override async Task ConsumeAndFormatMessagesBackgroundTask(CancellationToken cancellationToken)
        {
            ConsumeAndFormatMessagesCalled = true;
            ConsumedToken = cancellationToken;
            await foreach (var msg in PostedMessages.Reader.ReadAllAsync(cancellationToken))
            {
                ConsumedMessages.Add(msg);
            }
        }

        // Expose protected members for testing
        public new Envelope TransformMessage(Envelope message)
        {
            return base.TransformMessage(message);
        }
        public void SetClosed(bool value)
        {
            typeof(FormatterBase).GetField("Closed", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(this, value);
        }
    }

    private readonly Mock<IFormattersConfigurationProvider> _configMock = new();
    private readonly Mock<IFormatterLog> _loggerMock = new();
    private readonly Mock<ICucumberMessageBroker> _brokerMock = new();
    private readonly TestFormatter _sut;

    public FormatterBaseTests()
    {
        _sut = new TestFormatter(_configMock.Object, _loggerMock.Object, "testPlugin");
    }


    [Fact]
    public void LaunchFormatter_Disabled_ReportsInitializedFalse()
    {
        _configMock.Setup(c => c.Enabled).Returns(false);
        _sut.LaunchFormatter(_brokerMock.Object);
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("disabled via configuration"))), Times.Once);
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, false, AttachmentHandlingOption.Embed), Times.Once);
    }

    [Fact]
    public void LaunchFormatter_Enabled_Calls_LaunchInner_And_StartsTask()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);
        _sut.LaunchInnerCalled.Should().BeTrue();
        _sut.LaunchInnerConfig.Should().NotBeNull();
        _sut.LaunchInnerCallback.Should().NotBeNull();
    }

    [Fact]
    public async Task PublishAsync_Writes_Message_And_Closes_On_TestRunFinished()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);
        var msg = Envelope.Create(new TestRunFinished("", false, new Timestamp(0, 0), null, ""));
        await _sut.PublishAsync(msg);
        _sut.ConsumedMessages.Should().Contain(msg);
    }

    [Fact]
    public async Task PublishAsync_Does_Not_Write_When_Closed()
    {
        _sut.SetClosed(true);
        var msg = Envelope.Create(new TestRunStarted(new Timestamp(0, 0), ""));
        await _sut.PublishAsync(msg);
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("formatter is closed"))), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_Throws_If_Already_Closed()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);
        await _sut.PublishAsync(Envelope.Create(new TestRunStarted(new Timestamp(0, 0), "")));
        await _sut.CloseAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.CloseAsync());
    }

    [Fact]
    public void Dispose_Waits_For_FormatterTask_And_DumpsMessages()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);
        _sut.Dispose();
        _loggerMock.Verify(l => l.DumpMessages(), Times.Once);
    }

    [Fact]
    public void LaunchFormatter_NoConfigEntry_ReportsInitializedFalse()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns((IDictionary<string, object>)null!);
        _sut.LaunchFormatter(_brokerMock.Object);
        _loggerMock.Verify(l => l.WriteMessage(It.Is<string>(s => s.Contains("disabled via configuration"))), Times.Once);
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, false, AttachmentHandlingOption.Embed), Times.Once);
    }

    [Fact]
    public void LaunchFormatter_EmptyConfigEntry_ReportsInitializedTrue()
    {
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);
        _sut.LaunchInnerCalled.Should().BeTrue();
        _sut.LaunchInnerConfig.Should().NotBeNull();
        _sut.LaunchInnerConfig.Should().BeEmpty();
        _sut.LaunchInnerCallback.Should().NotBeNull();
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, true, AttachmentHandlingOption.Embed), Times.Once);

    }

    [Fact]
    public void GetAttachmentHandlingOptionValues_Returns_Default_When_Not_In_Configuration()
    {
        // Arrange
        var config = new Dictionary<string, object>();

        // Act
        var result = _sut.GetAttachmentHandlingOptionValues(config);

        // Assert
        result.AttachmentHandlingOption.Should().Be(AttachmentHandlingOption.Embed);
        result.ExternalAttachmentsStoragePath.Should().Be(string.Empty);
    }

    [Fact]
    public void GetAttachmentHandlingOptionValues_Returns_Configured_Option()
    {
        // Arrange
        var attachmentHandlingOptions = new AttachmentHandlingOptions(AttachmentHandlingOption.External, "/path/to/attachments");
        var config = new Dictionary<string, object> { { "attachmentHandling", attachmentHandlingOptions } };

        // Act
        var result = _sut.GetAttachmentHandlingOptionValues(config);

        // Assert
        result.AttachmentHandlingOption.Should().Be(AttachmentHandlingOption.External);
        result.ExternalAttachmentsStoragePath.Should().Be("/path/to/attachments");
    }

    [Fact]
    public void LaunchFormatter_Sets_AttachmentHandlingOption_From_Configuration()
    {
        // Arrange
        var attachmentHandlingOptions = new AttachmentHandlingOptions(AttachmentHandlingOption.External, "/output/attachments");
        var config = new Dictionary<string, object> { { "attachmentHandling", attachmentHandlingOptions } };
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(config);
        _configMock.Setup(c => c.ResolveTemplatePlaceholders("/output/attachments")).Returns("/resolved/path");

        // Act
        _sut.LaunchFormatter(_brokerMock.Object);

        // Assert
        _sut.AttachmentHandlingOption.Should().Be(AttachmentHandlingOption.External);
        _sut.ExternalAttachmentsStoragePath.Should().Be("/resolved/path");
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, true, AttachmentHandlingOption.External), Times.Once);
    }

    [Fact]
    public void TransformMessage_With_EMBED_Option_Returns_Attachment_Envelope()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var attachment = new Attachment("data", AttachmentContentEncoding.BASE64, "filename.txt", "text/plain", null, "caseId", "stepId", null, "testRunId", null, new Timestamp(0, 0));
        var envelope = Envelope.Create(attachment);

        // Set formatter to EMBED mode
        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.Embed);

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.Attachment.Should().NotBeNull();
        result.Attachment.Should().Be(attachment);
    }

    [Fact]
    public void TransformMessage_With_EXTERNAL_Option_Returns_ExternalAttachment_With_Resolved_Path()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var externalAttachment = new ExternalAttachment("http://example.com/attachment.txt", "text/plain", "stepId", "caseId", null, new Timestamp(0, 0));
        var envelope = Envelope.Create(externalAttachment);

        // Set formatter to EXTERNAL mode with storage path
        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.External);
        _sut.ExternalAttachmentsStoragePath = "/output/attachments";

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.ExternalAttachment.Should().NotBeNull();
        result.ExternalAttachment.Url.Should().Contain("attachment.txt");
        result.ExternalAttachment.Url.Should().Contain("/output/attachments");
    }

    [Fact]
    public void TransformMessage_With_EXTERNAL_Option_And_Rooted_Path_Combines_Paths()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var externalAttachment = new ExternalAttachment("/source/attachment.txt", "text/plain", "stepId", "caseId", null, new Timestamp(0, 0));
        var envelope = Envelope.Create(externalAttachment);

        // Set formatter to EXTERNAL mode with storage path
        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.External);
        _sut.ExternalAttachmentsStoragePath = "/output";

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.ExternalAttachment.Should().NotBeNull();
        result.ExternalAttachment.Url.Should().Be(Path.Combine("/output", "attachment.txt"));
    }

    [Fact]
    public void TransformMessage_With_EXTERNAL_Option_And_Relative_Path_Combines_Paths()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var externalAttachment = new ExternalAttachment("attachment.txt", "text/plain", "stepId", "caseId", null, new Timestamp(0, 0));
        var envelope = Envelope.Create(externalAttachment);

        // Set formatter to EXTERNAL mode with storage path
        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.External);
        _sut.ExternalAttachmentsStoragePath = "/output";

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.ExternalAttachment.Should().NotBeNull();
        result.ExternalAttachment.Url.Should().Be(Path.Combine("/output", "attachment.txt"));
    }

    [Fact]
    public void TransformMessage_Preserves_Attachment_Properties_When_Transforming()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var externalAttachment = new ExternalAttachment("file.txt", "application/pdf", "step123", "case456", "hook789", new Timestamp(100, 50));
        var envelope = Envelope.Create(externalAttachment);

        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.External);
        _sut.ExternalAttachmentsStoragePath = "/attachments";

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.ExternalAttachment.MediaType.Should().Be("application/pdf");
        result.ExternalAttachment.TestStepId.Should().Be("step123");
        result.ExternalAttachment.TestCaseStartedId.Should().Be("case456");
        result.ExternalAttachment.TestRunHookStartedId.Should().Be("hook789");
        result.ExternalAttachment.Timestamp.Seconds.Should().Be(100);
        result.ExternalAttachment.Timestamp.Nanos.Should().Be(50);
    }

    [Fact]
    public void TransformMessage_Returns_Message_Unchanged_For_Non_Attachment_Types()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var testStarted = new TestRunStarted(new Timestamp(0, 0), "");
        var envelope = Envelope.Create(testStarted);

        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.External);

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.Should().Be(envelope);
    }

    [Fact]
    public void TransformMessage_Returns_Message_Unchanged_When_Option_Does_Not_Match_Content()
    {
        // Arrange
        _configMock.Setup(c => c.Enabled).Returns(true);
        _configMock.Setup(c => c.GetFormatterConfigurationByName("testPlugin")).Returns(new Dictionary<string, object>());
        _sut.LaunchFormatter(_brokerMock.Object);

        var externalAttachment = new ExternalAttachment("file.txt", "text/plain", "stepId", "caseId", null, new Timestamp(0, 0));
        var envelope = Envelope.Create(externalAttachment);

        // Set to EMBED mode while message contains ExternalAttachment
        typeof(FormatterBase).GetField("_attachmentHandlingOption", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(_sut, AttachmentHandlingOption.Embed);

        // Act
        var result = _sut.TransformMessage(envelope);

        // Assert
        result.Should().Be(envelope);
    }
}
