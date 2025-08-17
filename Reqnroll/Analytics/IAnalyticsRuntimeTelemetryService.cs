using System.Collections.Generic;

namespace Reqnroll.Analytics;

/// <summary>
/// Dependency for sending runtime telemetry events.
/// </summary>
public interface IAnalyticsRuntimeTelemetryService
{
    void SendFeatureUseEvent(string featureName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);

    void SendProjectRunningEvent();
}
