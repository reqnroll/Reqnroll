using Microsoft.Build.Utilities;
using Reqnroll.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Reqnroll.Tools.MsBuild.Generation
{
    public class FeatureFileCodeBehindGenerator : IFeatureFileCodeBehindGenerator
    {
        private readonly FilePathGenerator _filePathGenerator;
        private readonly FeatureCodeBehindGenerator _featureCodeBehindGenerator;

        public FeatureFileCodeBehindGenerator(TaskLoggingHelper log, FeatureCodeBehindGenerator featureCodeBehindGenerator)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
            _featureCodeBehindGenerator = featureCodeBehindGenerator;
            _filePathGenerator = new FilePathGenerator();
        }

        public TaskLoggingHelper Log { get; }

        public IEnumerable<string> GenerateFilesForProject(
            IReadOnlyCollection<string> featureFiles,
            string projectFolder,
            string outputPath)
        {
            var codeBehindWriter = new CodeBehindWriter(null);

            if (featureFiles == null)
            {
                yield break;
            }

            foreach (var featureFile in featureFiles)
            {
                string featureFileItemSpec = featureFile;
                var generatorResult = _featureCodeBehindGenerator.GenerateCodeBehindFile(featureFileItemSpec);

                if (!generatorResult.Success)
                {
                    foreach (var error in generatorResult.Errors)
                    {
                        Log.LogError(
                            null,
                            null,
                            null,
                            featureFile,
                            error.Line,
                            error.LinePosition,
                            0,
                            0,
                            error.Message);
                    }

                    continue;
                }

                foreach (var warning in generatorResult.Warnings)
                {
                    Log.LogWarning(warning);
                }
                string targetFilePath = _filePathGenerator.GenerateFilePath(
                    projectFolder,
                    outputPath,
                    featureFile,
                    generatorResult.Filename);

                string resultedFile = codeBehindWriter.WriteCodeBehindFile(targetFilePath, featureFile, generatorResult);

                if (generatorResult.FeatureNdjsonMessages != null && generatorResult.FeatureNdjsonMessages.Count() > 0)
                {
                    string ndjsonFilename = Path.Combine(
                        Path.GetDirectoryName(targetFilePath) ?? string.Empty,
                        Path.GetFileNameWithoutExtension(targetFilePath) + ".ndjson"
                    );
                    File.WriteAllLines(ndjsonFilename, generatorResult.FeatureNdjsonMessages, Encoding.UTF8);
                }


                yield return FileSystemHelper.GetRelativePath(resultedFile, projectFolder);
            }
        }
    }
}
