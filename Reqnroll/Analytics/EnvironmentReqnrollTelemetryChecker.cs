using System;

namespace Reqnroll.Analytics
{
    public class EnvironmentReqnrollTelemetryChecker : IEnvironmentReqnrollTelemetryChecker
    {
        public const string ReqnrollTelemetryEnvironmentVariable = "REQNROLL_TELEMETRY_ENABLED";

        public bool IsReqnrollTelemetryEnabled()
        {
            var reqnrollTelemetry = Environment.GetEnvironmentVariable(ReqnrollTelemetryEnvironmentVariable);
            return reqnrollTelemetry == null || reqnrollTelemetry.Equals("1");
        }
    }
}
