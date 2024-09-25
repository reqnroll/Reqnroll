using Reqnroll.Time;
using Reqnroll.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reqnoll.CucumberMessage.FileSink.ReqnrollPlugin
{
    public class FileSinkConfiguration
    {
        internal const string REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY_ENVIRONMENT_VARIABLE = "REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY";
        private object _lock = new();

        public bool FileSinkEnabled { get; set; }
        public List<Destination> Destinations { get; set; }

        public FileSinkConfiguration() : this(true) { }
        public FileSinkConfiguration(bool fileSinkEnabled) : this(fileSinkEnabled, new List<Destination>()) { }
        public FileSinkConfiguration(bool fileSinkEnabled, List<Destination> destinations)
        {
            FileSinkEnabled = fileSinkEnabled;
            Destinations = destinations;
        }
        public string ConfiguredOutputDirectory(ITraceListener trace)
        {
            string outputDirectory;
            string configuredOutputDirectory = string.Empty;
            string defaultOutputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string environmentVariableOutputDirectory = Environment.GetEnvironmentVariable(REQNROLL_CUCUMBER_MESSAGES_OUTPUT_DIRECTORY_ENVIRONMENT_VARIABLE);
            var activeConfiguredDestination = Destinations.Where(d => d.Enabled).FirstOrDefault();

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
            if (outputDirectory == defaultOutputDirectory) logEntry = $"FileSinkPlugin Initialized from Assembly Location. BasePath: {outputDirectory}";
            else if (outputDirectory == configuredOutputDirectory) logEntry = $"FileSinkPlugin Initialized from Configuration File. BasePath: {configuredOutputDirectory}";
            else logEntry = $"FileSinkPlugin Initialized from Environment Variable. BasePath: {environmentVariableOutputDirectory}";

            trace?.WriteTestOutput(logEntry);
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

    public class Destination
    {
        public bool Enabled { get; set; }
        public string BasePath { get; set; }
        public string OutputDirectory { get; set; }

        public Destination(bool enabled, string basePath, string outputDirectory)
        {
            Enabled = true;
            BasePath = basePath;
            OutputDirectory = outputDirectory;
        }
    }
}

