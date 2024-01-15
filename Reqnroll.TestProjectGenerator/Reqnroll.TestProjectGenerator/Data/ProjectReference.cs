namespace Reqnroll.TestProjectGenerator.Data
{
    public class ProjectReference
    {
        public ProjectReference(string path, ProjectBuilder project)
        {
            Path = path;
            Project = project;
        }

        public string Path { get; }

        public ProjectBuilder Project { get; }
    }
}