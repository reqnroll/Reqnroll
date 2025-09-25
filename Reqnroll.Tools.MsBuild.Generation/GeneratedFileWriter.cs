using System.IO;
using System.Text;
using Reqnroll.Utils;

namespace Reqnroll.Tools.MsBuild.Generation;

/// <summary>
/// This class is going to be obsolete once we implement MsBuild level up-to-date checks.
/// </summary>
public class GeneratedFileWriter(IReqnrollTaskLoggingHelper log)
{
    public void WriteGeneratedFile(string outputPath, string generatedFileContent)
    {
        log.LogTaskDiagnosticMessage($"Writing data to {outputPath}");
        WriteFileIfChanged(outputPath, generatedFileContent);
    }

    private void WriteFileIfChanged(string filePath, string content)
    {
        if (File.Exists(filePath))
        {
            if (!FileSystemHelper.FileCompareContent(filePath, content))
            {
                WriteAllTextWithRetry(filePath, content, Encoding.UTF8);
            }
        }
        else
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            WriteAllTextWithRetry(filePath, content, Encoding.UTF8);
        }
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