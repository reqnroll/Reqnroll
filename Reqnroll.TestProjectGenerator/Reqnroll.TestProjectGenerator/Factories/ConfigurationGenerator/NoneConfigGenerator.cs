using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public class NoneConfigGenerator : IConfigurationGenerator
    {
        public ProjectFile Generate(Configuration configuration)
        {
            return null;
        }
    }
}
