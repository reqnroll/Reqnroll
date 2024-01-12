using System.Threading.Tasks;
using Reqnroll.Analytics.AppInsights;
using Reqnroll.CommonModels;

namespace Reqnroll.Analytics
{
    public interface IAnalyticsTransmitterSink
    {
        Task<IResult> TransmitEventAsync(IAnalyticsEvent analyticsEvent, string instrumentationKey = AppInsightsInstrumentationKey.Key);
    }
}
