#nullable enable

using System;
using System.IO;
using Reqnroll.CucumberMessages.Configuration;
using Reqnroll.Utils;

namespace Reqnroll.CucumberMessages.PubSub
{
    public abstract class FileWritingFormatterPluginBase : FormatterPluginBase
    {
        private readonly string _defaultFileType;
        private readonly string _defaultFileName;
        private readonly IFileSystem _fileSystem;

        public FileWritingFormatterPluginBase(ICucumberMessagesConfiguration configuration, string pluginName, string defaultFileType, string defaultFileName, IFileSystem fileSystem) : base(configuration, pluginName)
        {
            _defaultFileType = defaultFileType;
            _defaultFileName = defaultFileName;
            _fileSystem = fileSystem;
        }


        protected const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;

        internal override void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigurationString)
        {
            string outputFilePath = ParseConfigurationString(formatterConfigurationString, _pluginName);

            if (String.IsNullOrEmpty(outputFilePath))
                outputFilePath = $".{Path.DirectorySeparatorChar}{_defaultFileName}";

            string baseDirectory = Path.GetDirectoryName(outputFilePath);
            var validFile = FileFilter.GetValidFiles([outputFilePath]).Count == 1;
            if (string.IsNullOrEmpty(baseDirectory) || !validFile)
            {
                throw new InvalidOperationException($"Path of configured output Messages file: {outputFilePath} is invalid or missing.");
            }

            if (!_fileSystem.DirectoryExists(baseDirectory))
            {
                _fileSystem.CreateDirectory(baseDirectory);
            }


            string fileName = Path.GetFileName(outputFilePath);
            if (!fileName.EndsWith(_defaultFileType, StringComparison.OrdinalIgnoreCase))
            {
                fileName += _defaultFileType;
            }

            string outputPath = Path.Combine(baseDirectory, fileName);
            ConsumeAndWriteToFilesBackgroundTask(outputPath);
        }

        internal abstract void ConsumeAndWriteToFilesBackgroundTask(string outputPath);

        internal string ParseConfigurationString(string messagesConfiguration, string pluginName)
        {
            if (string.IsNullOrWhiteSpace(messagesConfiguration))
                return string.Empty;

            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(messagesConfiguration);
                if (document.RootElement.TryGetProperty("outputFilePath", out var outputFilePathElement))
                {
                    return outputFilePathElement.GetString() ?? string.Empty;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                trace?.WriteToolOutput($"Configuration of ${pluginName} is invalid: ${messagesConfiguration}");
            }

            return string.Empty;
        }
    }
}
