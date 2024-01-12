namespace Reqnroll.Analytics
{
    public interface IAnalyticsEventProvider
    {
        ReqnrollProjectCompilingEvent CreateProjectCompilingEvent(
            string msbuildVersion,
            string assemblyName,
            string targetFrameworks,
            string targetFramework,
            string projectGuid);

        ReqnrollProjectRunningEvent CreateProjectRunningEvent(string testAssemblyName);
    }
}
