using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Reqnroll.Analytics;

public abstract class AnalyticsTelemetryServiceBase(IAnalyticsTransmitter analyticsTransmitter, IAnalyticsEventProvider analyticsEventProvider)
{
    protected readonly IAnalyticsTransmitter AnalyticsTransmitter = analyticsTransmitter;
    protected readonly IAnalyticsEventProvider AnalyticsEventProvider = analyticsEventProvider;

    protected void SendTelemetryEvent(Func<Task> sendingAction)
    {
        if (!AnalyticsTransmitter.IsEnabled)
        {
            return;
        }

        async Task TrySendTelemetryEventAsync()
        {
            try
            {
                await sendingAction();
            }
            catch (Exception ex)
            {
                // catch all exceptions since we do not want to break anything
                Debug.WriteLine(ex, "Sending telemetry failed");
            }
        }

        _ = Task.Run(TrySendTelemetryEventAsync);
    }
}
