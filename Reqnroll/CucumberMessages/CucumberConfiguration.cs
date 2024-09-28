using Reqnroll.CommonModels;
using Reqnroll.EnvironmentAccess;
using Reqnroll.Tracing;
using System;
using System.IO;
using System.Linq;

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class CucumberConfiguration
    {
        internal const string REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY_ENVIRONMENT_VARIABLE = "REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY";
        internal const string REQNROLL_CUCUMBER_MESSAGES_ACTIVE_OUTPUT_PROFILE_ENVIRONMENT_VARIABLE = "REQNROLL_CUCUMBER_MESSAGES_ACTIVE_OUTPUT_PROFILE_ENVIRONMENT_VARIABLE";
        public const string REQNROLL_CUCUMBERMESSAGES_ENABLE_ENVIRONMENT_VARIABLE = "REQNROLL_CUCUMBER_MESSAGES_ENABLED";

        private ITraceListener _trace;
        private IEnvironmentWrapper _environmentWrapper;
        private object _lock = new();

        private CucumberOutputConfiguration outputConfiguration;
        public bool Enabled => outputConfiguration != null ? outputConfiguration.FileOutputEnabled : false;

        public CucumberConfiguration(ITraceListener traceListener, IEnvironmentWrapper environmentWrapper)
        {
            _trace = traceListener;
            _environmentWrapper = environmentWrapper;
        }

        public string ConfigureOutputDirectory(CucumberOutputConfiguration config)
        {
            outputConfiguration = config;
            string outputDirectory;
            string configuredOutputDirectory = string.Empty;
            string defaultOutputDirectory = _environmentWrapper.GetCurrentDirectory();
            var outDirValue = _environmentWrapper.GetEnvironmentVariable(REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY_ENVIRONMENT_VARIABLE);
            var profileValue = _environmentWrapper.GetEnvironmentVariable(REQNROLL_CUCUMBER_MESSAGES_ACTIVE_OUTPUT_PROFILE_ENVIRONMENT_VARIABLE);
            string environmentVariableOutputDirectory = outDirValue is Success<string> ? ((Success<string>)outDirValue).Result : null;
            string profileName = profileValue is Success<string> ? ((Success<string>)profileValue).Result : "DEFAULT";

            var activeConfiguredDestination = config.Destinations.Where(d => d.ProfileName == profileName).FirstOrDefault();

            if (activeConfiguredDestination != null)
            {
                configuredOutputDirectory = Path.Combine(activeConfiguredDestination.BasePath, activeConfiguredDestination.OutputDirectory);
            }

            outputDirectory = defaultOutputDirectory;
            if (!String.IsNullOrEmpty(configuredOutputDirectory))
                outputDirectory = configuredOutputDirectory;
            if (!String.IsNullOrEmpty(environmentVariableOutputDirectory))
                outputDirectory = environmentVariableOutputDirectory;

            string logEntry;
            if (outputDirectory == defaultOutputDirectory) logEntry = $"FileOutputPlugin Initialized from Assembly Location. BasePath: {outputDirectory}";
            else if (outputDirectory == configuredOutputDirectory) logEntry = $"FileOutputPlugin Initialized from Configuration File. BasePath: {configuredOutputDirectory}";
            else logEntry = $"FileOutputPlugin Initialized from Environment Variable. BasePath: {environmentVariableOutputDirectory}";

            _trace!.WriteTestOutput(logEntry);
            if (!Directory.Exists(outputDirectory))
            {
                lock (_lock)
                {
                    if (!Directory.Exists(outputDirectory))
                        Directory.CreateDirectory(outputDirectory);
                }
            }
            return outputDirectory;
        }

    }
}

