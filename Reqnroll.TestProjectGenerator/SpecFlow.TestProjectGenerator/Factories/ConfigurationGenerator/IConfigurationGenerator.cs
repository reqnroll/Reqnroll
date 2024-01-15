using TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public interface IConfigurationGenerator
    {
        ProjectFile Generate(Configuration configuration);
    }
}
