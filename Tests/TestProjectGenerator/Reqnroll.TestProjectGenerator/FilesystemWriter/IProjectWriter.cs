using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.FilesystemWriter
{
    public interface IProjectWriter
    {
        string WriteProject(DotNetSdkInfo sdk, Project project, string path);

        void WriteReferences(Project project, string projectFilePath);
    }
}
