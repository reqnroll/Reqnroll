using Reqnroll.Infrastructure;
using System.Collections.Generic;

namespace Reqnroll.Analytics;

/// <summary>
/// A helper class that aggregates all dependencies required to send runtime telemetry events.
/// </summary>
public class AnalyticsRuntimeTelemetryService(IAnalyticsTransmitter analyticsTransmitter, IAnalyticsEventProvider analyticsEventProvider, ITestAssemblyProvider testAssemblyProvider) 
    : AnalyticsTelemetryServiceBase(analyticsTransmitter, analyticsEventProvider), IAnalyticsRuntimeTelemetryService
{
    private string GetAssemblyName() => testAssemblyProvider.TestAssembly?.GetName().Name;

    public void SendProjectRunningEvent()
    {
        SendTelemetryEvent(
            async () =>
            {
                var telemetryEvent = AnalyticsEventProvider.CreateProjectRunningEvent(GetAssemblyName());
                await AnalyticsTransmitter.TransmitReqnrollProjectRunningEventAsync(telemetryEvent);
            });
    }

    public void SendFeatureUseEvent(string featureName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
    {
        SendTelemetryEvent(
            async () =>
            {
                var telemetryEvent = AnalyticsEventProvider.CreateFeatureUseEvent(GetAssemblyName(), featureName, properties, metrics);
                await AnalyticsTransmitter.TransmitReqnrollFeatureUseEventAsync(telemetryEvent);
            });
    }
}
