using Reqnroll.Plugins;

namespace Reqnroll.Formatters.PubSub;

public interface ICucumberMessagePublisher
{
    void Initialize(RuntimePluginEvents runtimePluginEvents);
}