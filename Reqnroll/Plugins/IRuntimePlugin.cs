using Reqnroll.UnitTestProvider;

namespace Reqnroll.Plugins
{
    public interface IRuntimePlugin
    {
        void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration);
    }
}
