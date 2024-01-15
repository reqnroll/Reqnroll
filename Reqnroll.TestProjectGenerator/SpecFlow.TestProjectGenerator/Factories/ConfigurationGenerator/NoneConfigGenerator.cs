using TechTalk.SpecFlow.TestProjectGenerator.ConfigurationModel;
using TechTalk.SpecFlow.TestProjectGenerator.Data;

namespace TechTalk.SpecFlow.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public class NoneConfigGenerator : IConfigurationGenerator
    {
        public ProjectFile Generate(Configuration configuration)
        {
            return null;
        }
    }
}
