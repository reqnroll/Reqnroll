using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public interface INuGetRestorer
    {
        void RestoreForProject(Project project);
    }
}