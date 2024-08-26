using Moq;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public class ReqnrollOutputHelperTests
    {
        private Mock<ITestThreadExecutionEventPublisher> _testThreadExecutionEventPublisher;

        [Fact]
        public void Should_publish_output_added_event()
        {
            var outputHelper = CreateReqnrollOutputHelper();
            var message = "This is a sample output message!";

            outputHelper.WriteLine(message);

            _testThreadExecutionEventPublisher.Verify(ep => ep.PublishEvent(It.Is<OutputAddedEvent>(m => m.Text == message)), Times.Once);
        }

        [Fact]
        public void Should_publish_attachment_added_event()
        {
            var outputHelper = CreateReqnrollOutputHelper();
            var filePath = @"C:\Temp\sample.png";

            outputHelper.AddAttachment(filePath);

            _testThreadExecutionEventPublisher.Verify(ep => ep.PublishEvent(It.Is<AttachmentAddedEvent>(m => m.FilePath == filePath)), Times.Once);
        }

        private ReqnrollOutputHelper CreateReqnrollOutputHelper()
        {
            _testThreadExecutionEventPublisher = new Mock<ITestThreadExecutionEventPublisher>();
            var traceListenerMock = new Mock<ITraceListener>();
            var attachmentHandlerMock = new Mock<IReqnrollAttachmentHandler>();
            var contextManager = new Mock<IContextManager>();

            return new ReqnrollOutputHelper(_testThreadExecutionEventPublisher.Object, traceListenerMock.Object, attachmentHandlerMock.Object, contextManager.Object);
        }
    }
}
