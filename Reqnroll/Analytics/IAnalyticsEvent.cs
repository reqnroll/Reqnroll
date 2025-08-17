using System;

namespace Reqnroll.Analytics
{
    public interface IAnalyticsEvent
    {
        string EventName { get; }
        DateTime UtcDate { get; }
        string UserId { get; }
        string SessionId { get; }
        string Platform { get; }
        string PlatformDescription { get; }
        string ReqnrollVersion { get; }
        string UnitTestProvider { get; }
        bool IsBuildServer { get; }
        string BuildServerName { get; }
        bool IsDockerContainer { get; }
        string HashedAssemblyName { get;}
        string TargetFrameworks { get; }
        string TargetFramework { get; }
    }
}
