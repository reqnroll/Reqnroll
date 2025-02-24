using NSubstitute;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using Xunit;

namespace Reqnroll.RuntimeTests.Infrastructure
{
    public class ReqnrollOutputHelperTests
    {
        private ITestThreadExecutionEventPublisher _testThreadExecutionEventPublisher;

        [Fact]
        public void Should_publish_output_added_event()
        {
            var outputHelper = CreateReqnrollOutputHelper();
            var message = "This is a sample output message!";

            outputHelper.WriteLine(message);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<OutputAddedEvent>(m => m.Text == message));
        }

        [Fact]
        public void Should_publish_attachment_added_event()
        {
            var outputHelper = CreateReqnrollOutputHelper();
            var filePath = @"C:\Temp\sample.png";

            outputHelper.AddAttachment(filePath);

            _testThreadExecutionEventPublisher.Received(1).PublishEvent(Arg.Is<AttachmentAddedEvent>(m => m.FilePath == filePath));
        }

        private ReqnrollOutputHelper CreateReqnrollOutputHelper()
        {
            _testThreadExecutionEventPublisher = Substitute.For<ITestThreadExecutionEventPublisher>();
            var traceListenerMock = Substitute.For<ITraceListener>();
            var attachmentHandlerMock = Substitute.For<IReqnrollAttachmentHandler>();

            return new ReqnrollOutputHelper(_testThreadExecutionEventPublisher, traceListenerMock, attachmentHandlerMock);
        }
    }
}
