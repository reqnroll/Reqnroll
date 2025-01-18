using System.Text.Json;

namespace Reqnroll.Analytics.AppInsights
{
    public class AppInsightsEventSerializer : IAppInsightsEventSerializer
    {
        public byte[] SerializeAnalyticsEvent(IAnalyticsEvent analyticsEvent, string instrumentationKey)
        {
            var eventTelemetry = new AppInsightsEventTelemetry(analyticsEvent, instrumentationKey);
            return JsonSerializer.SerializeToUtf8Bytes(eventTelemetry, AppInsightsEventTelemetryJsonSourceGenerator.Default.AppInsightsEventTelemetry);
        }
    }
}