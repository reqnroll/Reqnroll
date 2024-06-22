using Reqnroll.TestProjectGenerator;

namespace Reqnroll.TestProjectGenerator.Cli
{
    public interface IProjectContentGenerator
    {
        void Generate(ProjectBuilder pb);
    }
}
