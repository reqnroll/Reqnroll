using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess
{
    /// <summary>
    /// Provides build metadata information from various CI/CD environments by reading their specific environment variables.
    /// Supports multiple build servers including Azure Pipelines, TeamCity, Jenkins, GitHub Actions, GitLab CI, and others.
    /// </summary>
    public class BuildMetadataProvider(IEnvironmentInfoProvider environmentInfoProvider, IEnvironmentWrapper environment) : IBuildMetadataProvider
    {
        public const string UNKNOWN = "UNKNOWN";
        /// <summary>
        /// Retrieves the value of an environment variable safely.
        /// </summary>
        /// <param name="variable">The name of the environment variable to retrieve.</param>
        /// <returns>The value of the environment variable if it exists and can be accessed; otherwise, <c>null</c>.</returns>
        private string GetVariableOrUnknown(string variable)
        { 
            var value = GetVariable(variable);
            return string.IsNullOrEmpty(value) ? UNKNOWN : value;
        }
        private string GetVariable(string variable)
        {
            var varValue = environment.GetEnvironmentVariable(variable);
            if (varValue is ISuccess<string> success)
                return success.Result;
            return null;
        }

        /// <summary>
        /// Retrieves build metadata from the current CI/CD environment by detecting the build server type
        /// and extracting relevant information from environment variables.
        /// </summary>
        /// <returns>
        /// A <see cref="BuildMetadata"/> object containing build information such as build URL, number, repository details,
        /// revision, branch, and tag information. Returns <c>null</c> if the build server is not recognized or supported.
        /// </returns>
        public BuildMetadata GetBuildMetadata()
        {
            var buildServer = environmentInfoProvider.GetBuildServerName();
            var buildMetaData = buildServer switch
            {
                BuildServerNames.AzurePipelines => GetAzurePipelinesMetadata(),
                BuildServerNames.TeamCity => GetTeamCityMetadata(),
                BuildServerNames.Jenkins => GetJenkinsMetadata(),
                BuildServerNames.GitHubActions => GetGitHubActionsMetadata(),
                BuildServerNames.GitLabCI => GetGitLabMetadata(),
                BuildServerNames.AwsCodeBuild => GetAwsCodeBuildMetadata(),
                BuildServerNames.TravisCI => GetTravisMetadata(),
                BuildServerNames.AppVeyor => GetAppVeyorMetadata(),
                BuildServerNames.BitbucketPipelines => GetBitbucketMetadata(),
                BuildServerNames.Bamboo => GetBambooMetadata(),
                BuildServerNames.CircleCI => GetCircleCiMetadata(),
                BuildServerNames.GoCD => GetGoCdMetadata(),
                BuildServerNames.Buddy => GetBuddyMetadata(),
                BuildServerNames.Nevercode => GetNevercodeMetadata(),
                BuildServerNames.Semaphore => GetSemaphoreMetadata(),
                BuildServerNames.BrowserStack => GetBrowserStackMetadata(),
                BuildServerNames.Codefresh => GetCodefreshMetadata(),
                BuildServerNames.OctopusDeploy => GetOctopusDeployMetadata(),
                _ => null
            };

            if (buildMetaData == null)
                return null;

            return buildMetaData with { ProductName = buildServer };
        }

        // One function per build server:

        /// <summary>
        /// Extracts build metadata from Azure Pipelines environment variables.
        /// Handles special tag extraction from BUILD_SOURCEBRANCH when it contains refs/tags/ prefix.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Azure Pipelines-specific build information.</returns>
        private BuildMetadata GetAzurePipelinesMetadata()
        {
            string GetAzureTag()
            {
                var sourceBranch = GetVariableOrUnknown("BUILD_SOURCEBRANCH");
                if ((sourceBranch != UNKNOWN) && sourceBranch.StartsWith("refs/tags/"))
                    return sourceBranch.Substring("refs/tags/".Length);
                return UNKNOWN;
            }

            // According to the predefined Azure DevOps variables documentation (https://learn.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml)
            // there is no direct way to query the build URL. The BUILD_BUILDURI variable returns a URI, like 'vstfs:///Build/Build/1234' which is not a clickable link.
            // Therefore, we construct the build URL manually from the available Azure DevOps predefined variables.
            // The hint was taken from https://stackoverflow.com/a/52111404/26530.
            var collectionUri = GetVariableOrUnknown("SYSTEM_COLLECTIONURI"); // contains trailing slash
            var teamProject = GetVariableOrUnknown("SYSTEM_TEAMPROJECT");
            var buildId = GetVariableOrUnknown("BUILD_BUILDID");
            var buildUrl = $"{collectionUri}{teamProject}/_build/results?buildId={buildId}&_a=summary";
            var buildNumber = GetVariableOrUnknown("BUILD_BUILDNUMBER");
            var remote = GetVariableOrUnknown("BUILD_REPOSITORY_URI");
            var revision = GetVariableOrUnknown("BUILD_SOURCEVERSION");
            var branch = GetVariableOrUnknown("BUILD_SOURCEBRANCHNAME");
            var tag = GetAzureTag();

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from TeamCity environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with TeamCity-specific build information.</returns>
        /// <seealso href="https://www.jetbrains.com/help/teamcity/predefined-build-parameters.html">TeamCity Predefined Build Parameters</seealso>
        private BuildMetadata GetTeamCityMetadata()
        {
            var buildUrl = GetVariableOrUnknown("BUILD_URL");
            var buildNumber = GetVariableOrUnknown("BUILD_NUMBER");
            var remote = GetVariableOrUnknown("TEAMCITY_GIT_REPOSITORY_URL");
            var revision = GetVariableOrUnknown("BUILD_VCS_NUMBER");
            var branch = GetVariableOrUnknown("TEAMCITY_BUILD_BRANCH");
            var tag = GetVariableOrUnknown("TEAMCITY_BUILD_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Jenkins environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Jenkins-specific build information.</returns>
        private BuildMetadata GetJenkinsMetadata()
        {
            var buildUrl = GetVariableOrUnknown("BUILD_URL");
            var buildNumber = GetVariableOrUnknown("BUILD_NUMBER");
            var remote = GetVariableOrUnknown("GIT_URL");
            var revision = GetVariableOrUnknown("GIT_COMMIT");
            var branch = GetVariableOrUnknown("GIT_BRANCH");
            var tag = GetVariableOrUnknown("GIT_TAG_NAME");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from GitHub Actions environment variables.
        /// Constructs build URL and remote URL from GitHub-specific variables and handles branch/tag detection.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with GitHub Actions-specific build information.</returns>
        private BuildMetadata GetGitHubActionsMetadata()
        {
            string repo = GetVariableOrUnknown("GITHUB_REPOSITORY");
            string serverUrl = GetVariableOrUnknown("GITHUB_SERVER_URL");
            string runId = GetVariableOrUnknown("GITHUB_RUN_ID");
            string refType = GetVariableOrUnknown("GITHUB_REF_TYPE");
            string refName = GetVariableOrUnknown("GITHUB_REF_NAME");
            string buildUrl = (serverUrl != UNKNOWN) && (repo != UNKNOWN) && (runId != UNKNOWN)
                ? $"{serverUrl}/{repo}/actions/runs/{runId}"
                : UNKNOWN;
            string buildNumber = GetVariableOrUnknown("GITHUB_RUN_NUMBER");
            string remote = (serverUrl != UNKNOWN) && (repo != UNKNOWN) ? $"{serverUrl}/{repo}.git" : UNKNOWN;
            string revision = GetVariableOrUnknown("GITHUB_SHA");
            string branch = refType == "branch" ? refName : UNKNOWN;
            string tag = refType == "tag" ? refName : UNKNOWN;

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from GitLab CI environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with GitLab CI-specific build information.</returns>
        private BuildMetadata GetGitLabMetadata()
        {
            var buildUrl = GetVariableOrUnknown("CI_PIPELINE_URL");
            var buildNumber = GetVariableOrUnknown("CI_PIPELINE_IID");
            var remote = GetVariableOrUnknown("CI_REPOSITORY_URL");
            var revision = GetVariableOrUnknown("CI_COMMIT_SHA");
            var branch = GetVariableOrUnknown("CI_COMMIT_REF_NAME");
            var tag = GetVariableOrUnknown("CI_COMMIT_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from AWS CodeBuild environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with AWS CodeBuild-specific build information.</returns>
        private BuildMetadata GetAwsCodeBuildMetadata()
        {
            var buildUrl = GetVariableOrUnknown("CODEBUILD_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("CODEBUILD_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("CODEBUILD_SOURCE_REPO_URL");
            var revision = GetVariableOrUnknown("CODEBUILD_RESOLVED_SOURCE_VERSION");
            var branch = GetVariableOrUnknown("CODEBUILD_SOURCE_VERSION");
            var tag = GetVariableOrUnknown("CODEBUILD_WEBHOOK_TRIGGER");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Travis CI environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Travis CI-specific build information.</returns>
        private BuildMetadata GetTravisMetadata()
        {
            var buildUrl = GetVariableOrUnknown("TRAVIS_BUILD_WEB_URL");
            var buildNumber = GetVariableOrUnknown("TRAVIS_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("TRAVIS_REPO_SLUG");
            var revision = GetVariableOrUnknown("TRAVIS_COMMIT");
            var branch = GetVariableOrUnknown("TRAVIS_BRANCH");
            var tag = GetVariableOrUnknown("TRAVIS_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from AppVeyor environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with AppVeyor-specific build information.</returns>
        private BuildMetadata GetAppVeyorMetadata()
        {
            var buildUrl = GetVariableOrUnknown("APPVEYOR_URL");
            var buildNumber = GetVariableOrUnknown("APPVEYOR_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("APPVEYOR_REPO_NAME");
            var revision = GetVariableOrUnknown("APPVEYOR_REPO_COMMIT");
            var branch = GetVariableOrUnknown("APPVEYOR_REPO_BRANCH");
            var tag = GetVariableOrUnknown("APPVEYOR_REPO_TAG_NAME");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Bitbucket Pipelines environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Bitbucket Pipelines-specific build information.</returns>
        private BuildMetadata GetBitbucketMetadata()
        {
            var buildUrl = GetVariableOrUnknown("BITBUCKET_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("BITBUCKET_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("BITBUCKET_GIT_SSH_ORIGIN");
            var revision = GetVariableOrUnknown("BITBUCKET_COMMIT");
            var branch = GetVariableOrUnknown("BITBUCKET_BRANCH");
            var tag = GetVariableOrUnknown("BITBUCKET_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Atlassian Bamboo environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Bamboo-specific build information.</returns>
        private BuildMetadata GetBambooMetadata()
        {
            var buildUrl = GetVariableOrUnknown("bamboo_resultsUrl");
            var buildNumber = GetVariableOrUnknown("bamboo_buildNumber");
            var remote = GetVariableOrUnknown("bamboo_repository_git_repositoryUrl");
            var revision = GetVariableOrUnknown("bamboo_repository_revision_number");
            var branch = GetVariableOrUnknown("bamboo_planRepository_branch");
            var tag = GetVariableOrUnknown("bamboo_planRepository_tag");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from CircleCI environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with CircleCI-specific build information.</returns>
        private BuildMetadata GetCircleCiMetadata()
        {
            var buildUrl = GetVariableOrUnknown("CIRCLE_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("CIRCLE_BUILD_NUM");
            var remote = GetVariableOrUnknown("CIRCLE_REPOSITORY_URL");
            var revision = GetVariableOrUnknown("CIRCLE_SHA1");
            var branch = GetVariableOrUnknown("CIRCLE_BRANCH");
            var tag = GetVariableOrUnknown("CIRCLE_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from GoCD environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with GoCD-specific build information.</returns>
        private BuildMetadata GetGoCdMetadata()
        {
            var buildUrl = GetVariableOrUnknown("GO_SERVER_URL");
            var buildNumber = GetVariableOrUnknown("GO_PIPELINE_COUNTER");
            var remote = GetVariableOrUnknown("GO_REPO_URL");
            var revision = GetVariableOrUnknown("GO_REVISION");
            var branch = GetVariableOrUnknown("GO_BRANCH");
            var tag = GetVariableOrUnknown("GO_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Buddy environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Buddy-specific build information.</returns>
        private BuildMetadata GetBuddyMetadata()
        {
            var buildUrl = GetVariableOrUnknown("BUDDY_EXECUTION_URL");
            var buildNumber = GetVariableOrUnknown("BUDDY_EXECUTION_ID");
            var remote = GetVariableOrUnknown("BUDDY_SCM_URL");
            var revision = GetVariableOrUnknown("BUDDY_EXECUTION_REVISION");
            var branch = GetVariableOrUnknown("BUDDY_EXECUTION_BRANCH");
            var tag = GetVariableOrUnknown("BUDDY_EXECUTION_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Nevercode environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Nevercode-specific build information.</returns>
        private BuildMetadata GetNevercodeMetadata()
        {
            var buildUrl = GetVariableOrUnknown("NEVERCODE_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("NEVERCODE_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("NEVERCODE_REPO_URL");
            var revision = GetVariableOrUnknown("NEVERCODE_COMMIT");
            var branch = GetVariableOrUnknown("NEVERCODE_BRANCH");
            var tag = GetVariableOrUnknown("NEVERCODE_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Semaphore environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Semaphore-specific build information.</returns>
        private BuildMetadata GetSemaphoreMetadata()
        {
            var buildUrl = GetVariableOrUnknown("SEMAPHORE_WORKFLOW_URL");
            var buildNumber = GetVariableOrUnknown("SEMAPHORE_PIPELINE_NUMBER");
            var remote = GetVariableOrUnknown("SEMAPHORE_GIT_URL");
            var revision = GetVariableOrUnknown("SEMAPHORE_GIT_SHA");
            var branch = GetVariableOrUnknown("SEMAPHORE_GIT_BRANCH");
            var tag = GetVariableOrUnknown("SEMAPHORE_GIT_TAG_NAME");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from BrowserStack environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with BrowserStack-specific build information.</returns>
        private BuildMetadata GetBrowserStackMetadata()
        {
            var buildUrl = GetVariableOrUnknown("BROWSERSTACK_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("BROWSERSTACK_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("BROWSERSTACK_REPO_URL");
            var revision = GetVariableOrUnknown("BROWSERSTACK_COMMIT");
            var branch = GetVariableOrUnknown("BROWSERSTACK_BRANCH");
            var tag = GetVariableOrUnknown("BROWSERSTACK_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Codefresh environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Codefresh-specific build information.</returns>
        private BuildMetadata GetCodefreshMetadata()
        {
            var buildUrl = GetVariableOrUnknown("CF_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("CF_BUILD_ID");
            var remote = GetVariableOrUnknown("CF_REPO_CLONE_URL");
            var revision = GetVariableOrUnknown("CF_REVISION");
            var branch = GetVariableOrUnknown("CF_BRANCH");
            var tag = GetVariableOrUnknown("CF_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Octopus Deploy environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Octopus Deploy-specific build information.</returns>
        private BuildMetadata GetOctopusDeployMetadata()
        {
            var buildUrl = GetVariableOrUnknown("OCTOPUS_DEPLOY_BUILD_URL");
            var buildNumber = GetVariableOrUnknown("OCTOPUS_DEPLOY_BUILD_NUMBER");
            var remote = GetVariableOrUnknown("OCTOPUS_DEPLOY_REPO_URL");
            var revision = GetVariableOrUnknown("OCTOPUS_DEPLOY_COMMIT");
            var branch = GetVariableOrUnknown("OCTOPUS_DEPLOY_BRANCH");
            var tag = GetVariableOrUnknown("OCTOPUS_DEPLOY_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }
    }
}
