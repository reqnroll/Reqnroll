#nullable enable
using Reqnroll.Generator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Reqnroll.Tools.MsBuild.Generation;

public class FeatureFileCodeBehindGenerator(IReqnrollTaskLoggingHelper log, ReqnrollProjectInfo reqnrollProjectInfo, ITestGenerator testGenerator) : IFeatureFileCodeBehindGenerator
{
    public IReqnrollTaskLoggingHelper Log { get; } = log ?? throw new ArgumentNullException(nameof(log));

    public IEnumerable<FeatureFileCodeBehindGeneratorResult> GenerateFilesForProject()
    {
        var codeBehindWriter = new GeneratedFileWriter(Log);

        foreach (var featureFile in reqnrollProjectInfo.FeatureFiles)
        {
            FeatureFileCodeBehindGeneratorResult? result;
            try
            {
                result = GenerateFilesForFeatureFile(featureFile, codeBehindWriter);
            }
            catch (PathTooLongException ex)
            {
                // Long paths are not supported by every build process (e.g. with .NET Framework legacy path handling),
                // see https://github.com/reqnroll/Reqnroll/issues/1128
                Log.LogUserError(
                    $"The path of a file generated for this feature file is too long and long paths are not supported by the current build process ({ex.Message}). " +
                    "Use a shorter project path, build with 'dotnet build' or the 64-bit MSBuild.exe, " +
                    "or set 'ReqnrollUseIntermediateOutputPathForCodeBehind' to change the location of the generated files.",
                    featureFile.FeatureFilePath, 0, 0);
                continue;
            }

            if (result != null)
                yield return result;
        }
    }

    private FeatureFileCodeBehindGeneratorResult? GenerateFilesForFeatureFile(ReqnrollFeatureFileInfo featureFile, GeneratedFileWriter codeBehindWriter)
    {
        var codeBehindFileFullPath = reqnrollProjectInfo.GetFullPathAndNormalize(featureFile.CodeBehindFilePath);
        var messagesFileFullPath = reqnrollProjectInfo.GetFullPathAndNormalize(featureFile.MessagesFilePath);

        var featureFileInput = CreateFeatureFileInput(featureFile, codeBehindFileFullPath);

        var linkMessage = featureFile.FeatureFileLink != null ? $", linked as {featureFile.FeatureFileLink}" : string.Empty;
        Log.LogTaskDiagnosticMessage($"Processing {featureFile.FeatureFilePath} ({reqnrollProjectInfo.GetFullPathAndNormalize(featureFile.FeatureFilePath)}{linkMessage})");
        Log.LogTaskDiagnosticMessage($"  Code-behind: {featureFile.CodeBehindFilePath} ({codeBehindFileFullPath})");
        Log.LogTaskDiagnosticMessage($"  Messages: {featureFile.MessagesFilePath} ({messagesFileFullPath}), Resource name: {featureFile.MessagesResourceName}");

        var generatorResult = testGenerator.GenerateTestFile(featureFileInput, new GenerationSettings());

        if (!generatorResult.Success)
        {
            foreach (var error in generatorResult.Errors)
                Log.LogUserError(error.Message, featureFile.FeatureFilePath, error.Line, error.LinePosition);
            return null;
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
        else
        {
            codeBehindWriter.DeleteGeneratedFile(messagesFileFullPath);
        }

        return new FeatureFileCodeBehindGeneratorResult(
            codeBehindFileFullPath,
            containsMessages ? generatorResult.FeatureMessagesResourceName : null,
            containsMessages ? messagesFileFullPath : null);
    }

    private FeatureFileInput CreateFeatureFileInput(ReqnrollFeatureFileInfo featureFile, string codeBehindFileFullPath)
    {
        var featureFileInput = new FeatureFileInput(featureFile.FeatureFilePath);
        featureFileInput.MessagesResourceName = featureFile.MessagesResourceName; // Move this to the constructor of FeatureFileInput in v4
        featureFileInput.CodeBehindFilePath = codeBehindFileFullPath; // Move this to the constructor of FeatureFileInput in v4
        featureFileInput.FeatureFileLink = featureFile.FeatureFileLink; // Move this to the constructor of FeatureFileInput in v4
        return featureFileInput;
    }
}