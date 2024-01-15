using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public interface INuGetRestorer
    {
        void RestoreForProject(Project project);
    }
}