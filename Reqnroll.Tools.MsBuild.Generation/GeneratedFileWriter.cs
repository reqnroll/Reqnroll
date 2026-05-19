using System;
using System.IO;
using System.Text;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation;

public class GeneratedFileWriter(IReqnrollTaskLoggingHelper log)
{
    public void WriteGeneratedFile(string outputPath, string generatedFileContent)
    {
        var path = ChangePathToSupportLongPaths(outputPath);
        log.LogTaskDiagnosticMessage($"Writing data to {outputPath}");
        WriteFile(path, generatedFileContent);
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
        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        WriteAllTextWithRetry(filePath, content, Encoding.UTF8);
    }

    private static string ChangePathToSupportLongPaths(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path must not be null or empty.", nameof(path));

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

        // UNC path.
        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
            return @"\\?\UNC\" + fullPath.Substring(2);

        // Drive-qualified path.
        return @"\\?\" + fullPath;
    }

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