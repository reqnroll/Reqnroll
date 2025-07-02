using Reqnroll.CommonModels;

namespace Reqnroll.EnvironmentAccess
{
    public class CiMetadataProvider(IEnvironmentInfoProvider environmentInfoProvider, IEnvironmentWrapper environment) : ICiMetadataProvider
    {
        private string GetVariable(string variable)
        {
            var varValue = environment.GetEnvironmentVariable(variable);
            if (varValue is ISuccess<string>)
                return (varValue as ISuccess<string>).Result;
            return null;
        }
        public CiMetadata GetCiMetadata()
        {
            var buildServer = environmentInfoProvider.GetBuildServerName();
            return buildServer switch
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
        }


        // One function per build server:

        private CiMetadata GetAzurePipelinesMetadata()
        {
            string GetAzureTag()
            {
                var sourceBranch = GetVariable("BUILD_SOURCEBRANCH");
                if (!string.IsNullOrEmpty(sourceBranch) && sourceBranch.StartsWith("refs/tags/"))
                    return sourceBranch.Substring("refs/tags/".Length);
                return null;
            }

            // These are from: https://learn.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml
            return new CiMetadata
            {
                ProductName = "Azure Pipelines",
                BuildUrl = GetVariable("BUILD_BUILDURI"),
                BuildNumber = GetVariable("BUILD_BUILDNUMBER"),
                Remote = GetVariable("BUILD_REPOSITORY_URI"),
                Revision = GetVariable("BUILD_SOURCEVERSION"),
                Branch = GetVariable("BUILD_SOURCEBRANCHNAME"),
                Tag = GetAzureTag()
            };
        }

        // From: https://www.jetbrains.com/help/teamcity/predefined-build-parameters.html
        private CiMetadata GetTeamCityMetadata() => new CiMetadata
        {
            ProductName = "TeamCity",
            BuildUrl = GetVariable("BUILD_URL"),
            BuildNumber = GetVariable("BUILD_NUMBER"),
            Remote = GetVariable("TEAMCITY_GIT_REPOSITORY_URL"),
            Revision = GetVariable("BUILD_VCS_NUMBER"),
            Branch = GetVariable("TEAMCITY_BUILD_BRANCH"),
            Tag = GetVariable("TEAMCITY_BUILD_TAG")
        };

        private CiMetadata GetJenkinsMetadata() => new CiMetadata
        {
            ProductName = "Jenkins",
            BuildUrl = GetVariable("BUILD_URL"),
            BuildNumber = GetVariable("BUILD_NUMBER"),
            Remote = GetVariable("GIT_URL"),
            Revision = GetVariable("GIT_COMMIT"),
            Branch = GetVariable("GIT_BRANCH"),
            Tag = GetVariable("GIT_TAG_NAME")
        };

        private CiMetadata GetGitHubActionsMetadata()
        {
            string repo = GetVariable("GITHUB_REPOSITORY");
            string serverUrl = GetVariable("GITHUB_SERVER_URL");
            string runId = GetVariable("GITHUB_RUN_ID");
            string refType = GetVariable("GITHUB_REF_TYPE");
            string refName = GetVariable("GITHUB_REF_NAME");
            string buildUrl = !string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(repo) && !string.IsNullOrEmpty(runId)
                ? $"{serverUrl}/{repo}/actions/runs/{runId}"
                : null;

            return new CiMetadata
            {
                ProductName = "GitHub Actions",
                BuildUrl = buildUrl,
                BuildNumber = GetVariable("GITHUB_RUN_NUMBER"),
                Remote = !string.IsNullOrEmpty(serverUrl) && !string.IsNullOrEmpty(repo) ? $"{serverUrl}/{repo}.git" : null,
                Revision = GetVariable("GITHUB_SHA"),
                Branch = refType == "branch" ? refName : null,
                Tag = refType == "tag" ? refName : null
            };
        }

        private CiMetadata GetGitLabMetadata() => new CiMetadata
        {
            ProductName = "GitLab CI",
            BuildUrl = GetVariable("CI_PIPELINE_URL"),
            BuildNumber = GetVariable("CI_PIPELINE_IID"),
            Remote = GetVariable("CI_REPOSITORY_URL"),
            Revision = GetVariable("CI_COMMIT_SHA"),
            Branch = GetVariable("CI_COMMIT_REF_NAME"),
            Tag = GetVariable("CI_COMMIT_TAG")
        };

        private CiMetadata GetAwsCodeBuildMetadata() => new CiMetadata
        {
            ProductName = "AWS CodeBuild",
            BuildUrl = GetVariable("CODEBUILD_BUILD_URL"),
            BuildNumber = GetVariable("CODEBUILD_BUILD_NUMBER"),
            Remote = GetVariable("CODEBUILD_SOURCE_REPO_URL"),
            Revision = GetVariable("CODEBUILD_RESOLVED_SOURCE_VERSION"),
            Branch = GetVariable("CODEBUILD_SOURCE_VERSION"),
            Tag = GetVariable("CODEBUILD_WEBHOOK_TRIGGER")
        };

        private CiMetadata GetTravisMetadata() => new CiMetadata
        {
            ProductName = "Travis CI",
            BuildUrl = GetVariable("TRAVIS_BUILD_WEB_URL"),
            BuildNumber = GetVariable("TRAVIS_BUILD_NUMBER"),
            Remote = GetVariable("TRAVIS_REPO_SLUG"),
            Revision = GetVariable("TRAVIS_COMMIT"),
            Branch = GetVariable("TRAVIS_BRANCH"),
            Tag = GetVariable("TRAVIS_TAG")
        };

        private CiMetadata GetAppVeyorMetadata() => new CiMetadata
        {
            ProductName = "AppVeyor",
            BuildUrl = GetVariable("APPVEYOR_URL"),
            BuildNumber = GetVariable("APPVEYOR_BUILD_NUMBER"),
            Remote = GetVariable("APPVEYOR_REPO_NAME"),
            Revision = GetVariable("APPVEYOR_REPO_COMMIT"),
            Branch = GetVariable("APPVEYOR_REPO_BRANCH"),
            Tag = GetVariable("APPVEYOR_REPO_TAG_NAME")
        };

        private CiMetadata GetBitbucketMetadata() => new CiMetadata
        {
            ProductName = "Bitbucket Pipelines",
            BuildUrl = GetVariable("BITBUCKET_BUILD_URL"),
            BuildNumber = GetVariable("BITBUCKET_BUILD_NUMBER"),
            Remote = GetVariable("BITBUCKET_GIT_SSH_ORIGIN"),
            Revision = GetVariable("BITBUCKET_COMMIT"),
            Branch = GetVariable("BITBUCKET_BRANCH"),
            Tag = GetVariable("BITBUCKET_TAG")
        };

        private CiMetadata GetBambooMetadata() => new CiMetadata
        {
            ProductName = "Bamboo",
            BuildUrl = GetVariable("bamboo_resultsUrl"),
            BuildNumber = GetVariable("bamboo_buildNumber"),
            Remote = GetVariable("bamboo_repository_git_repositoryUrl"),
            Revision = GetVariable("bamboo_repository_revision_number"),
            Branch = GetVariable("bamboo_planRepository_branch"),
            Tag = GetVariable("bamboo_planRepository_tag")
        };

        private CiMetadata GetCircleCiMetadata() => new CiMetadata
        {
            ProductName = "CircleCI",
            BuildUrl = GetVariable("CIRCLE_BUILD_URL"),
            BuildNumber = GetVariable("CIRCLE_BUILD_NUM"),
            Remote = GetVariable("CIRCLE_REPOSITORY_URL"),
            Revision = GetVariable("CIRCLE_SHA1"),
            Branch = GetVariable("CIRCLE_BRANCH"),
            Tag = GetVariable("CIRCLE_TAG")
        };

        private CiMetadata GetGoCdMetadata() => new CiMetadata
        {
            ProductName = "GoCD",
            BuildUrl = GetVariable("GO_SERVER_URL"),
            BuildNumber = GetVariable("GO_PIPELINE_COUNTER"),
            Remote = GetVariable("GO_REPO_URL"),
            Revision = GetVariable("GO_REVISION"),
            Branch = GetVariable("GO_BRANCH"),
            Tag = GetVariable("GO_TAG")
        };

        private CiMetadata GetBuddyMetadata() => new CiMetadata
        {
            ProductName = "Buddy",
            BuildUrl = GetVariable("BUDDY_EXECUTION_URL"),
            BuildNumber = GetVariable("BUDDY_EXECUTION_ID"),
            Remote = GetVariable("BUDDY_SCM_URL"),
            Revision = GetVariable("BUDDY_EXECUTION_REVISION"),
            Branch = GetVariable("BUDDY_EXECUTION_BRANCH"),
            Tag = GetVariable("BUDDY_EXECUTION_TAG")
        };

        private CiMetadata GetNevercodeMetadata() => new CiMetadata
        {
            ProductName = "Nevercode",
            BuildUrl = GetVariable("NEVERCODE_BUILD_URL"),
            BuildNumber = GetVariable("NEVERCODE_BUILD_NUMBER"),
            Remote = GetVariable("NEVERCODE_REPO_URL"),
            Revision = GetVariable("NEVERCODE_COMMIT"),
            Branch = GetVariable("NEVERCODE_BRANCH"),
            Tag = GetVariable("NEVERCODE_TAG")
        };

        private CiMetadata GetSemaphoreMetadata() => new CiMetadata
        {
            ProductName = "Semaphore",
            BuildUrl = GetVariable("SEMAPHORE_WORKFLOW_URL"),
            BuildNumber = GetVariable("SEMAPHORE_PIPELINE_NUMBER"),
            Remote = GetVariable("SEMAPHORE_GIT_URL"),
            Revision = GetVariable("SEMAPHORE_GIT_SHA"),
            Branch = GetVariable("SEMAPHORE_GIT_BRANCH"),
            Tag = GetVariable("SEMAPHORE_GIT_TAG_NAME")
        };

        private CiMetadata GetBrowserStackMetadata() => new CiMetadata
        {
            ProductName = "BrowserStack",
            BuildUrl = GetVariable("BROWSERSTACK_BUILD_URL"),
            BuildNumber = GetVariable("BROWSERSTACK_BUILD_NUMBER"),
            Remote = GetVariable("BROWSERSTACK_REPO_URL"),
            Revision = GetVariable("BROWSERSTACK_COMMIT"),
            Branch = GetVariable("BROWSERSTACK_BRANCH"),
            Tag = GetVariable("BROWSERSTACK_TAG")
        };

        private CiMetadata GetCodefreshMetadata() => new CiMetadata
        {
            ProductName = "Codefresh",
            BuildUrl = GetVariable("CF_BUILD_URL"),
            BuildNumber = GetVariable("CF_BUILD_ID"),
            Remote = GetVariable("CF_REPO_CLONE_URL"),
            Revision = GetVariable("CF_REVISION"),
            Branch = GetVariable("CF_BRANCH"),
            Tag = GetVariable("CF_TAG")
        };

        private CiMetadata GetOctopusDeployMetadata() => new CiMetadata
        {
            ProductName = "Octopus Deploy",
            BuildUrl = GetVariable("OCTOPUS_DEPLOY_BUILD_URL"),
            BuildNumber = GetVariable("OCTOPUS_DEPLOY_BUILD_NUMBER"),
            Remote = GetVariable("OCTOPUS_DEPLOY_REPO_URL"),
            Revision = GetVariable("OCTOPUS_DEPLOY_COMMIT"),
            Branch = GetVariable("OCTOPUS_DEPLOY_BRANCH"),
            Tag = GetVariable("OCTOPUS_DEPLOY_TAG")
        };
    }
}
