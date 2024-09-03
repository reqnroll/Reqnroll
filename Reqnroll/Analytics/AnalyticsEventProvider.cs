using System;
using System.Reflection;
using System.Text;
using Reqnroll.Analytics.UserId;
using Reqnroll.UnitTestProvider;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Reqnroll.EnvironmentAccess;
using Reqnroll.CommonModels;

namespace Reqnroll.Analytics
{
    public class AnalyticsEventProvider : IAnalyticsEventProvider
    {
        private readonly IUserUniqueIdStore _userUniqueIdStore;
        private readonly IEnvironmentWrapper _environmentWrapper;
        private readonly string _unitTestProvider;

        public AnalyticsEventProvider(IUserUniqueIdStore userUniqueIdStore, UnitTestProviderConfiguration unitTestProviderConfiguration, IEnvironmentWrapper environmentWrapper)
        {
            _userUniqueIdStore = userUniqueIdStore;
            _environmentWrapper = environmentWrapper;
            _unitTestProvider = unitTestProviderConfiguration.UnitTestProvider;
        }

        public ReqnrollProjectCompilingEvent CreateProjectCompilingEvent(string msbuildVersion, string assemblyName, string targetFrameworks, string targetFramework, string projectGuid)
        {
            string userId = _userUniqueIdStore.GetUserId();
            string unitTestProvider = _unitTestProvider;
            string reqnrollVersion = _environmentWrapper.GetReqnrollVersion();
            string buildServerName = _environmentWrapper.GetBuildServerName();
            bool isDockerContainer = _environmentWrapper.IsRunningInDockerContainer();
            string hashedAssemblyName = ToSha256(assemblyName);
            string platform = _environmentWrapper.GetOSPlatform();
            string platformDescription = RuntimeInformation.OSDescription;

            var compiledEvent = new ReqnrollProjectCompilingEvent(
                DateTime.UtcNow,
                userId,
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
            string userId = _userUniqueIdStore.GetUserId();
            string unitTestProvider = _unitTestProvider;
            string reqnrollVersion = _environmentWrapper.GetReqnrollVersion();
            string targetFramework = _environmentWrapper.GetNetCoreVersion() ?? RuntimeInformation.FrameworkDescription;
            bool isDockerContainer = _environmentWrapper.IsRunningInDockerContainer();
            string buildServerName = _environmentWrapper.GetBuildServerName();

            string hashedAssemblyName = ToSha256(testAssemblyName);
            string platform = _environmentWrapper.GetOSPlatform();
            string platformDescription = RuntimeInformation.OSDescription;

            var runningEvent = new ReqnrollProjectRunningEvent(
                DateTime.UtcNow,
                userId,
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
