using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Analytics;

/// <summary>
/// Dependency for sending runtime telemetry events.
/// </summary>
public interface IAnalyticsRuntimeTelemetryService
{
    Task SendFeatureUseEventAsync(string featureName, Dictionary<string, string> properties = null, Dictionary<string, double> metrics = null);
}
