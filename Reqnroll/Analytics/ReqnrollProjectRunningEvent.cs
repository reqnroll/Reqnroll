using System;

namespace Reqnroll.Analytics
{
    public class ReqnrollProjectRunningEvent : ReqnrollAnalyticsEventBase
    {
        public ReqnrollProjectRunningEvent(DateTime utcDate, string userId, string sessionId, string platform, string platformDescription, string reqnrollVersion, string unitTestProvider, string buildServerName, string hashedAssemblyName, string targetFrameworks, string targetFramework, bool isDockerContainer) 
            : base(utcDate, userId, sessionId, platform, platformDescription, reqnrollVersion, unitTestProvider, buildServerName, hashedAssemblyName, targetFrameworks, targetFramework, isDockerContainer)
        {
        }

        public override string EventName => "Running Reqnroll project";
    }
}