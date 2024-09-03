using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess
{
    public class EnvironmentWrapper : IEnvironmentWrapper
    {
        public IResult<string> ResolveEnvironmentVariables(string source)
        {
            if (source is null)
            {
                return Result<string>.Failure(new ArgumentNullException(nameof(source)));
            }

            return Result<string>.Success(Environment.ExpandEnvironmentVariables(source));
        }

        public bool IsEnvironmentVariableSet(string name)
        {
            return Environment.GetEnvironmentVariables().Contains(name);
        }

        public IResult<string> GetEnvironmentVariable(string name)
        {
            if (IsEnvironmentVariableSet(name))
            {
                return Result<string>.Success(Environment.GetEnvironmentVariable(name));
            }

            return Result<string>.Failure($"Environment variable {name} not set");
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        }

        public string GetCurrentDirectory() => Environment.CurrentDirectory;

        public string GetOSPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Windows";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Linux";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "OSX";
            }

            throw new InvalidOperationException("Platform cannot be identified");
        }

        private readonly Dictionary<string, string> buildServerTypes
            = new Dictionary<string, string> {
                { "TF_BUILD","Azure Pipelines"},
                { "TEAMCITY_VERSION","TeamCity"},
                { "JENKINS_HOME","Jenkins"},
                { "GITHUB_ACTIONS","GitHub Actions"},
                { "GITLAB_CI","GitLab CI/CD"},
                { "CODEBUILD_BUILD_ID","AWS CodeBuild"},
                { "TRAVIS","Travis CI"},
                { "APPVEYOR","AppVeyor"},
                { "BITBUCKET_BUILD_NUMBER", "Bitbucket Pipelines" },
                { "bamboo_agentId", "Atlassian Bamboo" },
                { "CIRCLECI", "CircleCI" },
                { "GO_PIPELINE_NAME", "GoCD" },
                { "BUDDY", "Buddy" },
                { "NEVERCODE", "Nevercode" },
                { "SEMAPHORE", "SEMAPHORE" },
                { "BROWSERSTACK_USERNAME", "BrowserStack" },
                { "CF_BUILD_ID", "Codefresh" },
                { "TentacleVersion", "Octopus Deploy" },

                { "CI_NAME", "CodeShip" }
            };

        public string GetBuildServerName()
        {
            foreach (var buildServerType in buildServerTypes)
            {
                var envVariable = GetEnvironmentVariable(buildServerType.Key);
                if (envVariable is ISuccess<string>)
                    return buildServerType.Value;
            }
            return null;
        }

        public bool IsRunningInDockerContainer()
        {
            return GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") is ISuccess<string>;
        }

        public string GetReqnrollVersion()
        {
            return VersionInfo.AssemblyInformationalVersion;
        }
        public string GetNetCoreVersion()
        {
            var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            var assemblyPath = assembly.Location.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
            {
                return assemblyPath[netCoreAppIndex + 1];
            }

            return null;
        }

    }
}
