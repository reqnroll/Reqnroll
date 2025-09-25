#nullable enable
using Reqnroll.Generator.Interfaces;
using System;
using System.Collections.Generic;

namespace Reqnroll.Tools.MsBuild.Generation;

public class FeatureFileCodeBehindGenerator(IReqnrollTaskLoggingHelper log, ReqnrollProjectInfo reqnrollProjectInfo, ITestGenerator testGenerator) : IFeatureFileCodeBehindGenerator
{
    public IReqnrollTaskLoggingHelper Log { get; } = log ?? throw new ArgumentNullException(nameof(log));

    public IEnumerable<FeatureFileCodeBehindGeneratorResult> GenerateFilesForProject()
    {
        var codeBehindWriter = new GeneratedFileWriter(Log);

        foreach (var featureFile in reqnrollProjectInfo.FeatureFiles)
        {
            var featureFileInput = CreateFeatureFileInput(featureFile);
            var codeBehindFileFullPath = reqnrollProjectInfo.GetFullPathAndNormalize(featureFile.CodeBehindFilePath);
            var messagesFileFullPath = reqnrollProjectInfo.GetFullPathAndNormalize(featureFile.MessagesFilePath);

            Log.LogTaskDiagnosticMessage($"Processing {featureFile.FeatureFilePath} ({reqnrollProjectInfo.GetFullPathAndNormalize(featureFile.FeatureFilePath)})");
            Log.LogTaskDiagnosticMessage($"  Code-behind: {featureFile.CodeBehindFilePath} ({codeBehindFileFullPath})");
            Log.LogTaskDiagnosticMessage($"  Messages: {featureFile.MessagesFilePath} ({messagesFileFullPath})");

            var generatorResult = testGenerator.GenerateTestFile(featureFileInput, new GenerationSettings());

            if (!generatorResult.Success)
            {
                foreach (var error in generatorResult.Errors)
                    Log.LogUserError(error.Message, featureFile.FeatureFilePath, error.Line, error.LinePosition);
                continue;
            }

            foreach (var warning in generatorResult.Warnings)
                Log.LogUserWarning(warning);

            codeBehindWriter.WriteGeneratedFile(
                codeBehindFileFullPath, generatorResult.GeneratedTestCode);

            bool containsMessages = !String.IsNullOrEmpty(generatorResult.FeatureMessages);
            if (containsMessages)
            {
                // If Feature-level Cucumber Messages were emitted by the code generator
                // Save them in the 'obj' directory in a sub-folder structure that mirrors the location of the feature file relative to the project root folder.
                codeBehindWriter.WriteGeneratedFile(messagesFileFullPath, generatorResult.FeatureMessages);
            }

            yield return new FeatureFileCodeBehindGeneratorResult(
                codeBehindFileFullPath,
                containsMessages ? generatorResult.FeatureMessagesResourceName : null,
                containsMessages ? messagesFileFullPath : null);
        }
    }

    private FeatureFileInput CreateFeatureFileInput(ReqnrollFeatureFileInfo featureFile)
    {
        return new FeatureFileInput(featureFile.FeatureFilePath);
    }
}