using Moq;
using Reqnroll.Formatters.PubSub;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Formatters;

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
        _sut.FormatterInitialized(_formatterMock1.Object, true);

        Assert.False(_sut.IsEnabled); // should not be enabled until after both formatters are registered

        _sut.FormatterInitialized(_formatterMock2.Object, true);

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
        _sut.FormatterInitialized(_formatterMock1.Object, true);
        _sut.FormatterInitialized(_formatterMock2.Object, true);

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
        _sut.FormatterInitialized(_formatterMock1.Object, true);
        _sut.FormatterInitialized(_formatterMock2.Object, true);

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
}