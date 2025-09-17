using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Reqnroll.EnvironmentAccess;
using Xunit;

namespace Reqnroll.RuntimeTests.EnvironmentAccess
{
    public class BuildMetadataProviderTests
    {
        private class EnvironmentInfoProviderStub : IEnvironmentInfoProvider
        {
            private readonly string _buildServerName;
            public EnvironmentInfoProviderStub(string buildServerName) => _buildServerName = buildServerName;
            public string GetBuildServerName() => _buildServerName;
            public string GetOSPlatform() => "TestOS";
            public bool IsRunningInDockerContainer() => false;
            public string GetReqnrollVersion() => "1.0.0";
            public string GetNetCoreVersion() => "8.0";
        }

        public static IEnumerable<object[]> BuildServers => new[]
        {
            new object[] { "Azure Pipelines", new Dictionary<string, string> {
                ["SYSTEM_COLLECTIONURI"] = "https://url/", ["SYSTEM_TEAMPROJECT"] = "myprj", ["BUILD_BUILDID"] = "123", ["BUILD_BUILDNUMBER"] = "num", ["BUILD_REPOSITORY_URI"] = "repo", ["BUILD_SOURCEVERSION"] = "rev", ["BUILD_SOURCEBRANCHNAME"] = "branch", ["BUILD_SOURCEBRANCH"] = "refs/tags/v1.2.3" } },
            new object[] { "TeamCity", new Dictionary<string, string> {
                ["BUILD_URL"] = "url", ["BUILD_NUMBER"] = "num", ["TEAMCITY_GIT_REPOSITORY_URL"] = "repo", ["BUILD_VCS_NUMBER"] = "rev", ["TEAMCITY_BUILD_BRANCH"] = "branch", ["TEAMCITY_BUILD_TAG"] = "tag" } },
            new object[] { "Jenkins", new Dictionary<string, string> {
                ["BUILD_URL"] = "url", ["BUILD_NUMBER"] = "num", ["GIT_URL"] = "repo", ["GIT_COMMIT"] = "rev", ["GIT_BRANCH"] = "branch", ["GIT_TAG_NAME"] = "tag" } },
            new object[] { "GitHub Actions", new Dictionary<string, string> {
                ["GITHUB_REPOSITORY"] = "repo", ["GITHUB_SERVER_URL"] = "url", ["GITHUB_RUN_ID"] = "runid", ["GITHUB_RUN_NUMBER"] = "num", ["GITHUB_SHA"] = "rev", ["GITHUB_REF_TYPE"] = "branch", ["GITHUB_REF_NAME"] = "branchname" } },
            new object[] { "GitLab CI", new Dictionary<string, string> {
                ["CI_PIPELINE_URL"] = "url", ["CI_PIPELINE_IID"] = "num", ["CI_REPOSITORY_URL"] = "repo", ["CI_COMMIT_SHA"] = "rev", ["CI_COMMIT_REF_NAME"] = "branch", ["CI_COMMIT_TAG"] = "tag" } },
            new object[] { "AWS CodeBuild", new Dictionary<string, string> {
                ["CODEBUILD_BUILD_URL"] = "url", ["CODEBUILD_BUILD_NUMBER"] = "num", ["CODEBUILD_SOURCE_REPO_URL"] = "repo", ["CODEBUILD_RESOLVED_SOURCE_VERSION"] = "rev", ["CODEBUILD_SOURCE_VERSION"] = "branch", ["CODEBUILD_WEBHOOK_TRIGGER"] = "tag" } },
            new object[] { "Travis CI", new Dictionary<string, string> {
                ["TRAVIS_BUILD_WEB_URL"] = "url", ["TRAVIS_BUILD_NUMBER"] = "num", ["TRAVIS_REPO_SLUG"] = "repo", ["TRAVIS_COMMIT"] = "rev", ["TRAVIS_BRANCH"] = "branch", ["TRAVIS_TAG"] = "tag" } },
            new object[] { "AppVeyor", new Dictionary<string, string> {
                ["APPVEYOR_URL"] = "url", ["APPVEYOR_BUILD_NUMBER"] = "num", ["APPVEYOR_REPO_NAME"] = "repo", ["APPVEYOR_REPO_COMMIT"] = "rev", ["APPVEYOR_REPO_BRANCH"] = "branch", ["APPVEYOR_REPO_TAG_NAME"] = "tag" } },
            new object[] { "Bitbucket Pipelines", new Dictionary<string, string> {
                ["BITBUCKET_BUILD_URL"] = "url", ["BITBUCKET_BUILD_NUMBER"] = "num", ["BITBUCKET_GIT_SSH_ORIGIN"] = "repo", ["BITBUCKET_COMMIT"] = "rev", ["BITBUCKET_BRANCH"] = "branch", ["BITBUCKET_TAG"] = "tag" } },
            new object[] { "Bamboo", new Dictionary<string, string> {
                ["bamboo_resultsUrl"] = "url", ["bamboo_buildNumber"] = "num", ["bamboo_repository_git_repositoryUrl"] = "repo", ["bamboo_repository_revision_number"] = "rev", ["bamboo_planRepository_branch"] = "branch", ["bamboo_planRepository_tag"] = "tag" } },
            new object[] { "CircleCI", new Dictionary<string, string> {
                ["CIRCLE_BUILD_URL"] = "url", ["CIRCLE_BUILD_NUM"] = "num", ["CIRCLE_REPOSITORY_URL"] = "repo", ["CIRCLE_SHA1"] = "rev", ["CIRCLE_BRANCH"] = "branch", ["CIRCLE_TAG"] = "tag" } },
            new object[] { "GoCD", new Dictionary<string, string> {
                ["GO_SERVER_URL"] = "url", ["GO_PIPELINE_COUNTER"] = "num", ["GO_REPO_URL"] = "repo", ["GO_REVISION"] = "rev", ["GO_BRANCH"] = "branch", ["GO_TAG"] = "tag" } },
            new object[] { "Buddy", new Dictionary<string, string> {
                ["BUDDY_EXECUTION_URL"] = "url", ["BUDDY_EXECUTION_ID"] = "num", ["BUDDY_SCM_URL"] = "repo", ["BUDDY_EXECUTION_REVISION"] = "rev", ["BUDDY_EXECUTION_BRANCH"] = "branch", ["BUDDY_EXECUTION_TAG"] = "tag" } },
            new object[] { "Nevercode", new Dictionary<string, string> {
                ["NEVERCODE_BUILD_URL"] = "url", ["NEVERCODE_BUILD_NUMBER"] = "num", ["NEVERCODE_REPO_URL"] = "repo", ["NEVERCODE_COMMIT"] = "rev", ["NEVERCODE_BRANCH"] = "branch", ["NEVERCODE_TAG"] = "tag" } },
            new object[] { "Semaphore", new Dictionary<string, string> {
                ["SEMAPHORE_WORKFLOW_URL"] = "url", ["SEMAPHORE_PIPELINE_NUMBER"] = "num", ["SEMAPHORE_GIT_URL"] = "repo", ["SEMAPHORE_GIT_SHA"] = "rev", ["SEMAPHORE_GIT_BRANCH"] = "branch", ["SEMAPHORE_GIT_TAG_NAME"] = "tag" } },
            new object[] { "BrowserStack", new Dictionary<string, string> {
                ["BROWSERSTACK_BUILD_URL"] = "url", ["BROWSERSTACK_BUILD_NUMBER"] = "num", ["BROWSERSTACK_REPO_URL"] = "repo", ["BROWSERSTACK_COMMIT"] = "rev", ["BROWSERSTACK_BRANCH"] = "branch", ["BROWSERSTACK_TAG"] = "tag" } },
            new object[] { "Codefresh", new Dictionary<string, string> {
                ["CF_BUILD_URL"] = "url", ["CF_BUILD_ID"] = "num", ["CF_REPO_CLONE_URL"] = "repo", ["CF_REVISION"] = "rev", ["CF_BRANCH"] = "branch", ["CF_TAG"] = "tag" } },
            new object[] { "Octopus Deploy", new Dictionary<string, string> {
                ["OCTOPUS_DEPLOY_BUILD_URL"] = "url", ["OCTOPUS_DEPLOY_BUILD_NUMBER"] = "num", ["OCTOPUS_DEPLOY_REPO_URL"] = "repo", ["OCTOPUS_DEPLOY_COMMIT"] = "rev", ["OCTOPUS_DEPLOY_BRANCH"] = "branch", ["OCTOPUS_DEPLOY_TAG"] = "tag" } },
        };

        [Theory]
        [MemberData(nameof(BuildServers))]
        public void GetBuildMetadata_ReturnsExpectedValues(string buildServer, Dictionary<string, string> envVars)
        {
            var env = new EnvironmentWrapperStub();
            foreach (var kv in envVars)
                env.SetEnvironmentVariable(kv.Key, kv.Value);
            var info = new EnvironmentInfoProviderStub(buildServer);
            var provider = new BuildMetadataProvider(info, env);
            var metadata = provider.GetBuildMetadata();
            
            metadata.Should().NotBeNull();
            metadata.ProductName.Should().Be(buildServer);
            
            // Validate all BuildMetadata properties based on build server
            switch (buildServer)
            {
                case "Azure Pipelines":
                    metadata.BuildUrl.Should().Be("https://url/myprj/_build/results?buildId=123&_a=summary");
                    metadata.BuildNumber.Should().Be("num");
                    metadata.Remote.Should().Be("repo");
                    metadata.Revision.Should().Be("rev");
                    metadata.Branch.Should().Be("branch");
                    metadata.Tag.Should().Be("v1.2.3"); // Azure extracts tag from refs/tags/v1.2.3
                    break;
                    
                case "TeamCity":
                case "Jenkins":
                case "GitLab CI":
                case "AWS CodeBuild":
                case "Travis CI":
                case "AppVeyor":
                case "Bitbucket Pipelines":
                case "Bamboo":
                case "CircleCI":
                case "GoCD":
                case "Buddy":
                case "Nevercode":
                case "Semaphore":
                case "BrowserStack":
                case "Codefresh":
                case "Octopus Deploy":
                    metadata.BuildUrl.Should().Be("url");
                    metadata.BuildNumber.Should().Be("num");
                    metadata.Remote.Should().Be("repo");
                    metadata.Revision.Should().Be("rev");
                    metadata.Branch.Should().Be("branch");
                    metadata.Tag.Should().Be("tag");
                    break;
                    
                case "GitHub Actions":
                    metadata.BuildUrl.Should().Be("url/repo/actions/runs/runid");
                    metadata.BuildNumber.Should().Be("num");
                    metadata.Remote.Should().Be("url/repo.git");
                    metadata.Revision.Should().Be("rev");
                    metadata.Branch.Should().Be("branchname"); // GitHub uses GITHUB_REF_NAME when REF_TYPE is branch
                    metadata.Tag.Should().BeNull(); // No tag since REF_TYPE is branch
                    break;
            }
        }

        [Fact]
        public void GetBuildMetadata_ReturnsNull_ForUnknownBuildServer()
        {
            var env = new EnvironmentWrapperStub();
            var info = new EnvironmentInfoProviderStub("UnknownServer");
            var provider = new BuildMetadataProvider(info, env);
            provider.GetBuildMetadata().Should().BeNull();
        }

        [Fact]
        public void GetBuildMetadata_AzurePipelines_ExtractsTagFromSourceBranch()
        {
            var env = new EnvironmentWrapperStub();
            env.SetEnvironmentVariable("SYSTEM_COLLECTIONURI", "https://dev.azure.com/myorg/");
            env.SetEnvironmentVariable("SYSTEM_TEAMPROJECT", "myproject");
            env.SetEnvironmentVariable("BUILD_BUILDID", "123");
            env.SetEnvironmentVariable("BUILD_BUILDNUMBER", "123.456");
            env.SetEnvironmentVariable("BUILD_REPOSITORY_URI", "https://repo.uri");
            env.SetEnvironmentVariable("BUILD_SOURCEVERSION", "abc123");
            env.SetEnvironmentVariable("BUILD_SOURCEBRANCHNAME", "main");
            env.SetEnvironmentVariable("BUILD_SOURCEBRANCH", "refs/tags/v2.0.0");
            
            var info = new EnvironmentInfoProviderStub("Azure Pipelines");
            var provider = new BuildMetadataProvider(info, env);
            var metadata = provider.GetBuildMetadata();
            
            metadata.Should().NotBeNull();
            metadata.ProductName.Should().Be("Azure Pipelines");
            metadata.BuildUrl.Should().Be("https://dev.azure.com/myorg/myproject/_build/results?buildId=123&_a=summary");
            metadata.BuildNumber.Should().Be("123.456");
            metadata.Remote.Should().Be("https://repo.uri");
            metadata.Revision.Should().Be("abc123");
            metadata.Branch.Should().Be("main");
            metadata.Tag.Should().Be("v2.0.0");
        }

        [Fact]
        public void GetBuildMetadata_AzurePipelines_NoTagWhenSourceBranchNotTag()
        {
            var env = new EnvironmentWrapperStub();
            env.SetEnvironmentVariable("BUILD_BUILDURI", "https://build.uri");
            env.SetEnvironmentVariable("BUILD_BUILDNUMBER", "123");
            env.SetEnvironmentVariable("BUILD_REPOSITORY_URI", "https://repo.uri");
            env.SetEnvironmentVariable("BUILD_SOURCEVERSION", "abc123");
            env.SetEnvironmentVariable("BUILD_SOURCEBRANCHNAME", "main");
            env.SetEnvironmentVariable("BUILD_SOURCEBRANCH", "refs/heads/main");
            
            var info = new EnvironmentInfoProviderStub("Azure Pipelines");
            var provider = new BuildMetadataProvider(info, env);
            var metadata = provider.GetBuildMetadata();
            
            metadata.Should().NotBeNull();
            metadata.Tag.Should().BeNull();
        }

        [Fact]
        public void GetBuildMetadata_GitHubActions_HandlesTagReference()
        {
            var env = new EnvironmentWrapperStub();
            env.SetEnvironmentVariable("GITHUB_REPOSITORY", "owner/repo");
            env.SetEnvironmentVariable("GITHUB_SERVER_URL", "https://github.com");
            env.SetEnvironmentVariable("GITHUB_RUN_ID", "12345");
            env.SetEnvironmentVariable("GITHUB_RUN_NUMBER", "67");
            env.SetEnvironmentVariable("GITHUB_SHA", "abc123");
            env.SetEnvironmentVariable("GITHUB_REF_TYPE", "tag");
            env.SetEnvironmentVariable("GITHUB_REF_NAME", "v1.0.0");
            
            var info = new EnvironmentInfoProviderStub("GitHub Actions");
            var provider = new BuildMetadataProvider(info, env);
            var metadata = provider.GetBuildMetadata();
            
            metadata.Should().NotBeNull();
            metadata.ProductName.Should().Be("GitHub Actions");
            metadata.BuildUrl.Should().Be("https://github.com/owner/repo/actions/runs/12345");
            metadata.BuildNumber.Should().Be("67");
            metadata.Remote.Should().Be("https://github.com/owner/repo.git");
            metadata.Revision.Should().Be("abc123");
            metadata.Branch.Should().BeNull(); // No branch since REF_TYPE is tag
            metadata.Tag.Should().Be("v1.0.0");
        }

        [Fact]
        public void GetBuildMetadata_HandlesNullEnvironmentVariables()
        {
            var env = new EnvironmentWrapperStub();
            // Don't set any environment variables
            
            var info = new EnvironmentInfoProviderStub("TeamCity");
            var provider = new BuildMetadataProvider(info, env);
            var metadata = provider.GetBuildMetadata();
            
            metadata.Should().NotBeNull();
            metadata.ProductName.Should().Be("TeamCity");
            metadata.BuildUrl.Should().BeNull();
            metadata.BuildNumber.Should().BeNull();
            metadata.Remote.Should().BeNull();
            metadata.Revision.Should().BeNull();
            metadata.Branch.Should().BeNull();
            metadata.Tag.Should().BeNull();
        }
    }
}
