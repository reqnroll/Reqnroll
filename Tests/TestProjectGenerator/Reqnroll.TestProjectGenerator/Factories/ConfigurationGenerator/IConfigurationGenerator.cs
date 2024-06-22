using Reqnroll.TestProjectGenerator.ConfigurationModel;
using Reqnroll.TestProjectGenerator.Data;

namespace Reqnroll.TestProjectGenerator.Factories.ConfigurationGenerator
{
    public interface IConfigurationGenerator
    {
        ProjectFile Generate(Configuration configuration);
    }
}
