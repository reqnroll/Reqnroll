using TechTalk.SpecFlow.TestProjectGenerator;

namespace SpecFlow.TestProjectGenerator.Cli
{
    public interface IProjectContentGenerator
    {
        void Generate(ProjectBuilder pb);
    }
}
