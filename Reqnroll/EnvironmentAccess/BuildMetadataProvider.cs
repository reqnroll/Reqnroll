using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess
{
    public class BuildMetadataProvider(IEnvironmentInfoProvider environmentInfoProvider, IEnvironmentWrapper environment) : IBuildMetadataProvider
    {
        internal string GetVariable(string variable)
        {
            var varValue = environment.GetEnvironmentVariable(variable);
            if (varValue is ISuccess<string> success)
                return success.Result;
            return null;
        }
        public BuildMetadata GetBuildMetadata()
        {
            var buildServer = environmentInfoProvider.GetBuildServerName();
            var buildMetaData = buildServer switch
            {
                "Azure Pipelines" => GetAzurePipelinesMetadata(),
                "TeamCity" => GetTeamCityMetadata(),
                "Jenkins" => GetJenkinsMetadata(),
                "GitHub Actions" => GetGitHubActionsMetadata(),
                "GitLab CI" => GetGitLabMetadata(),
                "AWS CodeBuild" => GetAwsCodeBuildMetadata(),
                "Travis CI" => GetTravisMetadata(),
                "AppVeyor" => GetAppVeyorMetadata(),
                "Bitbucket Pipelines" => GetBitbucketMetadata(),
                "Bamboo" => GetBambooMetadata(),
                "CircleCI" => GetCircleCiMetadata(),
                "GoCD" => GetGoCdMetadata(),
                "Buddy" => GetBuddyMetadata(),
                "Nevercode" => GetNevercodeMetadata(),
                "Semaphore" => GetSemaphoreMetadata(),
                "BrowserStack" => GetBrowserStackMetadata(),
                "Codefresh" => GetCodefreshMetadata(),
                "Octopus Deploy" => GetOctopusDeployMetadata(),
                _ => null
            };

            if (buildMetaData == null)
                return null;

            return buildMetaData with { ProductName = buildServer };
        }


        // One function per build server:

        private BuildMetadata GetAzurePipelinesMetadata()
        {
            string GetAzureTag()
            {
                var sourceBranch = GetVariable("BUILD_SOURCEBRANCH");
                if (!string.IsNullOrEmpty(sourceBranch) && sourceBranch.StartsWith("refs/tags/"))
                    return sourceBranch.Substring("refs/tags/".Length);
                return null;
            }

            var buildUrl = GetVariable("BUILD_BUILDURI");
            var buildNumber = GetVariable("BUILD_BUILDNUMBER");
            var remote = GetVariable("BUILD_REPOSITORY_URI");
            var revision = GetVariable("BUILD_SOURCEVERSION");
            var branch = GetVariable("BUILD_SOURCEBRANCHNAME");
            var tag = GetAzureTag();

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

        /// <summary>
        /// Gets TeamCity build metadata.
        /// See: https://www.jetbrains.com/help/teamcity/predefined-build-parameters.html
        /// </summary>
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
            string remote = !string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(repo) ? $"{serverUrl}/{repo}.git" : null;
            string revision = GetVariable("GITHUB_SHA");
            string branch = refType == "branch" ? refName : null;
            string tag = refType == "tag" ? refName : null;

            return new BuildMetadata(buildUrl, buildNumber, remote, revision, branch, tag);
        }

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
