using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Io.Cucumber.Messages.Types;
using Moq;
using Reqnroll.Formatters.ExecutionTracking;
using Reqnroll.Formatters.PubSub;
using Xunit;

namespace Reqnroll.RuntimeTests.Formatters.ExecutionTracking;

public class OrderFixingMessagePublisherTests
{
    private static Envelope CreateEnvelopeWithTestCase(string id = "case1")
    {
        // TestCase constructor: (string id, string pickleId, List<TestStep> testSteps, string testRunStartedId)
        var testCase = new TestCase(id, "pickleId", new List<TestStep>(), "runId");
        return Envelope.Create(testCase);
    }

    private static Envelope CreateEnvelopeWithAttachment(string id = "att1")
    {
        // Attachment constructor: (string body, AttachmentContentEncoding encoding, string mediaType, string fileName, Source source, string testCaseStartedId, string testCaseStepId, string url, string testRunStartedId, string testRunHookStartedId, Timestamp timestamp)
        var attachment = new Attachment("body", AttachmentContentEncoding.IDENTITY, "mediaType", "fileName", null, id, "stepId", null, "runId", null, null);
        return Envelope.Create(attachment);
    }

    [Fact]
    public async Task BuffersNonTestCaseMessagesUntilTestCaseIsPublished()
    {
        var publisherMock = new Mock<IMessagePublisher>();
        var sut = new PickleExecutionTracker.OrderFixingMessagePublisher(publisherMock.Object);
        var attachmentEnvelope = CreateEnvelopeWithAttachment();
        await sut.PublishAsync(attachmentEnvelope);
        publisherMock.Verify(p => p.PublishAsync(It.IsAny<Envelope>()), Times.Never);
    }

    [Fact]
    public async Task FlushesBufferAndSwitchesToPassThruAfterTestCaseIsPublished()
    {
        var publisherMock = new Mock<IMessagePublisher>();
        var sut = new PickleExecutionTracker.OrderFixingMessagePublisher(publisherMock.Object);
        var attachmentEnvelope = CreateEnvelopeWithAttachment();
        var testCaseEnvelope = CreateEnvelopeWithTestCase();
        await sut.PublishAsync(attachmentEnvelope);
        await sut.PublishAsync(testCaseEnvelope);
        publisherMock.Verify(p => p.PublishAsync(testCaseEnvelope), Times.Once);
        publisherMock.Verify(p => p.PublishAsync(attachmentEnvelope), Times.Once);
    }

    [Fact]
    public async Task PassesThroughMessagesAfterTestCaseIsPublished()
    {
        var publisherMock = new Mock<IMessagePublisher>();
        var sut = new PickleExecutionTracker.OrderFixingMessagePublisher(publisherMock.Object);
        var testCaseEnvelope = CreateEnvelopeWithTestCase();
        var attachmentEnvelope = CreateEnvelopeWithAttachment();
        await sut.PublishAsync(testCaseEnvelope);
        await sut.PublishAsync(attachmentEnvelope);
        publisherMock.Verify(p => p.PublishAsync(testCaseEnvelope), Times.Once);
        publisherMock.Verify(p => p.PublishAsync(attachmentEnvelope), Times.Once);
    }

    [Fact]
    public void ThrowsArgumentNullExceptionIfPublisherIsNull()
    {
        Action act = () => { _ = new PickleExecutionTracker.OrderFixingMessagePublisher(null!); };
        act.Should().Throw<ArgumentNullException>();
    }
}
