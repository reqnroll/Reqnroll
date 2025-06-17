#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Reqnroll.Formatters.Configuration;
using Reqnroll.Utils;

namespace Reqnroll.Formatters.PubSub
{
    public abstract class FileWritingFormatterPluginBase : FormatterPluginBase
    {
        private readonly string _defaultFileType;
        private readonly string _defaultFileName;
        private readonly IFileSystem _fileSystem;

        public FileWritingFormatterPluginBase(IFormattersConfiguration configuration, ICucumberMessageBroker broker, string pluginName, string defaultFileType, string defaultFileName, IFileSystem fileSystem) : base(configuration, broker, pluginName)
        {
            _defaultFileType = defaultFileType;
            _defaultFileName = defaultFileName;
            _fileSystem = fileSystem;
        }


        protected const int TUNING_PARAM_FILE_WRITE_BUFFER_SIZE = 65536;

        internal override void ConsumeAndFormatMessagesBackgroundTask(string formatterConfigurationString, Func<bool, Task> onInitialized)
        {
            string defaultBaseDirectory = $".{Path.DirectorySeparatorChar}";

            string outputFilePath = ParseConfigurationString(formatterConfigurationString, _pluginName);
            string fileName = Path.GetFileName(outputFilePath);
            string baseDirectory = Path.GetDirectoryName(outputFilePath);

            if (baseDirectory.IsNullOrEmpty())
                baseDirectory = defaultBaseDirectory;

            if (fileName.IsNullOrEmpty())
                fileName = _defaultFileName;

            if (!fileName.EndsWith(_defaultFileType, StringComparison.OrdinalIgnoreCase))
            {
                fileName += _defaultFileType;
            }

            string outputPath = Path.Combine(baseDirectory, fileName);
            var validFile = FileFilter.GetValidFiles([outputPath]).Count == 1;
            if (!validFile)
            {
                onInitialized(false);
                throw new InvalidOperationException($"Path of configured formatter output file: {outputPath} is invalid or missing.");
            }

            if (!_fileSystem.DirectoryExists(baseDirectory))
            {
                _fileSystem.CreateDirectory(baseDirectory);
            }

            onInitialized(true);
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
