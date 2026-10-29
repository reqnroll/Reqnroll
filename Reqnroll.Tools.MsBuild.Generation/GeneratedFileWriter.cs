using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation;

/// <summary>
/// Writes and deletes the generated files (code-behind, messages).
/// </summary>
/// <param name="log">The task logger.</param>
/// <param name="isExtendedLengthPathSupported">Checks whether the current process accepts extended-length (<c>\\?\</c>) paths. Defaults to <see cref="ExtendedLengthPathSupport.IsSupported"/>; can be overridden for testing.</param>
/// <param name="isWindows">Whether the current platform is Windows. Defaults to the current platform; can be overridden for testing.</param>
public class GeneratedFileWriter(IReqnrollTaskLoggingHelper log, Func<bool> isExtendedLengthPathSupported = null, bool? isWindows = null)
{
    /// <summary>
    /// Full paths with at least this length are treated as long paths. The Windows limit is 260 (MAX_PATH) for files,
    /// but only 248 for directories (MAX_PATH minus room for an 8.3 file name), so we use the lower one for both.
    /// </summary>
    internal const int LongPathThreshold = 248;

    private readonly Func<bool> _isExtendedLengthPathSupported = isExtendedLengthPathSupported ?? (() => ExtendedLengthPathSupport.IsSupported);
    private readonly bool _isWindows = isWindows ?? RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public void WriteGeneratedFile(string outputPath, string generatedFileContent)
    {
        log.LogTaskDiagnosticMessage($"Writing data to {outputPath}");
        WriteFile(outputPath, generatedFileContent);
    }

    public void DeleteGeneratedFile(string outputPath)
    {
        var path = GetFileSystemPath(outputPath);

        if (!File.Exists(path))
            return;

        log.LogTaskDiagnosticMessage($"Deleting {outputPath}");
        try
        {
            File.Delete(path);
        }
        catch (IOException ex)
        {
            log.LogTaskDiagnosticMessage($"Failed to delete {outputPath}: {ex.Message}.");
        }
    }

    private void WriteFile(string filePath, string content)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        var fileSystemDirectoryPath = GetFileSystemPath(directoryPath);
        if (!string.IsNullOrEmpty(fileSystemDirectoryPath) && !Directory.Exists(fileSystemDirectoryPath))
        {
            Directory.CreateDirectory(fileSystemDirectoryPath);
        }
        var fileSystemFilePath = GetFileSystemPath(filePath);
        WriteAllTextWithRetry(fileSystemFilePath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Returns the path to be used for file system operations. For long paths on Windows the extended-length path syntax
    /// (<c>\\?\</c>) is used if the current process supports it, otherwise the full path is returned unchanged.
    /// </summary>
    /// <remarks>
    /// The extended-length path syntax is only used for long paths, because it is not supported in every process:
    /// with .NET Framework legacy path handling (e.g. x86 MSBuild.exe 18.11 on some machines) such paths are rejected
    /// with "Illegal characters in path". See https://github.com/reqnroll/Reqnroll/issues/1128 for details.
    /// </remarks>
    internal string GetFileSystemPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        string fullPath = Path.GetFullPath(path);

        // Cross-platform: only apply extended syntax on Windows.
        if (!_isWindows)
            return fullPath;

        // Already device/extended syntax.
        if (fullPath.StartsWith(@"\\?\", StringComparison.Ordinal) ||
            fullPath.StartsWith(@"\\.\", StringComparison.Ordinal))
            return fullPath;

        // Short paths work without the extended syntax in every process.
        if (fullPath.Length < LongPathThreshold)
            return fullPath;

        if (!_isExtendedLengthPathSupported())
        {
            log.LogTaskDiagnosticMessage($"Long path '{fullPath}' cannot be converted to extended-length path syntax, because the current process does not support it.");
            return fullPath;
        }

        // UNC path.
        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
            return @"\\?\UNC\" + fullPath.Substring(2);

        // Drive-qualified path.
        return @"\\?\" + fullPath;
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
            catch (IOException ex) when (ex is not PathTooLongException)
            {
                if (i == maxAttempts)
                    throw;
                System.Threading.Thread.Sleep(i * 50);
            }
        }
    }
}
