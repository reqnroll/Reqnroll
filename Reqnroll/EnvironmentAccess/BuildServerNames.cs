namespace Reqnroll.EnvironmentAccess
{
    /// <summary>
    /// Contains constant values for build server names used across environment detection and metadata extraction.
    /// </summary>
    public static class BuildServerNames
    {
        /// <summary>
        /// Microsoft Azure Pipelines (formerly Visual Studio Team Services).
        /// </summary>
        public const string AzurePipelines = "Azure Pipelines";

        /// <summary>
        /// JetBrains TeamCity continuous integration server.
        /// </summary>
        public const string TeamCity = "TeamCity";

        /// <summary>
        /// Jenkins automation server.
        /// </summary>
        public const string Jenkins = "Jenkins";

        /// <summary>
        /// GitHub Actions CI/CD platform.
        /// </summary>
        public const string GitHubActions = "GitHub Actions";

        /// <summary>
        /// GitLab CI/CD platform.
        /// </summary>
        public const string GitLabCI = "GitLab CI";

        /// <summary>
        /// Amazon Web Services CodeBuild.
        /// </summary>
        public const string AwsCodeBuild = "AWS CodeBuild";

        /// <summary>
        /// Travis CI continuous integration service.
        /// </summary>
        public const string TravisCI = "Travis CI";

        /// <summary>
        /// AppVeyor continuous integration service.
        /// </summary>
        public const string AppVeyor = "AppVeyor";

        /// <summary>
        /// Bitbucket Pipelines CI/CD service.
        /// </summary>
        public const string BitbucketPipelines = "Bitbucket Pipelines";

        /// <summary>
        /// Atlassian Bamboo continuous integration server.
        /// </summary>
        public const string Bamboo = "Bamboo";

        /// <summary>
        /// CircleCI continuous integration platform.
        /// </summary>
        public const string CircleCI = "CircleCI";

        /// <summary>
        /// GoCD continuous delivery server.
        /// </summary>
        public const string GoCD = "GoCD";

        /// <summary>
        /// Buddy CI/CD platform.
        /// </summary>
        public const string Buddy = "Buddy";

        /// <summary>
        /// Nevercode mobile CI/CD platform.
        /// </summary>
        public const string Nevercode = "Nevercode";

        /// <summary>
        /// Semaphore continuous integration platform.
        /// </summary>
        public const string Semaphore = "Semaphore";

        /// <summary>
        /// BrowserStack testing platform.
        /// </summary>
        public const string BrowserStack = "BrowserStack";

        /// <summary>
        /// Codefresh CI/CD platform.
        /// </summary>
        public const string Codefresh = "Codefresh";

        /// <summary>
        /// Octopus Deploy deployment automation server.
        /// </summary>
        public const string OctopusDeploy = "Octopus Deploy";

        /// <summary>
        /// CodeShip continuous integration service.
        /// </summary>
        public const string CodeShip = "CodeShip";
    }
}