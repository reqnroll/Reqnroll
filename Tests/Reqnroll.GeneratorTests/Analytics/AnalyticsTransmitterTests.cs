using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
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
            var analyticsTransmitterSinkMock = Substitute.For<IAnalyticsTransmitterSink>();
            var environmentReqnrollTelemetryCheckerMock = Substitute.For<IEnvironmentReqnrollTelemetryChecker>();
            environmentReqnrollTelemetryCheckerMock.IsReqnrollTelemetryEnabled()
                                                   .Returns(false);

            var analyticsEventMock = Substitute.For<IAnalyticsEvent>();
            var analyticsTransmitter = new AnalyticsTransmitter(analyticsTransmitterSinkMock, environmentReqnrollTelemetryCheckerMock);

            // ACT
            var result = await analyticsTransmitter.TransmitEventAsync(analyticsEventMock);

            // ASSERT
            result.Should().BeAssignableTo<ISuccess>();
        }

        [Fact]
        public async Task TryTransmitEvent_AnalyticsDisabled_ShouldNotCallSink()
        {
            // ARRANGE
            var analyticsTransmitterSinkMock = Substitute.For<IAnalyticsTransmitterSink>();
            var environmentReqnrollTelemetryCheckerMock = Substitute.For<IEnvironmentReqnrollTelemetryChecker>();
            environmentReqnrollTelemetryCheckerMock.IsReqnrollTelemetryEnabled()
                                                   .Returns(false);

            var analyticsEventMock = Substitute.For<IAnalyticsEvent>();
            var analyticsTransmitter = new AnalyticsTransmitter(analyticsTransmitterSinkMock, environmentReqnrollTelemetryCheckerMock);

            // ACT
            await analyticsTransmitter.TransmitEventAsync(analyticsEventMock);

            // ASSERT
            await analyticsTransmitterSinkMock.DidNotReceive().TransmitEventAsync(Arg.Any<IAnalyticsEvent>(), Arg.Any<string>());
        }

        [Fact]
        public async Task TransmitReqnrollProjectCompilingEvent_AnalyticsEnabled_ShouldCallSink()
        {
            // ARRANGE
            var analyticsTransmitterSinkMock = Substitute.For<IAnalyticsTransmitterSink>();
            var environmentReqnrollTelemetryCheckerMock = Substitute.For<IEnvironmentReqnrollTelemetryChecker>();
            environmentReqnrollTelemetryCheckerMock.IsReqnrollTelemetryEnabled()
                                                   .Returns(true);

            var reqnrollProjectCompilingEvent = Arg.Any<ReqnrollProjectCompilingEvent>();
            var analyticsTransmitter = new AnalyticsTransmitter(analyticsTransmitterSinkMock, environmentReqnrollTelemetryCheckerMock);

            // ACT
            await analyticsTransmitter.TransmitReqnrollProjectCompilingEventAsync(reqnrollProjectCompilingEvent);

            // ASSERT
            await analyticsTransmitterSinkMock.Received(1).TransmitEventAsync(Arg.Any<IAnalyticsEvent>(), Arg.Any<string>());
        }
    }
}
