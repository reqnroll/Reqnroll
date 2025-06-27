using Moq;
using Reqnroll.BoDi;
using Reqnroll.Formatters.PubSub;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Io.Cucumber.Messages.Types;
using Reqnroll.Formatters.RuntimeSupport;
using Reqnroll.Plugins;

namespace Reqnroll.RuntimeTests.Formatters.PubSub
{
    public class CucumberMessageBrokerTests
    {
        private readonly Mock<IObjectContainer> _objectContainerMock;
        private readonly Mock<IFormatterLog> _logMock;
        private readonly Mock<IBindingMessagesGenerator> _bindingMessageGeneratorMock;
        private readonly Mock<ICucumberMessageSink> _sinkMock1;
        private readonly Mock<ICucumberMessageSink> _sinkMock2;
        private readonly CucumberMessageBroker _sut;

        public CucumberMessageBrokerTests()
        {
            _objectContainerMock = new Mock<IObjectContainer>();
            _logMock = new Mock<IFormatterLog>();
            _bindingMessageGeneratorMock = new Mock<IBindingMessagesGenerator>();
            _sinkMock1 = new Mock<ICucumberMessageSink>();
            _sinkMock1.As<IRuntimePlugin>();
            _sinkMock1.Setup(s => s.Name).Returns("sink1");
            _sinkMock2 = new Mock<ICucumberMessageSink>();
            _sinkMock2.As<IRuntimePlugin>();
            _sinkMock2.Setup(s => s.Name).Returns("sink2");
            _objectContainerMock.Setup(c => c.ResolveAll<IRuntimePlugin>()).Returns([(IRuntimePlugin)_sinkMock1.Object, (IRuntimePlugin) _sinkMock2.Object]);
            // Initialize the system under test (SUT)
            _sut = new CucumberMessageBroker(_logMock.Object, _objectContainerMock.Object);
        }

        [Fact]
        public async Task Enabled_Should_Return_True_When_Sinks_Are_Registered()
        {
            // Arrange
            _sut.SinkInitialized(_sinkMock1.Object, true);

            Assert.False(_sut.Enabled); // should not be enabled until after both sinks are registered

            _sut.SinkInitialized(_sinkMock2.Object, true);

            // Act
            var result = _sut.Enabled;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Enabled_Should_Return_False_When_No_Sinks_Are_Registered()
        {
            var log = new Mock<IFormatterLog>();

            var sut = new CucumberMessageBroker(log.Object, _objectContainerMock.Object);

            // Act
            var result = sut.Enabled;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PublishAsync_Should_Invoke_PublishAsync_On_All_Sinks()
        {
            // Arrange
            _sut.SinkInitialized(_sinkMock1.Object, true);
            _sut.SinkInitialized(_sinkMock2.Object, true);

            var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "testStart"));

            // Act
            await _sut.PublishAsync(message);

            // Assert
            _sinkMock1.Verify(sink => sink.PublishAsync(message), Times.Once);
            _sinkMock2.Verify(sink => sink.PublishAsync(message), Times.Once);
        }

        [Fact]
        public async Task PublishAsync_Should_Swallow_Exceptions_From_Sinks()
        {
            // Arrange
            _sut.SinkInitialized(_sinkMock1.Object, true);
            _sut.SinkInitialized(_sinkMock2.Object, true);

            var message = Envelope.Create(new TestRunStarted(new Timestamp(1, 0), "testStart"));

            _sinkMock1
                .Setup(sink => sink.PublishAsync(message))
                .ThrowsAsync(new System.Exception("Sink 1 failed"));

            // Act
            var exception = await Record.ExceptionAsync(() => _sut.PublishAsync(message));

            // Assert
            Assert.Null(exception); // No exception should propagate
            _sinkMock2.Verify(sink => sink.PublishAsync(message), Times.Once); // Other sinks should still be called
        }
    }
}
