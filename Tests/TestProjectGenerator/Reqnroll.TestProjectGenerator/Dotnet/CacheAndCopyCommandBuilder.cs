using System;
using System.Collections.Concurrent;
using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public class CacheAndCopyCommandBuilder : CommandBuilder
{
    private readonly CommandBuilder _baseCommandBuilder;
    private readonly string _targetPath;
    private static readonly ConcurrentDictionary<string, object> LockObjects = new();

    public CacheAndCopyCommandBuilder(IOutputWriter outputWriter, CommandBuilder baseCommandBuilder, string targetPath) : base(outputWriter, baseCommandBuilder.ExecutablePath, baseCommandBuilder.ArgumentsFormat, baseCommandBuilder.WorkingDirectory)
    {
        _baseCommandBuilder = baseCommandBuilder;
        _targetPath = targetPath;
    }

    private string CalculateCacheTargetPath(string suffix = "")
    {
        var targetPathInfo = new DirectoryInfo(_targetPath);
        var directoryName = targetPathInfo.Name;
        string argsCleaned = ArgumentsFormat.Replace(_targetPath, "").Replace(" ", "").Replace("\"", "")+directoryName;
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
                    var commandBuilder = new CommandBuilder(_outputWriter, ExecutablePath, ArgumentsFormat.Replace(_targetPath, tempPath), WorkingDirectory);

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

        var copyFolderCommandBuilder = new CopyFolderCommandBuilder(_outputWriter, cachePath, _targetPath);
        var copyFolderResult = copyFolderCommandBuilder.Execute(exceptionFunction);
        return originalResult == null ? copyFolderResult : 
            new CommandResult(originalResult.ExitCode, originalResult.ConsoleOutput + Environment.NewLine + copyFolderResult.ConsoleOutput);
    }
}
