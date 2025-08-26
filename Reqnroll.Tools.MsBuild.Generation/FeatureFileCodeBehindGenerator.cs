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
            string outputPath,
            string intermediateOutputPath)
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

                yield return FileSystemHelper.GetRelativePath(resultedFile, projectFolder);

                if (!String.IsNullOrEmpty(generatorResult.FeatureNdjsonMessages))
                {
                    // If Feature-level Cucumber Messages were emitted by the code generator
                    // Save them in the 'obj' directory in a sub-folder structure that mirrors the location of the feature file relative to the project root folder.

                    // The value of 'obj' is passed from the .targets file as the IntermediateOutputPath property of the GenerateFeatureFileCodeBehindTask.
                    // It's value may be the $(BaseIntermediateOutputPath), ie, 'obj' or the $(IntermediateOutputPath), 'obj/<Configuration>/<TargetFramework>'


                    string relativeFeaturePath = FileSystemHelper.GetRelativePath(featureFile, projectFolder);
                    string relativeFeatureDir = Path.GetDirectoryName(relativeFeaturePath) ?? string.Empty;

                    string targetndjsonFilePath = Path.Combine(
                        projectFolder,
                        intermediateOutputPath,
                        relativeFeatureDir
                    );

                    string ndjsonFilename = Path.Combine(Path.GetFileNameWithoutExtension(targetFilePath) + ".ndjson");

                    string ndjsonFilePathAndName = Path.Combine(targetndjsonFilePath, ndjsonFilename);

                    _ = codeBehindWriter.WriteNdjsonFile(ndjsonFilePathAndName, ndjsonFilename, generatorResult);

                    // We interleave the names of the generated code-behind cs files with the names of the generated ndjson files for convenience
                    // These are sorted out in the GenerateFeatureFileCodeBehindTask by sorting on file extension.

                    yield return FileSystemHelper.GetRelativePath(ndjsonFilePathAndName, projectFolder);
                }

            }
        }
    }
}
