namespace Reqnroll.Analytics.AppInsights
{
    public interface IAppInsightsEventSerializer
    {
        byte[] SerializeAnalyticsEvent(IAnalyticsEvent analyticsEvent, string instrumentationKey);
    }
}
