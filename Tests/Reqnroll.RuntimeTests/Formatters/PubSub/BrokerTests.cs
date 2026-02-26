using Moq;
using Reqnroll.Formatters.PubSub;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Formatters;
using Reqnroll.Formatters.Configuration;

namespace Reqnroll.RuntimeTests.Formatters.PubSub;

public class CucumberMessageBrokerTests
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Mock<IFormatterLog> _logMock;
    private readonly Mock<ICucumberMessageFormatter> _formatterMock1;
    private readonly Mock<ICucumberMessageFormatter> _formatterMock2;
    private readonly CucumberMessageBroker _sut;

    public CucumberMessageBrokerTests()
    {
        _logMock = new Mock<IFormatterLog>();
        _formatterMock1 = new Mock<ICucumberMessageFormatter>();
        _formatterMock1.Setup(s => s.Name).Returns("formatter1");
        _formatterMock2 = new Mock<ICucumberMessageFormatter>();
        _formatterMock2.Setup(s => s.Name).Returns("formatter2");
        // Initialize the system under test (SUT)
        _sut = new CucumberMessageBroker(_logMock.Object, new Dictionary<string, ICucumberMessageFormatter> { { "formatter1", _formatterMock1.Object}, { "formatter2", _formatterMock2.Object} }, null);
    }

    [Fact]
    public async Task Enabled_Should_Return_True_When_Formatters_Are_Registered()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.Embed);

        Assert.False(_sut.IsEnabled); // should not be enabled until after both formatters are registered

        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.Embed);

        // Act
        var result = _sut.IsEnabled;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Enabled_Should_Return_False_When_No_Formatters_Are_Registered()
    {
        var log = new Mock<IFormatterLog>();

        var sut = new CucumberMessageBroker(log.Object, new Dictionary<string, ICucumberMessageFormatter>(), null);

        // Act
        var result = sut.IsEnabled;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PublishAsync_Should_Invoke_PublishAsync_On_All_Formatters()
    {
        // Arrange
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.Embed);
        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.Embed);

        var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "testStart"));

        // Act
        await _sut.PublishAsync(message);

        // Assert
        _formatterMock1.Verify(sink => sink.PublishAsync(message), Times.Once);
        _formatterMock2.Verify(sink => sink.PublishAsync(message), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_Should_Swallow_Exceptions_From_Formatters()
    {
        // Arrange
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.Embed);
        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.Embed);

        var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "testStart"));

        _formatterMock1
            .Setup(formatter => formatter.PublishAsync(message))
            .ThrowsAsync(new System.Exception("Formatter 1 failed"));

        // Act
        var exception = await Record.ExceptionAsync(() => _sut.PublishAsync(message));

        // Assert
        Assert.Null(exception); // No exception should propagate
        _formatterMock2.Verify(formatter => formatter.PublishAsync(message), Times.Once); // Other formatters should still be called
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Return_EMBED_When_Single_Formatter_Uses_EMBED()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.Embed);
        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.None);

        // Act
        var result = _sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.Embed | AttachmentHandlingOption.None, result);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Combine_Options_With_Bitwise_OR()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.Embed);
        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.External);

        // Act
        var result = _sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.Embed | AttachmentHandlingOption.External, result);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Return_EMBED_Default_When_No_Formatters_Active()
    {
        // Arrange
        var log = new Mock<IFormatterLog>();
        var sut = new CucumberMessageBroker(log.Object, new Dictionary<string, ICucumberMessageFormatter>(), null);

        // Act
        var result = sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.Embed, result);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Not_Include_Disabled_Formatter_Options()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.Embed);
        _sut.FormatterInitialized(_formatterMock2.Object, false, AttachmentHandlingOption.External); // Disabled

        // Act
        var result = _sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.Embed, result);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Handle_All_Three_Options()
    {
        // Arrange
        var log = new Mock<IFormatterLog>();
        var formatterMock1 = new Mock<ICucumberMessageFormatter>();
        formatterMock1.Setup(s => s.Name).Returns("formatter1");
        var formatterMock2 = new Mock<ICucumberMessageFormatter>();
        formatterMock2.Setup(s => s.Name).Returns("formatter2");
        var formatterMock3 = new Mock<ICucumberMessageFormatter>();
        formatterMock3.Setup(s => s.Name).Returns("formatter3");

        var sut = new CucumberMessageBroker(log.Object, 
            new Dictionary<string, ICucumberMessageFormatter> 
            { 
                { "formatter1", formatterMock1.Object },
                { "formatter2", formatterMock2.Object },
                { "formatter3", formatterMock3.Object }
            }, 
            null);

        sut.Initialize();
        sut.FormatterInitialized(formatterMock1.Object, true, AttachmentHandlingOption.Embed);
        sut.FormatterInitialized(formatterMock2.Object, true, AttachmentHandlingOption.External);
        sut.FormatterInitialized(formatterMock3.Object, true, AttachmentHandlingOption.None);

        // Act
        var result = sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.Embed | AttachmentHandlingOption.External | AttachmentHandlingOption.None, result);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Be_Set_After_All_Formatters_Initialize()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.External);

        // Act - formatter 1 initialized but formatter 2 hasn't yet
        var resultPartial = _sut.AggregateAttachmentHandlingOption;

        // At this point, aggregation should not have been finalized
        // (depends on initialization completion)
        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.Embed);

        // After all formatters initialize
        var resultFinal = _sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.External | AttachmentHandlingOption.Embed, resultFinal);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Return_NONE_When_All_Formatters_Use_NONE()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, true, AttachmentHandlingOption.None);
        _sut.FormatterInitialized(_formatterMock2.Object, true, AttachmentHandlingOption.None);

        // Act
        var result = _sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.None, result);
    }

    [Fact]
    public void IsEnabled_Should_Return_False_When_All_Formatters_Are_Disabled()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, false, AttachmentHandlingOption.Embed);
        _sut.FormatterInitialized(_formatterMock2.Object, false, AttachmentHandlingOption.Embed);

        // Act
        var result = _sut.IsEnabled;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AggregateAttachmentHandlingOption_Should_Be_Default_When_All_Formatters_Disabled()
    {
        // Arrange
        _sut.Initialize();
        _sut.FormatterInitialized(_formatterMock1.Object, false, AttachmentHandlingOption.External);
        _sut.FormatterInitialized(_formatterMock2.Object, false, AttachmentHandlingOption.External);

        // Act
        var result = _sut.AggregateAttachmentHandlingOption;

        // Assert
        Assert.Equal(AttachmentHandlingOption.Embed, result);
    }
}