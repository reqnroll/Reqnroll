using System;
using System.IO;
using System.Text;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation;

public class GeneratedFileWriter(IReqnrollTaskLoggingHelper log)
{
    public void WriteGeneratedFile(string outputPath, string generatedFileContent)
    {
        log.LogTaskDiagnosticMessage($"Writing data to {outputPath}");
        WriteFile(outputPath, generatedFileContent);
    }

    public void DeleteGeneratedFile(string outputPath)
    {
        var path = ChangePathToSupportLongPaths(outputPath);

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
        var longDirPath = ChangePathToSupportLongPaths(directoryPath);
        if (!string.IsNullOrEmpty(longDirPath) && !Directory.Exists(longDirPath))
        {
            Directory.CreateDirectory(longDirPath);
        }
        var longPath = ChangePathToSupportLongPaths(filePath);
        WriteAllTextWithRetry(longPath, content, Encoding.UTF8);
    }

    private static string ChangePathToSupportLongPaths(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        string fullPath = Path.GetFullPath(path);

        // Cross-platform: only apply extended syntax on Windows.
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return fullPath;
        }

        // Already device/extended syntax.
        if (fullPath.StartsWith(@"\\?\", StringComparison.Ordinal) ||
            fullPath.StartsWith(@"\\.\", StringComparison.Ordinal))
            return fullPath;

        // UNC longDirPath.
        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
            return @"\\?\UNC\" + fullPath.Substring(2);

        // Drive-qualified longDirPath.
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
            catch (IOException)
            {
                if (i == maxAttempts)
                    throw;
                System.Threading.Thread.Sleep(i * 50);
            }
        }
    }
}