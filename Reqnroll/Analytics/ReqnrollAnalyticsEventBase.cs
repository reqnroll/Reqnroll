using System;

namespace Reqnroll.Analytics
{
    public abstract class ReqnrollAnalyticsEventBase : IAnalyticsEvent
    {
        public abstract string EventName { get; }
        public DateTime UtcDate { get; }
        public string UserId { get; }
        public string SessionId { get; }
        public string Platform { get; }
        public string PlatformDescription { get; }
        public string ReqnrollVersion { get; }
        public string UnitTestProvider { get; }
        public bool IsBuildServer { get; }
        public string BuildServerName { get; }
        public string HashedAssemblyName { get; }
        public string TargetFrameworks { get; }
        public string TargetFramework { get; }
        public bool IsDockerContainer { get; }

        protected ReqnrollAnalyticsEventBase(
            DateTime utcDate,
            string userId,
            string sessionId,
            string platform,
            string platformDescription,
            string reqnrollVersion,
            string unitTestProvider,
            string buildServerName,
            string hashedAssemblyName,
            string targetFrameworks,
            string targetFramework,
            bool isDockerContainer)
        {
            UtcDate = utcDate;
            UserId = userId;
            SessionId = sessionId;
            Platform = platform;
            PlatformDescription = platformDescription;
            ReqnrollVersion = reqnrollVersion;
            UnitTestProvider = unitTestProvider;
            BuildServerName = buildServerName;
            IsBuildServer = !string.IsNullOrWhiteSpace(buildServerName);
            HashedAssemblyName = hashedAssemblyName;
            TargetFrameworks = targetFrameworks;
            TargetFramework = targetFramework;
            IsDockerContainer = isDockerContainer;
        }
    }
}
