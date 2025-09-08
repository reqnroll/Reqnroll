using System.Threading.Tasks;
using Reqnroll.CommonModels;

namespace Reqnroll.Analytics
{
    public class AnalyticsTransmitter : IAnalyticsTransmitter
    {
        private readonly IAnalyticsTransmitterSink _analyticsTransmitterSink;
        private readonly IEnvironmentReqnrollTelemetryChecker _environmentReqnrollTelemetryChecker;

        public bool IsEnabled => _environmentReqnrollTelemetryChecker.IsReqnrollTelemetryEnabled();

        public AnalyticsTransmitter(IAnalyticsTransmitterSink analyticsTransmitterSink, IEnvironmentReqnrollTelemetryChecker environmentReqnrollTelemetryChecker)
        {
            _analyticsTransmitterSink = analyticsTransmitterSink;
            _environmentReqnrollTelemetryChecker = environmentReqnrollTelemetryChecker;
        }

        public async Task<IResult> TransmitReqnrollProjectCompilingEventAsync(ReqnrollProjectCompilingEvent projectCompilingEvent)
        {
            return await TransmitEventAsync(projectCompilingEvent);
        }

        public async Task<IResult> TransmitReqnrollProjectRunningEventAsync(ReqnrollProjectRunningEvent projectRunningEvent)
        {
            return await TransmitEventAsync(projectRunningEvent);
        }

        public async Task<IResult> TransmitReqnrollFeatureUseEventAsync(ReqnrollFeatureUseEvent featureUseEvent)
        {
            return await TransmitEventAsync(featureUseEvent);
        }

        public async Task<IResult> TransmitEventAsync(IAnalyticsEvent analyticsEvent)
        {
            if (!IsEnabled)
            {
                return Result.Success();
            }

            return await _analyticsTransmitterSink.TransmitEventAsync(analyticsEvent);
        }
    }
}
