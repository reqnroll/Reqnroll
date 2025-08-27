using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess
{
    /// <summary>
    /// Provides build metadata information from various CI/CD environments by reading their specific environment variables.
    /// Supports multiple build servers including Azure Pipelines, TeamCity, Jenkins, GitHub Actions, GitLab CI, and others.
    /// </summary>
    public class BuildMetadataProvider(IEnvironmentInfoProvider environmentInfoProvider, IEnvironmentWrapper environment) : IBuildMetadataProvider
    {
        /// <summary>
        /// Retrieves the value of an environment variable safely.
        /// </summary>
        /// <param name="variable">The name of the environment variable to retrieve.</param>
        /// <returns>The value of the environment variable if it exists and can be accessed; otherwise, <c>null</c>.</returns>
        internal string GetVariable(string variable)
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
                var sourceBranch = GetVariable("BUILD_SOURCEBRANCH");
                if (!string.IsNullOrEmpty(sourceBranch) && sourceBranch.StartsWith("refs/tags/"))
                    return sourceBranch.Substring("refs/tags/".Length);
                return null;
            }

            // According to the predefined Azure DevOps variables documentation (https://learn.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml)
            // there is no direct way to query the build URL. The BUILD_BUILDURI variable returns a URI, like 'vstfs:///Build/Build/1234' which is not a clickable link.
            // Therefore, we construct the build URL manually from the available Azure DevOps predefined variables.
            // The hint was taken from https://stackoverflow.com/a/52111404/26530.
            var collectionUri = GetVariable("SYSTEM_COLLECTIONURI"); // contains trailing slash
            var teamProject = GetVariable("SYSTEM_TEAMPROJECT");
            var buildId = GetVariable("BUILD_BUILDID");
            var buildUrl = $"{collectionUri}{teamProject}/_build/results?buildId={buildId}&_a=summary";
            var buildNumber = GetVariable("BUILD_BUILDNUMBER");
            var remote = GetVariable("BUILD_REPOSITORY_URI");
            var revision = GetVariable("BUILD_SOURCEVERSION");
            var branch = GetVariable("BUILD_SOURCEBRANCHNAME");
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
            var buildUrl = GetVariable("BUILD_URL");
            var buildNumber = GetVariable("BUILD_NUMBER");
            var remote = GetVariable("TEAMCITY_GIT_REPOSITORY_URL");
            var revision = GetVariable("BUILD_VCS_NUMBER");
            var branch = GetVariable("TEAMCITY_BUILD_BRANCH");
            var tag = GetVariable("TEAMCITY_BUILD_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Jenkins environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Jenkins-specific build information.</returns>
        private BuildMetadata GetJenkinsMetadata()
        {
            var buildUrl = GetVariable("BUILD_URL");
            var buildNumber = GetVariable("BUILD_NUMBER");
            var remote = GetVariable("GIT_URL");
            var revision = GetVariable("GIT_COMMIT");
            var branch = GetVariable("GIT_BRANCH");
            var tag = GetVariable("GIT_TAG_NAME");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from GitHub Actions environment variables.
        /// Constructs build URL and remote URL from GitHub-specific variables and handles branch/tag detection.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with GitHub Actions-specific build information.</returns>
        private BuildMetadata GetGitHubActionsMetadata()
        {
            string repo = GetVariable("GITHUB_REPOSITORY");
            string serverUrl = GetVariable("GITHUB_SERVER_URL");
            string runId = GetVariable("GITHUB_RUN_ID");
            string refType = GetVariable("GITHUB_REF_TYPE");
            string refName = GetVariable("GITHUB_REF_NAME");
            string buildUrl = !string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(repo) && !string.IsNullOrEmpty(runId)
                ? $"{serverUrl}/{repo}/actions/runs/{runId}"
                : null;
            string buildNumber = GetVariable("GITHUB_RUN_NUMBER");
            string remote = !string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(repo) ? $"{serverUrl}/{repo}.git" : "";
            string revision = GetVariable("GITHUB_SHA");
            string branch = refType == "branch" ? refName : null;
            string tag = refType == "tag" ? refName : null;

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from GitLab CI environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with GitLab CI-specific build information.</returns>
        private BuildMetadata GetGitLabMetadata()
        {
            var buildUrl = GetVariable("CI_PIPELINE_URL");
            var buildNumber = GetVariable("CI_PIPELINE_IID");
            var remote = GetVariable("CI_REPOSITORY_URL");
            var revision = GetVariable("CI_COMMIT_SHA");
            var branch = GetVariable("CI_COMMIT_REF_NAME");
            var tag = GetVariable("CI_COMMIT_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from AWS CodeBuild environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with AWS CodeBuild-specific build information.</returns>
        private BuildMetadata GetAwsCodeBuildMetadata()
        {
            var buildUrl = GetVariable("CODEBUILD_BUILD_URL");
            var buildNumber = GetVariable("CODEBUILD_BUILD_NUMBER");
            var remote = GetVariable("CODEBUILD_SOURCE_REPO_URL");
            var revision = GetVariable("CODEBUILD_RESOLVED_SOURCE_VERSION");
            var branch = GetVariable("CODEBUILD_SOURCE_VERSION");
            var tag = GetVariable("CODEBUILD_WEBHOOK_TRIGGER");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Travis CI environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Travis CI-specific build information.</returns>
        private BuildMetadata GetTravisMetadata()
        {
            var buildUrl = GetVariable("TRAVIS_BUILD_WEB_URL");
            var buildNumber = GetVariable("TRAVIS_BUILD_NUMBER");
            var remote = GetVariable("TRAVIS_REPO_SLUG");
            var revision = GetVariable("TRAVIS_COMMIT");
            var branch = GetVariable("TRAVIS_BRANCH");
            var tag = GetVariable("TRAVIS_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from AppVeyor environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with AppVeyor-specific build information.</returns>
        private BuildMetadata GetAppVeyorMetadata()
        {
            var buildUrl = GetVariable("APPVEYOR_URL");
            var buildNumber = GetVariable("APPVEYOR_BUILD_NUMBER");
            var remote = GetVariable("APPVEYOR_REPO_NAME");
            var revision = GetVariable("APPVEYOR_REPO_COMMIT");
            var branch = GetVariable("APPVEYOR_REPO_BRANCH");
            var tag = GetVariable("APPVEYOR_REPO_TAG_NAME");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Bitbucket Pipelines environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Bitbucket Pipelines-specific build information.</returns>
        private BuildMetadata GetBitbucketMetadata()
        {
            var buildUrl = GetVariable("BITBUCKET_BUILD_URL");
            var buildNumber = GetVariable("BITBUCKET_BUILD_NUMBER");
            var remote = GetVariable("BITBUCKET_GIT_SSH_ORIGIN");
            var revision = GetVariable("BITBUCKET_COMMIT");
            var branch = GetVariable("BITBUCKET_BRANCH");
            var tag = GetVariable("BITBUCKET_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Atlassian Bamboo environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Bamboo-specific build information.</returns>
        private BuildMetadata GetBambooMetadata()
        {
            var buildUrl = GetVariable("bamboo_resultsUrl");
            var buildNumber = GetVariable("bamboo_buildNumber");
            var remote = GetVariable("bamboo_repository_git_repositoryUrl");
            var revision = GetVariable("bamboo_repository_revision_number");
            var branch = GetVariable("bamboo_planRepository_branch");
            var tag = GetVariable("bamboo_planRepository_tag");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from CircleCI environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with CircleCI-specific build information.</returns>
        private BuildMetadata GetCircleCiMetadata()
        {
            var buildUrl = GetVariable("CIRCLE_BUILD_URL");
            var buildNumber = GetVariable("CIRCLE_BUILD_NUM");
            var remote = GetVariable("CIRCLE_REPOSITORY_URL");
            var revision = GetVariable("CIRCLE_SHA1");
            var branch = GetVariable("CIRCLE_BRANCH");
            var tag = GetVariable("CIRCLE_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from GoCD environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with GoCD-specific build information.</returns>
        private BuildMetadata GetGoCdMetadata()
        {
            var buildUrl = GetVariable("GO_SERVER_URL");
            var buildNumber = GetVariable("GO_PIPELINE_COUNTER");
            var remote = GetVariable("GO_REPO_URL");
            var revision = GetVariable("GO_REVISION");
            var branch = GetVariable("GO_BRANCH");
            var tag = GetVariable("GO_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Buddy environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Buddy-specific build information.</returns>
        private BuildMetadata GetBuddyMetadata()
        {
            var buildUrl = GetVariable("BUDDY_EXECUTION_URL");
            var buildNumber = GetVariable("BUDDY_EXECUTION_ID");
            var remote = GetVariable("BUDDY_SCM_URL");
            var revision = GetVariable("BUDDY_EXECUTION_REVISION");
            var branch = GetVariable("BUDDY_EXECUTION_BRANCH");
            var tag = GetVariable("BUDDY_EXECUTION_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Nevercode environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Nevercode-specific build information.</returns>
        private BuildMetadata GetNevercodeMetadata()
        {
            var buildUrl = GetVariable("NEVERCODE_BUILD_URL");
            var buildNumber = GetVariable("NEVERCODE_BUILD_NUMBER");
            var remote = GetVariable("NEVERCODE_REPO_URL");
            var revision = GetVariable("NEVERCODE_COMMIT");
            var branch = GetVariable("NEVERCODE_BRANCH");
            var tag = GetVariable("NEVERCODE_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Semaphore environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Semaphore-specific build information.</returns>
        private BuildMetadata GetSemaphoreMetadata()
        {
            var buildUrl = GetVariable("SEMAPHORE_WORKFLOW_URL");
            var buildNumber = GetVariable("SEMAPHORE_PIPELINE_NUMBER");
            var remote = GetVariable("SEMAPHORE_GIT_URL");
            var revision = GetVariable("SEMAPHORE_GIT_SHA");
            var branch = GetVariable("SEMAPHORE_GIT_BRANCH");
            var tag = GetVariable("SEMAPHORE_GIT_TAG_NAME");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from BrowserStack environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with BrowserStack-specific build information.</returns>
        private BuildMetadata GetBrowserStackMetadata()
        {
            var buildUrl = GetVariable("BROWSERSTACK_BUILD_URL");
            var buildNumber = GetVariable("BROWSERSTACK_BUILD_NUMBER");
            var remote = GetVariable("BROWSERSTACK_REPO_URL");
            var revision = GetVariable("BROWSERSTACK_COMMIT");
            var branch = GetVariable("BROWSERSTACK_BRANCH");
            var tag = GetVariable("BROWSERSTACK_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Codefresh environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Codefresh-specific build information.</returns>
        private BuildMetadata GetCodefreshMetadata()
        {
            var buildUrl = GetVariable("CF_BUILD_URL");
            var buildNumber = GetVariable("CF_BUILD_ID");
            var remote = GetVariable("CF_REPO_CLONE_URL");
            var revision = GetVariable("CF_REVISION");
            var branch = GetVariable("CF_BRANCH");
            var tag = GetVariable("CF_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Extracts build metadata from Octopus Deploy environment variables.
        /// </summary>
        /// <returns>A <see cref="BuildMetadata"/> object with Octopus Deploy-specific build information.</returns>
        private BuildMetadata GetOctopusDeployMetadata()
        {
            var buildUrl = GetVariable("OCTOPUS_DEPLOY_BUILD_URL");
            var buildNumber = GetVariable("OCTOPUS_DEPLOY_BUILD_NUMBER");
            var remote = GetVariable("OCTOPUS_DEPLOY_REPO_URL");
            var revision = GetVariable("OCTOPUS_DEPLOY_COMMIT");
            var branch = GetVariable("OCTOPUS_DEPLOY_BRANCH");
            var tag = GetVariable("OCTOPUS_DEPLOY_TAG");

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }
    }
}
