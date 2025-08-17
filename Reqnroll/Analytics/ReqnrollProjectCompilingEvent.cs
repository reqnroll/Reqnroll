using System;

namespace Reqnroll.Analytics
{
    public class ReqnrollProjectCompilingEvent : ReqnrollAnalyticsEventBase
    {
        public string MSBuildVersion { get; }
        public string ProjectGuid { get; set; }

        public ReqnrollProjectCompilingEvent(DateTime utcDate, string userId, string sessionId, string platform, string platformDescription, string reqnrollVersion, string unitTestProvider, string buildServerName, string hashedAssemblyName, string targetFrameworks, string targetFramework, string msBuildVersion, string projectGuid, bool isDockerContainer) 
            : base(utcDate, userId, sessionId, platform, platformDescription, reqnrollVersion, unitTestProvider, buildServerName, hashedAssemblyName, targetFrameworks, targetFramework, isDockerContainer)
        {
            MSBuildVersion = msBuildVersion;
            ProjectGuid = projectGuid;
        }

        public override string EventName => "Compiling Reqnroll project";
    }
}