using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.FilesystemWriter
{
    public interface IProjectWriter
    {
        string WriteProject(Project project, string path);

        void WriteReferences(Project project, string projectFilePath);
    }
}
