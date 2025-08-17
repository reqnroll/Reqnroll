using Reqnroll.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Reqnroll.Analytics;

/// <summary>
/// A helper class that aggregates all dependencies required to send runtime telemetry events.
/// </summary>
public class AnalyticsRuntimeTelemetryService(IAnalyticsTransmitter analyticsTransmitter, IAnalyticsEventProvider analyticsEventProvider, ITestAssemblyProvider testAssemblyProvider) : AnalyticsTelemetryServiceBase(analyticsTransmitter, analyticsEventProvider), IAnalyticsRuntimeTelemetryService
{
    private string GetAssemblyName() => testAssemblyProvider.TestAssembly?.GetName().Name;

    public async Task SendFeatureUseEventAsync(string featureName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null)
    {
        try
        {
            var telemetryEvent = AnalyticsEventProvider.CreateFeatureUseEvent(GetAssemblyName(), featureName, properties, metrics);
            await AnalyticsTransmitter.TransmitReqnrollFeatureUseEventAsync(telemetryEvent);
        }
        catch (Exception ex)
        {
            // catch all exceptions since we do not want to break anything
            Debug.WriteLine(ex, "Sending telemetry failed");
        }
    }
}
