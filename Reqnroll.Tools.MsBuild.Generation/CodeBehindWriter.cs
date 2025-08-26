using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Build.Utilities;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class CodeBehindWriter
    {
        public CodeBehindWriter(TaskLoggingHelper log)
        {
            Log = log;
        }

        public TaskLoggingHelper Log { get; }

        public string WriteCodeBehindFile(string outputPath, string featureFile, TestFileGeneratorResult testFileGeneratorResult) //todo needs unit tests
        {
            if (string.IsNullOrEmpty(testFileGeneratorResult.Filename))
            {
                Log?.LogWithNameTag(Log.LogError, $"{featureFile} has no generated filename");
                return null;
            }

            string directoryPath = Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException();
            Log?.LogWithNameTag(Log.LogMessage, directoryPath);

            Log?.LogWithNameTag(Log.LogMessage, $"Writing data to {outputPath}; path = {directoryPath}; generatedFilename = {testFileGeneratorResult.Filename}");

            WriteFileIfChanged(outputPath, directoryPath, testFileGeneratorResult.GeneratedTestCode);

            return outputPath;
        }

        internal object WriteNdjsonFile(string targetNdjsonFilePath, string ndjsonFilename, TestFileGeneratorResult generatorResult)
        {
            Log?.LogWithNameTag(Log.LogMessage, $"Writing ndjson to {targetNdjsonFilePath}");
            var directoryPath = Path.GetDirectoryName(targetNdjsonFilePath);

            WriteFileIfChanged(targetNdjsonFilePath, directoryPath, generatorResult.FeatureNdjsonMessages);

            return null;
        }

        private void WriteFileIfChanged(string filePath, string directoryPath, string content)
        {
            if (File.Exists(filePath))
            {
                if (!FileSystemHelper.FileCompareContent(filePath, content))
                {
                    WriteAllTextWithRetry(filePath, content, Encoding.UTF8);
                }
            }
            else
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                WriteAllTextWithRetry(filePath, content, Encoding.UTF8);
            }
        }

        /// <summary>
        /// When building a multi-targeted project, the build system may try to write the same file multiple times,
        /// and this can cause an IOException ("The process cannot access the file because it is being used by another process.").
        /// See https://github.com/reqnroll/Reqnroll/issues/197
        /// Once we move to Roslyn-based generation, this problem will go away, but for now, we use a workaround of
        /// retrying the write operation a few times (the content is anyway the same).
        /// </summary>
        private void WriteAllTextWithRetry(string path, string contents, Encoding encoding)
        {
            const int maxAttempts = 5;
            for (int i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    File.WriteAllText(path, contents, encoding);
                    return;
                }
                catch (IOException)
                {
                    if (i == maxAttempts)
                        throw;
                    System.Threading.Thread.Sleep(i * 50);
                }
            }
        }
    }
}
