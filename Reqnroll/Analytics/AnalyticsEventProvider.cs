using System;
using System.Collections.Generic;
using System.Text;
using Reqnroll.Analytics.UserId;
using Reqnroll.UnitTestProvider;
using System.Runtime.InteropServices;
using Reqnroll.EnvironmentAccess;

namespace Reqnroll.Analytics
{
    public class AnalyticsEventProvider(IUserUniqueIdStore userUniqueIdStore, UnitTestProviderConfiguration unitTestProviderConfiguration, IEnvironmentInfoProvider environmentInfoProvider)
        : IAnalyticsEventProvider
    {
        private readonly string _unitTestProvider = unitTestProviderConfiguration.UnitTestProvider;
        private readonly string _sessionId = Guid.NewGuid().ToString("N"); // the session ID helps to correlate events in the same execution, e.g. multiple formatter usages

        public ReqnrollProjectCompilingEvent CreateProjectCompilingEvent(string msbuildVersion, string assemblyName, string targetFrameworks, string targetFramework, string projectGuid)
        {
            string userId = userUniqueIdStore.GetUserId();
            string unitTestProvider = _unitTestProvider;
            string reqnrollVersion = environmentInfoProvider.GetReqnrollVersion();
            string buildServerName = environmentInfoProvider.GetBuildServerName();
            bool isDockerContainer = environmentInfoProvider.IsRunningInDockerContainer();
            string hashedAssemblyName = ToSha256(assemblyName);
            string platform = environmentInfoProvider.GetOSPlatform();
            string platformDescription = RuntimeInformation.OSDescription;

            var compiledEvent = new ReqnrollProjectCompilingEvent(
                DateTime.UtcNow,
                userId,
                _sessionId,
                platform,
                platformDescription,
                reqnrollVersion,
                unitTestProvider,
                buildServerName,
                hashedAssemblyName,
                targetFrameworks,
                targetFramework,
                msbuildVersion,
                projectGuid,
                isDockerContainer);

            return compiledEvent;
        }

        public ReqnrollProjectRunningEvent CreateProjectRunningEvent(string testAssemblyName)
        {
            string userId = userUniqueIdStore.GetUserId();
            string unitTestProvider = _unitTestProvider;
            string reqnrollVersion = environmentInfoProvider.GetReqnrollVersion();
            string targetFramework = environmentInfoProvider.GetNetCoreVersion() ?? RuntimeInformation.FrameworkDescription;
            bool isDockerContainer = environmentInfoProvider.IsRunningInDockerContainer();
            string buildServerName = environmentInfoProvider.GetBuildServerName();

            string hashedAssemblyName = ToSha256(testAssemblyName);
            string platform = environmentInfoProvider.GetOSPlatform();
            string platformDescription = RuntimeInformation.OSDescription;

            var runningEvent = new ReqnrollProjectRunningEvent(
                DateTime.UtcNow,
                userId,
                _sessionId,
                platform,
                platformDescription,
                reqnrollVersion,
                unitTestProvider,
                buildServerName,
                hashedAssemblyName,
                null,
                targetFramework,
                isDockerContainer);
            return runningEvent;
        }

        public ReqnrollFeatureUseEvent CreateFeatureUseEvent(string testAssemblyName, string featureName, Dictionary<string, string> properties, Dictionary<string, double> metrics)
        {
            string userId = userUniqueIdStore.GetUserId();
            string unitTestProvider = _unitTestProvider;
            string reqnrollVersion = environmentInfoProvider.GetReqnrollVersion();
            string targetFramework = environmentInfoProvider.GetNetCoreVersion() ?? RuntimeInformation.FrameworkDescription;
            bool isDockerContainer = environmentInfoProvider.IsRunningInDockerContainer();
            string buildServerName = environmentInfoProvider.GetBuildServerName();

            string hashedAssemblyName = ToSha256(testAssemblyName);
            string platform = environmentInfoProvider.GetOSPlatform();
            string platformDescription = RuntimeInformation.OSDescription;

            var runningEvent = new ReqnrollFeatureUseEvent(
                DateTime.UtcNow,
                userId,
                _sessionId,
                platform,
                platformDescription,
                reqnrollVersion,
                unitTestProvider,
                buildServerName,
                hashedAssemblyName,
                null,
                targetFramework,
                isDockerContainer,
                featureName,
                properties,
                metrics);
            return runningEvent;
        }


        private string ToSha256(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return null;
            }

            var crypt = System.Security.Cryptography.SHA256.Create();
            var stringBuilder = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            foreach (byte theByte in crypto)
            {
                stringBuilder.Append(theByte.ToString("x2"));
            }

            return stringBuilder.ToString();
        }

    }
}
