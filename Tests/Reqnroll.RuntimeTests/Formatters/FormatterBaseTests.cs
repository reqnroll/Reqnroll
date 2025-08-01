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

namespace Reqnroll.RuntimeTests.Formatters;

public class FormatterBaseTests
{
    private class TestFormatter : FormatterBase
    {
        public bool LaunchInnerCalled = false;
        public IDictionary<string, object> LaunchInnerConfig = null!;
        public Action<bool> LaunchInnerCallback = null!;
        public bool ConsumeAndFormatMessagesCalled = false;
        public CancellationToken? ConsumedToken;
        public List<Envelope> ConsumedMessages = new();
        public bool ReportInitializedCalled = false;
        public bool CloseAsyncCalled = false;
        public bool CompleteWriterOnLaunchInner = false;

        public TestFormatter(IFormattersConfigurationProvider config, IFormatterLog logger, string name)
            : base(config, logger, name) { }

        public override void LaunchInner(IDictionary<string, object> formatterConfig, Action<bool> onAfterInitialization)
        {
            LaunchInnerCalled = true;
            LaunchInnerConfig = formatterConfig;
            LaunchInnerCallback = onAfterInitialization;
            if (CompleteWriterOnLaunchInner)
                PostedMessages.Writer.Complete();
            onAfterInitialization(true);
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
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, false), Times.Once);
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
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, false), Times.Once);
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
        _brokerMock.Verify(b => b.FormatterInitialized(_sut, true), Times.Once);

    }
}
