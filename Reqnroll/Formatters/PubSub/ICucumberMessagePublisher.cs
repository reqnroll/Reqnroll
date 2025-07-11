using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.Formatters.PubSub
{
    public interface ICucumberMessagePublisher
    {
        void Initialize(RuntimePluginEvents runtimePluginEvents);
    }
}