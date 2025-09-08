using System.Collections.Generic;

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

        ReqnrollFeatureUseEvent CreateFeatureUseEvent(string testAssemblyName, string featureName, Dictionary<string, string> properties, Dictionary<string, double> metrics);
    }
}
