using Reqnroll.Plugins;

namespace Reqnroll.Formatters.PubSub;

public interface IFormattersPublisher
{
    void Initialize(RuntimePluginEvents runtimePluginEvents);
}