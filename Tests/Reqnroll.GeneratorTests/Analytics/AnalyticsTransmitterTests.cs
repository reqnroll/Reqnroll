using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Reqnroll.Analytics;
using Reqnroll.CommonModels;
using Xunit;

namespace Reqnroll.GeneratorTests.Analytics
{
    public class AnalyticsTransmitterTests
    {
        [Fact]
        public async Task TryTransmitEvent_AnalyticsDisabled_ShouldReturnSuccess()
        {
            // ARRANGE
            var analyticsTransmitterSinkMock = new Mock<IAnalyticsTransmitterSink>();
            var environmentReqnrollTelemetryCheckerMock = new Mock<IEnvironmentReqnrollTelemetryChecker>();
            environmentReqnrollTelemetryCheckerMock.Setup(m => m.IsReqnrollTelemetryEnabled())
                                                   .Returns(false);

            var analyticsEventMock = new Mock<IAnalyticsEvent>();
            var analyticsTransmitter = new AnalyticsTransmitter(analyticsTransmitterSinkMock.Object, environmentReqnrollTelemetryCheckerMock.Object);

            // ACT
            var result = await analyticsTransmitter.TransmitEventAsync(analyticsEventMock.Object);

            // ASSERT
            result.Should().BeAssignableTo<ISuccess>();
        }

        [Fact]
        public async Task TryTransmitEvent_AnalyticsDisabled_ShouldNotCallSink()
        {
            // ARRANGE
            var analyticsTransmitterSinkMock = new Mock<IAnalyticsTransmitterSink>();
            var environmentReqnrollTelemetryCheckerMock = new Mock<IEnvironmentReqnrollTelemetryChecker>();
            environmentReqnrollTelemetryCheckerMock.Setup(m => m.IsReqnrollTelemetryEnabled())
                                                   .Returns(false);

            var analyticsEventMock = new Mock<IAnalyticsEvent>();
            var analyticsTransmitter = new AnalyticsTransmitter(analyticsTransmitterSinkMock.Object, environmentReqnrollTelemetryCheckerMock.Object);

            // ACT
            await analyticsTransmitter.TransmitEventAsync(analyticsEventMock.Object);

            // ASSERT
            analyticsTransmitterSinkMock.Verify(sink => sink.TransmitEventAsync(It.IsAny<IAnalyticsEvent>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task TransmitReqnrollProjectCompilingEvent_AnalyticsEnabled_ShouldCallSink()
        {
            // ARRANGE
            var analyticsTransmitterSinkMock = new Mock<IAnalyticsTransmitterSink>();
            var environmentReqnrollTelemetryCheckerMock = new Mock<IEnvironmentReqnrollTelemetryChecker>();
            environmentReqnrollTelemetryCheckerMock.Setup(m => m.IsReqnrollTelemetryEnabled())
                                                   .Returns(true);

            var reqnrollProjectCompilingEvent = It.IsAny<ReqnrollProjectCompilingEvent>();
            var analyticsTransmitter = new AnalyticsTransmitter(analyticsTransmitterSinkMock.Object, environmentReqnrollTelemetryCheckerMock.Object);

            // ACT
            await analyticsTransmitter.TransmitReqnrollProjectCompilingEventAsync(reqnrollProjectCompilingEvent);

            // ASSERT
            analyticsTransmitterSinkMock.Verify(m => m.TransmitEventAsync(It.IsAny<IAnalyticsEvent>(), It.IsAny<string>()), Times.Once);
        }
    }
}
