using System;
using System.IO;
using System.Threading;

namespace Reqnroll.TestProjectGenerator.Helpers;
public class FolderCleaner
{
    private const int MaxTestRunAgeMinutes = 60;
    private const int MaxTestRunsToKeep = 2*8; // allow safely running tests with 8 test processes

    private static int _oldFoldersCleaned = 0;
    private readonly Folders _folders;
    private readonly TestProjectFolders _testProjectFolders;
    private readonly IOutputWriter _outputWriter;

    public FolderCleaner(Folders folders, TestProjectFolders testProjectFolders, IOutputWriter outputWriter)
    {
        _folders = folders;
        _testProjectFolders = testProjectFolders;
        _outputWriter = outputWriter;
    }

    public void EnsureOldRunFoldersCleaned()
    {
        if (Interlocked.CompareExchange(ref _oldFoldersCleaned, 1, 0) != 0 ||
            !Directory.Exists(_folders.FolderToSaveGeneratedSolutions))
            return;

        CleanOldRunFolders();
    }

    private void CleanOldRunFolders()
    {
        int counter = 0;
        var deleteBefore = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(MaxTestRunAgeMinutes));
        var deletedCount = FileSystemHelper.DeleteFolderContent(
            _folders.FolderToSaveGeneratedSolutions, 
            f => f.LastWriteTimeUtc < deleteBefore || (++counter > MaxTestRunsToKeep),
            ignoreErrors: true);
        if (deletedCount > 0)
            _outputWriter.WriteLine($"{deletedCount} old run folder cleaned up");
    }

    public void CleanSolutionFolder()
    {
        FileSystemHelper.DeleteFolder(_testProjectFolders.PathToSolutionDirectory);
    }
}
