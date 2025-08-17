namespace Reqnroll.Analytics;

public abstract class AnalyticsTelemetryServiceBase(IAnalyticsTransmitter analyticsTransmitter, IAnalyticsEventProvider analyticsEventProvider)
{
    protected readonly IAnalyticsTransmitter AnalyticsTransmitter = analyticsTransmitter;
    protected readonly IAnalyticsEventProvider AnalyticsEventProvider = analyticsEventProvider;
}
