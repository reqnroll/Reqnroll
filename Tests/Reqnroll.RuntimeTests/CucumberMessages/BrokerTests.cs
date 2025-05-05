using Moq;
using Reqnroll.BoDi;
using Reqnroll.CucumberMessages.PubSub;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Io.Cucumber.Messages.Types;

namespace Reqnroll.RuntimeTests.CucumberMessages.PubSub
{
    public class CucumberMessageBrokerTests
    {
        private readonly Mock<IObjectContainer> _objectContainerMock;
        private readonly Mock<ICucumberMessageSink> _sinkMock1;
        private readonly Mock<ICucumberMessageSink> _sinkMock2;
        private readonly CucumberMessageBroker _sut;

        public CucumberMessageBrokerTests()
        {
            _objectContainerMock = new Mock<IObjectContainer>();
            _sinkMock1 = new Mock<ICucumberMessageSink>();
            _sinkMock2 = new Mock<ICucumberMessageSink>();

            // Setup the object container to resolve multiple sinks
            _objectContainerMock
                .Setup(container => container.ResolveAll<ICucumberMessageSink>())
                .Returns(new List<ICucumberMessageSink> { _sinkMock1.Object, _sinkMock2.Object });

            // Initialize the system under test (SUT)
            _sut = new CucumberMessageBroker(_objectContainerMock.Object);
        }

        [Fact]
        public void Enabled_Should_Return_True_When_Sinks_Are_Registered()
        {
            // Act
            var result = _sut.Enabled;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Enabled_Should_Return_False_When_No_Sinks_Are_Registered()
        {
            // Arrange
            _objectContainerMock
                .Setup(container => container.ResolveAll<ICucumberMessageSink>())
                .Returns(new List<ICucumberMessageSink>());

            var sut = new CucumberMessageBroker(_objectContainerMock.Object);

            // Act
            var result = sut.Enabled;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PublishAsync_Should_Invoke_PublishAsync_On_All_Sinks()
        {
            // Arrange
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
