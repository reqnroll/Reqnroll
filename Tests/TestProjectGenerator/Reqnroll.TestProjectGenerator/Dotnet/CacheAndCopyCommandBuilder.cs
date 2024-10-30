using System;
using System.Collections.Concurrent;
using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public class CacheAndCopyCommandBuilder : CommandBuilder
{
    private const string TemplateName = "TName";
    private readonly CommandBuilder _baseCommandBuilder;
    private readonly string _targetPath;
    private readonly string _nameToReplace;
    private static readonly ConcurrentDictionary<string, object> LockObjects = new();

    public CacheAndCopyCommandBuilder(IOutputWriter outputWriter, CommandBuilder baseCommandBuilder, string targetPath, string nameToReplace = null) 
        : base(outputWriter, baseCommandBuilder.ExecutablePath, baseCommandBuilder.ArgumentsFormat, baseCommandBuilder.WorkingDirectory)
    {
        _baseCommandBuilder = baseCommandBuilder;
        _targetPath = targetPath;
        _nameToReplace = nameToReplace;
    }

    private string CalculateCacheTargetPath(string suffix = "")
    {
        var targetPathInfo = new DirectoryInfo(_targetPath);
        var directoryName = targetPathInfo.Name;
        string argsCleaned = ArgumentsFormat.Replace(_targetPath, "").Replace(" ", "").Replace("\"", "").Replace("/", "") + directoryName;
        if (_nameToReplace != null)
        {
            argsCleaned = argsCleaned.Replace(_nameToReplace, TemplateName);
            directoryName = directoryName.Replace(_nameToReplace, TemplateName);
        }
        return Path.Combine(Path.GetTempPath(), "RRC", $"RRC_{argsCleaned}{suffix}", directoryName);
    }

    public override CommandResult Execute(Func<Exception, Exception> exceptionFunction)
    {
        var cachePath = CalculateCacheTargetPath();

        CommandResult originalResult = null;
        if (!Directory.Exists(cachePath))
        {
            LockObjects.TryAdd(cachePath, new object());

            lock (LockObjects[cachePath])
            {
                if (!Directory.Exists(cachePath))
                {
                    var tempPath = CalculateCacheTargetPath($"-tmp{Guid.NewGuid():N}");
                    var arguments = ArgumentsFormat.Replace(_targetPath, tempPath);
                    if (_nameToReplace != null) arguments = arguments.Replace(_nameToReplace, TemplateName);
                    var commandBuilder = new CommandBuilder(_outputWriter, ExecutablePath, arguments, WorkingDirectory);

                    originalResult = commandBuilder.Execute(exceptionFunction);
                    try
                    {
                        if (!Directory.Exists(cachePath))
                            Directory.Move(Path.Combine(tempPath, ".."), Path.Combine(cachePath, ".."));
                    }
                    catch (IOException ex)
                    {
                        _outputWriter.WriteLine($"Unable to move TMP to CACHE: {ex.Message}");
                    }
                    try
                    {
                        if (Directory.Exists(tempPath))
                            Directory.Delete(Path.Combine(tempPath, ".."), true);
                    }
                    catch (IOException ex)
                    {
                        _outputWriter.WriteLine($"Unable to delete TMP: {ex.Message}");
                    }
                }
            }
        }

        var copyFolderCommandBuilder = new CopyFolderCommandBuilder(_outputWriter, cachePath, _targetPath, TemplateName, _nameToReplace);
        var copyFolderResult = copyFolderCommandBuilder.Execute(exceptionFunction);
        return originalResult == null ? copyFolderResult : 
            new CommandResult(originalResult.ExitCode, originalResult.ConsoleOutput + Environment.NewLine + copyFolderResult.ConsoleOutput);
    }
}
