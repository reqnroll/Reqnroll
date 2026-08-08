using System;

namespace Reqnroll.Diagnostics;

internal static class TelemetrySettings
{
    public static bool IsTelemetryEnabled => Environment.GetEnvironmentVariable("REQNROLL_TELEMETRY_ENABLED") == "1";
}
