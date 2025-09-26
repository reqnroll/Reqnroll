using Moq;
using Reqnroll.Configuration;
using Reqnroll.Events;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;
using System;
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

            _testThreadExecutionEventPublisher.Verify(ep => ep.PublishEventAsync(It.Is<OutputAddedEvent>(m => m.Text == message)), Times.Once);
        }

        [Fact]
        public void Should_publish_attachment_added_event()
        {
            var outputHelper = CreateReqnrollOutputHelper();
            var filePath = @"C:\Temp\sample.png";

            outputHelper.AddAttachment(filePath);

            _testThreadExecutionEventPublisher.Verify(ep => ep.PublishEventAsync(It.Is<AttachmentAddedEvent>(m => m.FilePath == filePath)), Times.Once);
        }

        private ReqnrollOutputHelper CreateReqnrollOutputHelper()
        {
            _testThreadExecutionEventPublisher = new Mock<ITestThreadExecutionEventPublisher>();
            var traceListenerMock = new Mock<ITraceListener>();
            var attachmentHandlerMock = new Mock<IReqnrollAttachmentHandler>();
            var contextManager = new Mock<IContextManager>();
            var featureInfo = new FeatureInfo(new System.Globalization.CultureInfo("en-US"), "", "test feature", null);
            var config = new ReqnrollConfiguration(ConfigSource.Json, null, null, null, null, false, MissingOrPendingStepsOutcome.Error, false, false, TimeSpan.FromSeconds(10), Reqnroll.BindingSkeletons.StepDefinitionSkeletonStyle.CucumberExpressionAttribute, null, false, false, new string[] { }, true, ObsoleteBehavior.Error, false);
            var featureContext = new FeatureContext(null, featureInfo, config);
            contextManager.SetupGet(c => c.FeatureContext).Returns(featureContext);

            return new ReqnrollOutputHelper(_testThreadExecutionEventPublisher.Object, traceListenerMock.Object, attachmentHandlerMock.Object, contextManager.Object);
        }
    }
}
