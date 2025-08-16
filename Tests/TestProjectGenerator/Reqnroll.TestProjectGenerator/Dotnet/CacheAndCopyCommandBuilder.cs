using Reqnroll.TestProjectGenerator.FilesystemWriter;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Reqnroll.TestProjectGenerator.Dotnet;

public class CacheAndCopyCommandBuilder : CommandBuilder
{
    private const string TemplateName = "TName";
    private readonly DotNetSdkInfo _sdk;
    private readonly CommandBuilder _baseCommandBuilder;
    private readonly string _targetPath;
    private readonly string _nameToReplace;
    private static readonly ConcurrentDictionary<string, object> LockObjects = new();

    public CacheAndCopyCommandBuilder(IOutputWriter outputWriter, DotNetSdkInfo sdk, CommandBuilder baseCommandBuilder, string targetPath, string nameToReplace = null)
        : base(outputWriter, baseCommandBuilder.ExecutablePath, baseCommandBuilder.ArgumentsFormat, baseCommandBuilder.WorkingDirectory)
    {
        _sdk = sdk;
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

        var sdkSpecifier = _sdk == null ? "" : $"_{_sdk.Version}";
        return Path.Combine(new ConfigurationDriver().TempFolderPath, "RRC", $"RRC{sdkSpecifier}_{argsCleaned}{suffix}", directoryName);
    }

    public override CommandResult Execute(Func<Exception, Exception> exceptionFunction)
    {
        var cachePath = CalculateCacheTargetPath();
        var cacheContainerPath = Path.Combine(cachePath, "..");

        CommandResult originalResult = null;
        if (!Directory.Exists(cacheContainerPath))
            originalResult = ExecuteOriginal(false);

        var copyFolderCommandBuilder = new CopyFolderCommandBuilder(_outputWriter, cachePath, _targetPath, TemplateName, _nameToReplace);
        var copyFolderResult = copyFolderCommandBuilder.Execute(exceptionFunction);
        if (copyFolderResult.ExitCode != 0)
        {
            // the files cleaned up by the OS, so we need to re-run the original command
            _outputWriter.WriteLine("Retry generating cached folder because of copy error.");
            originalResult = ExecuteOriginal(true);
            copyFolderResult = copyFolderCommandBuilder.Execute(exceptionFunction);
        }

        return originalResult == null ? copyFolderResult : 
            new CommandResult(originalResult.ExitCode, originalResult.ConsoleOutput + Environment.NewLine + copyFolderResult.ConsoleOutput);

        CommandResult ExecuteOriginal(bool force)
        {
            LockObjects.TryAdd(cacheContainerPath, new object());

            lock (LockObjects[cacheContainerPath])
            {
                if (force || !Directory.Exists(cacheContainerPath))
                {
                    var tempPath = CalculateCacheTargetPath($"-tmp{Guid.NewGuid():N}");
                    var tempContainerPath = Path.GetFullPath(Path.Combine(tempPath, ".."));
                    var arguments = ArgumentsFormat.Replace(_targetPath, tempPath);
                    if (_nameToReplace != null) arguments = arguments.Replace(_nameToReplace, TemplateName);
                    var commandBuilder = new CommandBuilder(_outputWriter, ExecutablePath, arguments, WorkingDirectory);

                    originalResult = commandBuilder.Execute(exceptionFunction);
                    try
                    {
                        bool cacheTargetExists = Directory.Exists(cacheContainerPath);
                        if (cacheTargetExists && force)
                        {
                            Directory.Delete(cacheContainerPath, true);
                            cacheTargetExists = false;
                        }

                        if (!cacheTargetExists)
                            Directory.Move(tempContainerPath, cacheContainerPath);
                    }
                    catch (IOException ex)
                    {
                        _outputWriter.WriteLine($"Unable to move TMP to CACHE: {ex.Message}");
                    }
                    try
                    {
                        if (Directory.Exists(tempContainerPath))
                            Directory.Delete(tempContainerPath, true);
                    }
                    catch (IOException ex)
                    {
                        _outputWriter.WriteLine($"Unable to delete TMP: {ex.Message}");
                    }
                }
            }

            return originalResult;
        }
    }
}
